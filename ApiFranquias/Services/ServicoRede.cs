using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Excecoes;
using ConectaFranquias.Api.Extensoes;
using ConectaFranquias.Api.Mapeamentos;
using ConectaFranquias.Api.Repositories;
using ConectaFranquias.Api.Services.Seguranca;
using ConectaFranquias.Api.Validacoes;

namespace ConectaFranquias.Api.Services;

public interface IServicoRede
{
    Task<IReadOnlyList<FranqueadoraResposta>> ListarFranqueadorasAsync(CancellationToken cancelamento = default);

    Task<FranqueadoraResposta> ObterFranqueadoraAsync(int franqueadoraId, CancellationToken cancelamento = default);

    Task<FranqueadoraResposta> CriarFranqueadoraAsync(CriarFranqueadoraRequisicao requisicao, CancellationToken cancelamento = default);

    Task<FranqueadoraResposta> AtualizarFranqueadoraAsync(int franqueadoraId, AtualizarFranqueadoraRequisicao requisicao, CancellationToken cancelamento = default);

    Task<ResultadoPaginado<FranqueadoResposta>> BuscarFranqueadosAsync(FiltroFranqueados filtro, CancellationToken cancelamento = default);

    Task<FranqueadoResposta> ObterFranqueadoAsync(int franqueadoId, CancellationToken cancelamento = default);

    Task<FranqueadoResposta> CriarFranqueadoAsync(CriarFranqueadoRequisicao requisicao, CancellationToken cancelamento = default);

    Task<FranqueadoResposta> AtualizarFranqueadoAsync(int franqueadoId, AtualizarFranqueadoRequisicao requisicao, CancellationToken cancelamento = default);

    Task<ResultadoPaginado<UnidadeResumoResposta>> BuscarUnidadesAsync(FiltroUnidades filtro, CancellationToken cancelamento = default);

    Task<UnidadeResposta> ObterUnidadeAsync(int unidadeId, CancellationToken cancelamento = default);

    Task<UnidadeResposta> CriarUnidadeAsync(CriarUnidadeRequisicao requisicao, CancellationToken cancelamento = default);

    Task<UnidadeResposta> AtualizarUnidadeAsync(int unidadeId, AtualizarUnidadeRequisicao requisicao, CancellationToken cancelamento = default);

    Task<UnidadeResposta> AlterarSituacaoAsync(int unidadeId, AlterarSituacaoUnidadeRequisicao requisicao, CancellationToken cancelamento = default);

    Task<ResponsavelResposta> AdicionarResponsavelAsync(int unidadeId, CriarResponsavelRequisicao requisicao, CancellationToken cancelamento = default);

    Task<ResponsavelResposta> AtualizarResponsavelAsync(int unidadeId, int responsavelId, AtualizarResponsavelRequisicao requisicao, CancellationToken cancelamento = default);

    Task InativarResponsavelAsync(int unidadeId, int responsavelId, CancellationToken cancelamento = default);
}

public class ServicoRede(
    IRepositorioFranqueadora repositorioFranqueadora,
    IRepositorioFranqueado repositorioFranqueado,
    IRepositorioUnidade repositorioUnidade,
    IUnidadeDeTrabalho unidadeDeTrabalho,
    IContextoUsuario contextoUsuario) : IServicoRede
{

    public async Task<IReadOnlyList<FranqueadoraResposta>> ListarFranqueadorasAsync(
        CancellationToken cancelamento = default)
    {
        var franqueadoras = await repositorioFranqueadora.ListarComUnidadesAsync(cancelamento);
        return franqueadoras.Select(item => item.ParaResposta(item.Unidades.Count)).ToList();
    }

    public async Task<FranqueadoraResposta> ObterFranqueadoraAsync(
        int franqueadoraId,
        CancellationToken cancelamento = default)
    {
        var franqueadora = await repositorioFranqueadora.ObterComUnidadesAsync(franqueadoraId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Franqueadora", franqueadoraId);

        return franqueadora.ParaResposta(franqueadora.Unidades.Count);
    }

    public async Task<FranqueadoraResposta> CriarFranqueadoraAsync(
        CriarFranqueadoraRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        if (await repositorioFranqueadora.CnpjJaCadastradoAsync(requisicao.Cnpj, cancelamento))
        {
            throw new ExcecaoConflito($"Já existe uma franqueadora cadastrada com o CNPJ '{requisicao.Cnpj}'.");
        }

        var franqueadora = new Franqueadora(
            requisicao.RazaoSocial.Trim(),
            requisicao.NomeFantasia.Trim(),
            DocumentoValidador.FormatarCnpj(requisicao.Cnpj),
            requisicao.Email,
            requisicao.Telefone.Trim(),
            requisicao.DataFundacao);

        await repositorioFranqueadora.AdicionarAsync(franqueadora, cancelamento);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return franqueadora.ParaResposta(0);
    }

    public async Task<FranqueadoraResposta> AtualizarFranqueadoraAsync(
        int franqueadoraId,
        AtualizarFranqueadoraRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var franqueadora = await repositorioFranqueadora.ObterPorIdAsync(franqueadoraId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Franqueadora", franqueadoraId);

        franqueadora.AtualizarDados(
            requisicao.RazaoSocial.Trim(),
            requisicao.NomeFantasia.Trim(),
            requisicao.Email,
            requisicao.Telefone.Trim());

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        var totalUnidades = await repositorioFranqueadora.ContarUnidadesAsync(franqueadoraId, cancelamento);
        return franqueadora.ParaResposta(totalUnidades);
    }

    public async Task<ResultadoPaginado<FranqueadoResposta>> BuscarFranqueadosAsync(
        FiltroFranqueados filtro,
        CancellationToken cancelamento = default)
    {
        var pagina = await repositorioFranqueado.BuscarAsync(filtro, cancelamento);
        return pagina.ParaResultadoPaginado(filtro, item => item.ParaResposta(item.Unidades.Count));
    }

    public async Task<FranqueadoResposta> ObterFranqueadoAsync(
        int franqueadoId,
        CancellationToken cancelamento = default)
    {
        var franqueado = await repositorioFranqueado.ObterComUnidadesAsync(franqueadoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Franqueado", franqueadoId);

        return franqueado.ParaResposta(franqueado.Unidades.Count);
    }

    public async Task<FranqueadoResposta> CriarFranqueadoAsync(
        CriarFranqueadoRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        if (await repositorioFranqueado.CpfJaCadastradoAsync(requisicao.Cpf, cancelamento))
        {
            throw new ExcecaoConflito($"Já existe um franqueado cadastrado com o CPF '{requisicao.Cpf}'.");
        }

        var franqueado = new Franqueado(
            requisicao.Nome.Trim(),
            DocumentoValidador.FormatarCpf(requisicao.Cpf),
            requisicao.Email,
            requisicao.Telefone.Trim(),
            requisicao.Cidade.Trim(),
            requisicao.Estado.Trim().ToUpperInvariant());

        await repositorioFranqueado.AdicionarAsync(franqueado, cancelamento);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return franqueado.ParaResposta(0);
    }

    public async Task<FranqueadoResposta> AtualizarFranqueadoAsync(
        int franqueadoId,
        AtualizarFranqueadoRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var franqueado = await repositorioFranqueado.ObterPorIdAsync(franqueadoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Franqueado", franqueadoId);

        franqueado.AtualizarDados(
            requisicao.Nome.Trim(),
            requisicao.Email,
            requisicao.Telefone.Trim(),
            requisicao.Cidade.Trim(),
            requisicao.Estado.Trim().ToUpperInvariant());

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        var totalUnidades = await repositorioFranqueado.ContarUnidadesAsync(franqueadoId, cancelamento);
        return franqueado.ParaResposta(totalUnidades);
    }

    public async Task<ResultadoPaginado<UnidadeResumoResposta>> BuscarUnidadesAsync(
        FiltroUnidades filtro,
        CancellationToken cancelamento = default)
    {
        if (!contextoUsuario.EhAdministrador)
        {
            var unidadeDoUsuario = contextoUsuario.UnidadeId
                ?? throw new ExcecaoAutorizacao("Usuário sem unidade vinculada.");

            var unidade = await repositorioUnidade.ObterComRelacionamentosAsync(unidadeDoUsuario, cancelamento);

            IReadOnlyList<UnidadeResumoResposta> itens = unidade is null ? [] : [unidade.ParaResumo()];

            return new ResultadoPaginado<UnidadeResumoResposta>(itens, itens.Count, filtro.Pagina, filtro.TamanhoPagina);
        }

        var pagina = await repositorioUnidade.BuscarAsync(filtro, cancelamento);
        return pagina.ParaResultadoPaginado(filtro, item => item.ParaResumo());
    }

    public async Task<UnidadeResposta> ObterUnidadeAsync(int unidadeId, CancellationToken cancelamento = default)
    {
        contextoUsuario.GarantirAcessoAUnidade(unidadeId);
        return (await CarregarUnidadeAsync(unidadeId, cancelamento)).ParaResposta();
    }

    public async Task<UnidadeResposta> CriarUnidadeAsync(
        CriarUnidadeRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        if (await repositorioUnidade.CnpjJaCadastradoAsync(requisicao.Cnpj, cancelamento))
        {
            throw new ExcecaoConflito($"Já existe uma unidade cadastrada com o CNPJ '{requisicao.Cnpj}'.");
        }

        if (await repositorioUnidade.CodigoJaCadastradoAsync(requisicao.Codigo, cancelamento))
        {
            throw new ExcecaoConflito($"Já existe uma unidade com o código '{requisicao.Codigo}'.");
        }

        if (!await repositorioFranqueadora.ExisteAsync(requisicao.FranqueadoraId, cancelamento))
        {
            throw new ExcecaoNaoEncontrado("Franqueadora", requisicao.FranqueadoraId);
        }

        if (!await repositorioFranqueado.ExisteAsync(requisicao.FranqueadoId, cancelamento))
        {
            throw new ExcecaoNaoEncontrado("Franqueado", requisicao.FranqueadoId);
        }

        var unidade = new UnidadeFranqueada(
            requisicao.FranqueadoraId,
            requisicao.FranqueadoId,
            requisicao.Codigo.Trim().ToUpperInvariant(),
            requisicao.NomeFantasia.Trim(),
            requisicao.RazaoSocial.Trim(),
            DocumentoValidador.FormatarCnpj(requisicao.Cnpj),
            requisicao.Email,
            requisicao.Telefone.Trim(),
            requisicao.Endereco.ParaEntidade(),
            requisicao.DataInicioContrato,
            requisicao.PercentualRoyalty,
            requisicao.TaxaFranquiaMensal);

        await repositorioUnidade.AdicionarAsync(unidade, cancelamento);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return (await CarregarUnidadeAsync(unidade.UnidadeId, cancelamento)).ParaResposta();
    }

    public async Task<UnidadeResposta> AtualizarUnidadeAsync(
        int unidadeId,
        AtualizarUnidadeRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        contextoUsuario.GarantirAcessoAUnidade(unidadeId);

        var unidade = await repositorioUnidade.ObterPorIdAsync(unidadeId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Unidade", unidadeId);

        if (!await repositorioFranqueado.ExisteAsync(requisicao.FranqueadoId, cancelamento))
        {
            throw new ExcecaoNaoEncontrado("Franqueado", requisicao.FranqueadoId);
        }

        if (!contextoUsuario.EhAdministrador &&
            (requisicao.PercentualRoyalty != unidade.PercentualRoyalty ||
             requisicao.TaxaFranquiaMensal != unidade.TaxaFranquiaMensal ||
             requisicao.FranqueadoId != unidade.FranqueadoId))
        {
            throw new ExcecaoAutorizacao(
                "Apenas administradores da franqueadora podem alterar franqueado, percentual de royalty ou taxa de franquia.");
        }

        unidade.AtualizarDados(
            requisicao.FranqueadoId,
            requisicao.NomeFantasia.Trim(),
            requisicao.RazaoSocial.Trim(),
            requisicao.Email,
            requisicao.Telefone.Trim(),
            requisicao.Endereco.ParaEntidade(),
            requisicao.PercentualRoyalty,
            requisicao.TaxaFranquiaMensal);

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return (await CarregarUnidadeAsync(unidadeId, cancelamento)).ParaResposta();
    }

    public async Task<UnidadeResposta> AlterarSituacaoAsync(
        int unidadeId,
        AlterarSituacaoUnidadeRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var unidade = await repositorioUnidade.ObterPorIdAsync(unidadeId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Unidade", unidadeId);

        if (unidade.Situacao == requisicao.Situacao)
        {
            throw new ExcecaoNegocio($"A unidade já se encontra na situação '{requisicao.Situacao}'.");
        }

        unidade.AlterarSituacao(requisicao.Situacao);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return (await CarregarUnidadeAsync(unidadeId, cancelamento)).ParaResposta();
    }

    public async Task<ResponsavelResposta> AdicionarResponsavelAsync(
        int unidadeId,
        CriarResponsavelRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        contextoUsuario.GarantirAcessoAUnidade(unidadeId);

        if (!await repositorioUnidade.ExisteAsync(unidadeId, cancelamento))
        {
            throw new ExcecaoNaoEncontrado("Unidade", unidadeId);
        }

        if (await repositorioUnidade.ResponsavelJaCadastradoAsync(unidadeId, requisicao.Cpf, cancelamento))
        {
            throw new ExcecaoConflito($"O CPF '{requisicao.Cpf}' já está cadastrado como responsável desta unidade.");
        }

        var responsavel = new Responsavel(
            unidadeId,
            requisicao.Nome.Trim(),
            DocumentoValidador.FormatarCpf(requisicao.Cpf),
            requisicao.Cargo.Trim(),
            requisicao.Email,
            requisicao.Telefone.Trim());

        await repositorioUnidade.AdicionarResponsavelAsync(responsavel, cancelamento);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return responsavel.ParaResposta();
    }

    public async Task<ResponsavelResposta> AtualizarResponsavelAsync(
        int unidadeId,
        int responsavelId,
        AtualizarResponsavelRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        contextoUsuario.GarantirAcessoAUnidade(unidadeId);

        var responsavel = await CarregarResponsavelAsync(unidadeId, responsavelId, cancelamento);

        responsavel.AtualizarDados(
            requisicao.Nome.Trim(),
            requisicao.Cargo.Trim(),
            requisicao.Email,
            requisicao.Telefone.Trim());

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return responsavel.ParaResposta();
    }

    public async Task InativarResponsavelAsync(
        int unidadeId,
        int responsavelId,
        CancellationToken cancelamento = default)
    {
        contextoUsuario.GarantirAcessoAUnidade(unidadeId);

        var responsavel = await CarregarResponsavelAsync(unidadeId, responsavelId, cancelamento);

        if (!responsavel.Ativo)
        {
            throw new ExcecaoNegocio("Este responsável já está inativo.");
        }

        responsavel.Inativar();
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);
    }

    private async Task<UnidadeFranqueada> CarregarUnidadeAsync(int unidadeId, CancellationToken cancelamento) =>
        await repositorioUnidade.ObterComRelacionamentosAsync(unidadeId, cancelamento)
        ?? throw new ExcecaoNaoEncontrado("Unidade", unidadeId);

    private async Task<Responsavel> CarregarResponsavelAsync(
        int unidadeId,
        int responsavelId,
        CancellationToken cancelamento)
    {
        var responsavel = await repositorioUnidade.ObterResponsavelAsync(responsavelId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Responsável", responsavelId);

        if (responsavel.UnidadeId != unidadeId)
        {
            throw new ExcecaoNaoEncontrado($"O responsável {responsavelId} não pertence à unidade {unidadeId}.");
        }

        return responsavel;
    }
}
