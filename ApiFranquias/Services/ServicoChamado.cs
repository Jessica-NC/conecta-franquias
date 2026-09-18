using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Excecoes;
using ConectaFranquias.Api.Extensoes;
using ConectaFranquias.Api.Mapeamentos;
using ConectaFranquias.Api.Repositories;
using ConectaFranquias.Api.Services.Seguranca;

namespace ConectaFranquias.Api.Services;

public interface IServicoChamado
{
    Task<ResultadoPaginado<ChamadoResumoResposta>> BuscarAsync(FiltroChamados filtro, CancellationToken cancelamento = default);

    Task<ChamadoResposta> ObterPorIdAsync(int chamadoId, CancellationToken cancelamento = default);

    Task<ChamadoResposta> AbrirAsync(AbrirChamadoRequisicao requisicao, CancellationToken cancelamento = default);

    Task<ChamadoResposta> AtualizarAsync(int chamadoId, AtualizarChamadoRequisicao requisicao, CancellationToken cancelamento = default);

    Task<ChamadoResposta> RegistrarInteracaoAsync(int chamadoId, RegistrarInteracaoRequisicao requisicao, CancellationToken cancelamento = default);

    Task<ChamadoResposta> EncerrarAsync(int chamadoId, EncerrarChamadoRequisicao requisicao, CancellationToken cancelamento = default);
}

public class ServicoChamado(
    IRepositorioChamado repositorioChamado,
    IRepositorioUnidade repositorioUnidade,
    IUnidadeDeTrabalho unidadeDeTrabalho,
    IContextoUsuario contextoUsuario) : IServicoChamado
{
    public async Task<ResultadoPaginado<ChamadoResumoResposta>> BuscarAsync(
        FiltroChamados filtro,
        CancellationToken cancelamento = default)
    {
        filtro.UnidadeId = contextoUsuario.ResolverUnidadeDaConsulta(filtro.UnidadeId);

        var pagina = await repositorioChamado.BuscarAsync(filtro, cancelamento);
        return pagina.ParaResultadoPaginado(filtro, item => item.ParaResumo());
    }

    public async Task<ChamadoResposta> ObterPorIdAsync(int chamadoId, CancellationToken cancelamento = default)
    {
        var chamado = await repositorioChamado.ObterComInteracoesAsync(chamadoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Chamado", chamadoId);

        contextoUsuario.GarantirAcessoAUnidade(chamado.UnidadeId);

        return chamado.ParaResposta();
    }

    public async Task<ChamadoResposta> AbrirAsync(
        AbrirChamadoRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        contextoUsuario.GarantirAcessoAUnidade(requisicao.UnidadeId);

        if (!await repositorioUnidade.ExisteAsync(requisicao.UnidadeId, cancelamento))
        {
            throw new ExcecaoNaoEncontrado("Unidade", requisicao.UnidadeId);
        }

        var protocolo = await repositorioChamado.GerarProtocoloAsync(cancelamento);

        var chamado = new ChamadoSuporte(
            requisicao.UnidadeId,
            contextoUsuario.UsuarioId,
            protocolo,
            requisicao.Titulo.Trim(),
            requisicao.Descricao.Trim(),
            requisicao.Categoria,
            requisicao.Prioridade);

        await repositorioChamado.AdicionarAsync(chamado, cancelamento);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterPorIdAsync(chamado.ChamadoSuporteId, cancelamento);
    }

    public async Task<ChamadoResposta> AtualizarAsync(
        int chamadoId,
        AtualizarChamadoRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var chamado = await repositorioChamado.ObterPorIdAsync(chamadoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Chamado", chamadoId);

        contextoUsuario.GarantirAcessoAUnidade(chamado.UnidadeId);

        chamado.AtualizarClassificacao(
            requisicao.Titulo.Trim(),
            requisicao.Descricao.Trim(),
            requisicao.Categoria,
            requisicao.Prioridade);

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterPorIdAsync(chamadoId, cancelamento);
    }

    public async Task<ChamadoResposta> RegistrarInteracaoAsync(
        int chamadoId,
        RegistrarInteracaoRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var chamado = await repositorioChamado.ObterComInteracoesAsync(chamadoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Chamado", chamadoId);

        contextoUsuario.GarantirAcessoAUnidade(chamado.UnidadeId);

        // Encerrar exige a solução aplicada; para isso existe o endpoint dedicado.
        if (requisicao.NovaSituacao == SituacaoChamado.Encerrado)
        {
            throw new ExcecaoNegocio(
                "Para encerrar o chamado utilize o endpoint de encerramento, informando a solução aplicada.");
        }

        chamado.RegistrarInteracao(contextoUsuario.UsuarioId, requisicao.Mensagem.Trim(), requisicao.NovaSituacao);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterPorIdAsync(chamadoId, cancelamento);
    }

    public async Task<ChamadoResposta> EncerrarAsync(
        int chamadoId,
        EncerrarChamadoRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var chamado = await repositorioChamado.ObterComInteracoesAsync(chamadoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Chamado", chamadoId);

        contextoUsuario.GarantirAcessoAUnidade(chamado.UnidadeId);

        chamado.Encerrar(contextoUsuario.UsuarioId, requisicao.SolucaoAplicada.Trim());
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterPorIdAsync(chamadoId, cancelamento);
    }
}
