using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Excecoes;
using ConectaFranquias.Api.Extensoes;
using ConectaFranquias.Api.Mapeamentos;
using ConectaFranquias.Api.Repositories;
using ConectaFranquias.Api.Services.Seguranca;

namespace ConectaFranquias.Api.Services;

public interface IServicoUsuario
{
    Task<LoginResposta> AutenticarAsync(LoginRequisicao requisicao, CancellationToken cancelamento = default);

    Task<ResultadoPaginado<UsuarioResposta>> BuscarAsync(FiltroUsuarios filtro, CancellationToken cancelamento = default);

    Task<UsuarioResposta> ObterPorIdAsync(int usuarioId, CancellationToken cancelamento = default);

    Task<UsuarioResposta> CriarAsync(CriarUsuarioRequisicao requisicao, CancellationToken cancelamento = default);

    Task<UsuarioResposta> AtualizarAsync(int usuarioId, AtualizarUsuarioRequisicao requisicao, CancellationToken cancelamento = default);

    Task InativarAsync(int usuarioId, CancellationToken cancelamento = default);

    Task ReativarAsync(int usuarioId, CancellationToken cancelamento = default);
}

public class ServicoUsuario(
    IRepositorioUsuario repositorioUsuario,
    IRepositorioUnidade repositorioUnidade,
    IUnidadeDeTrabalho unidadeDeTrabalho,
    IServicoHashSenha servicoHashSenha,
    IGeradorTokenJwt geradorToken,
    IContextoUsuario contextoUsuario) : IServicoUsuario
{
    public async Task<LoginResposta> AutenticarAsync(
        LoginRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var usuario = await repositorioUsuario.ObterPorEmailAsync(requisicao.Email, cancelamento);

        // A mesma mensagem para e-mail inexistente e senha errada evita revelar
        // quais e-mails estão cadastrados.
        if (usuario is null || !servicoHashSenha.SenhaConfere(requisicao.Senha, usuario.SenhaHash))
        {
            throw new ExcecaoAutorizacao("E-mail ou senha inválidos.");
        }

        if (!usuario.Ativo)
        {
            throw new ExcecaoAutorizacao("Este usuário está inativo. Procure o administrador da franqueadora.");
        }

        var (token, expiraEm) = geradorToken.Gerar(usuario);

        return new LoginResposta
        {
            Token = token,
            ExpiraEm = expiraEm,
            UsuarioId = usuario.UsuarioId,
            Nome = usuario.Nome,
            Perfil = usuario.Perfil.Tipo,
            UnidadeId = usuario.UnidadeId
        };
    }

    public async Task<ResultadoPaginado<UsuarioResposta>> BuscarAsync(
        FiltroUsuarios filtro,
        CancellationToken cancelamento = default)
    {
        filtro.UnidadeId = contextoUsuario.ResolverUnidadeDaConsulta(filtro.UnidadeId);

        var pagina = await repositorioUsuario.BuscarAsync(filtro, cancelamento);
        return pagina.ParaResultadoPaginado(filtro, MapeamentoDominio.ParaResposta);
    }

    public async Task<UsuarioResposta> ObterPorIdAsync(int usuarioId, CancellationToken cancelamento = default)
    {
        var usuario = await repositorioUsuario.ObterComRelacionamentosAsync(usuarioId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Usuário", usuarioId);

        if (usuario.UnidadeId.HasValue)
        {
            contextoUsuario.GarantirAcessoAUnidade(usuario.UnidadeId.Value);
        }
        else if (!contextoUsuario.EhAdministrador)
        {
            throw new ExcecaoAutorizacao("Somente administradores podem consultar usuários da franqueadora.");
        }

        return usuario.ParaResposta();
    }

    public async Task<UsuarioResposta> CriarAsync(
        CriarUsuarioRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        if (await repositorioUsuario.EmailJaCadastradoAsync(requisicao.Email, cancelamento: cancelamento))
        {
            throw new ExcecaoConflito($"Já existe um usuário cadastrado com o e-mail '{requisicao.Email}'.");
        }

        await GarantirUnidadeValidaAsync(requisicao.UnidadeId, cancelamento);

        var perfil = await repositorioUsuario.ObterPerfilPorTipoAsync(requisicao.Perfil, cancelamento)
            ?? throw new ExcecaoNegocio($"O perfil '{requisicao.Perfil}' não está configurado no sistema.");

        var usuario = new Usuario(
            requisicao.Nome.Trim(),
            requisicao.Email,
            servicoHashSenha.GerarHash(requisicao.Senha),
            perfil.PerfilId,
            requisicao.UnidadeId);

        await repositorioUsuario.AdicionarAsync(usuario, cancelamento);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterRespostaAtualizadaAsync(usuario.UsuarioId, cancelamento);
    }

    public async Task<UsuarioResposta> AtualizarAsync(
        int usuarioId,
        AtualizarUsuarioRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var usuario = await repositorioUsuario.ObterPorIdAsync(usuarioId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Usuário", usuarioId);

        if (await repositorioUsuario.EmailJaCadastradoAsync(requisicao.Email, usuarioId, cancelamento))
        {
            throw new ExcecaoConflito($"Já existe outro usuário cadastrado com o e-mail '{requisicao.Email}'.");
        }

        await GarantirUnidadeValidaAsync(requisicao.UnidadeId, cancelamento);

        var perfil = await repositorioUsuario.ObterPerfilPorTipoAsync(requisicao.Perfil, cancelamento)
            ?? throw new ExcecaoNegocio($"O perfil '{requisicao.Perfil}' não está configurado no sistema.");

        usuario.AtualizarDados(requisicao.Nome.Trim(), requisicao.Email, perfil.PerfilId, requisicao.UnidadeId);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterRespostaAtualizadaAsync(usuarioId, cancelamento);
    }

    public async Task InativarAsync(int usuarioId, CancellationToken cancelamento = default)
    {
        var usuario = await repositorioUsuario.ObterPorIdAsync(usuarioId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Usuário", usuarioId);

        if (usuarioId == contextoUsuario.UsuarioId)
        {
            throw new ExcecaoNegocio("Um usuário não pode inativar a própria conta.");
        }

        if (!usuario.Ativo)
        {
            throw new ExcecaoNegocio("Este usuário já está inativo.");
        }

        usuario.Inativar();
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);
    }

    public async Task ReativarAsync(int usuarioId, CancellationToken cancelamento = default)
    {
        var usuario = await repositorioUsuario.ObterPorIdAsync(usuarioId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Usuário", usuarioId);

        if (usuario.Ativo)
        {
            throw new ExcecaoNegocio("Este usuário já está ativo.");
        }

        usuario.Ativar();
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);
    }

    private async Task GarantirUnidadeValidaAsync(int? unidadeId, CancellationToken cancelamento)
    {
        if (unidadeId is not null && !await repositorioUnidade.ExisteAsync(unidadeId.Value, cancelamento))
        {
            throw new ExcecaoNaoEncontrado("Unidade", unidadeId.Value);
        }
    }

    private async Task<UsuarioResposta> ObterRespostaAtualizadaAsync(int usuarioId, CancellationToken cancelamento)
    {
        var usuario = await repositorioUsuario.ObterComRelacionamentosAsync(usuarioId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Usuário", usuarioId);

        return usuario.ParaResposta();
    }
}
