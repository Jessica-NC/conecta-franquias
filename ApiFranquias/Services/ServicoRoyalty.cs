using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Excecoes;
using ConectaFranquias.Api.Extensoes;
using ConectaFranquias.Api.Mapeamentos;
using ConectaFranquias.Api.Repositories;
using ConectaFranquias.Api.Services.Seguranca;

namespace ConectaFranquias.Api.Services;

public interface IServicoRoyalty
{
    Task<ResultadoPaginado<RoyaltyResposta>> BuscarAsync(FiltroRoyalties filtro, CancellationToken cancelamento = default);

    Task<RoyaltyResposta> ObterPorIdAsync(int royaltyId, CancellationToken cancelamento = default);

    Task<RoyaltyResposta> ApurarAsync(ApurarRoyaltyRequisicao requisicao, CancellationToken cancelamento = default);

    Task<RoyaltyResposta> RegistrarPagamentoAsync(int royaltyId, RegistrarPagamentoRoyaltyRequisicao requisicao, CancellationToken cancelamento = default);
}

public class ServicoRoyalty(
    IRepositorioRoyalty repositorioRoyalty,
    IRepositorioUnidade repositorioUnidade,
    IRepositorioVenda repositorioVenda,
    IUnidadeDeTrabalho unidadeDeTrabalho,
    IContextoUsuario contextoUsuario) : IServicoRoyalty
{
    private const int DiaVencimento = 10;

    public async Task<ResultadoPaginado<RoyaltyResposta>> BuscarAsync(
        FiltroRoyalties filtro,
        CancellationToken cancelamento = default)
    {
        filtro.UnidadeId = contextoUsuario.ResolverUnidadeDaConsulta(filtro.UnidadeId);

        var pagina = await repositorioRoyalty.BuscarAsync(filtro, cancelamento);
        return pagina.ParaResultadoPaginado(filtro, item => item.ParaResposta());
    }

    public async Task<RoyaltyResposta> ObterPorIdAsync(int royaltyId, CancellationToken cancelamento = default)
    {
        var royalty = await repositorioRoyalty.ObterComUnidadeAsync(royaltyId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Royalty", royaltyId);

        contextoUsuario.GarantirAcessoAUnidade(royalty.UnidadeId);

        return royalty.ParaResposta();
    }

    public async Task<RoyaltyResposta> ApurarAsync(
        ApurarRoyaltyRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var unidade = await repositorioUnidade.ObterPorIdAsync(requisicao.UnidadeId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Unidade", requisicao.UnidadeId);

        var dataInicio = new DateTime(requisicao.CompetenciaAno, requisicao.CompetenciaMes, 1);
        var dataFim = dataInicio.AddMonths(1).AddDays(-1);

        if (dataInicio > DateTime.UtcNow.Date)
        {
            throw new ExcecaoNegocio("Não é possível apurar royalties de uma competência futura.");
        }

        var faturamento = await repositorioVenda.CalcularFaturamentoAsync(
            unidade.UnidadeId,
            dataInicio,
            dataFim,
            cancelamento);

        var existente = await repositorioRoyalty.ObterPorCompetenciaAsync(
            unidade.UnidadeId,
            requisicao.CompetenciaAno,
            requisicao.CompetenciaMes,
            cancelamento);

        Royalty royalty;

        if (existente is not null)
        {
            if (!requisicao.ReapurarSeExistir)
            {
                throw new ExcecaoConflito(
                    $"A competência {requisicao.CompetenciaAno:D4}-{requisicao.CompetenciaMes:D2} da unidade " +
                    $"'{unidade.NomeFantasia}' já foi apurada. Use 'reapurarSeExistir' para recalcular.");
            }

            existente.AtualizarApuracao(faturamento, unidade.PercentualRoyalty, unidade.TaxaFranquiaMensal);
            royalty = existente;
        }
        else
        {
            royalty = new Royalty(
                unidade.UnidadeId,
                requisicao.CompetenciaAno,
                requisicao.CompetenciaMes,
                dataInicio,
                dataFim,
                faturamento,
                unidade.PercentualRoyalty,
                unidade.TaxaFranquiaMensal,
                dataVencimento: dataInicio.AddMonths(1).AddDays(DiaVencimento - 1));

            // Competências já vencidas nascem marcadas como atrasadas.
            royalty.MarcarComoAtrasado();

            await repositorioRoyalty.AdicionarAsync(royalty, cancelamento);
        }

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterPorIdAsync(royalty.RoyaltyId, cancelamento);
    }

    public async Task<RoyaltyResposta> RegistrarPagamentoAsync(
        int royaltyId,
        RegistrarPagamentoRoyaltyRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var royalty = await repositorioRoyalty.ObterPorIdAsync(royaltyId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Royalty", royaltyId);

        var dataPagamento = requisicao.DataPagamento ?? DateTime.UtcNow;

        if (dataPagamento.Date > DateTime.UtcNow.Date)
        {
            throw new ExcecaoNegocio("A data de pagamento não pode estar no futuro.");
        }

        royalty.RegistrarPagamento(requisicao.ValorPago, dataPagamento);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterPorIdAsync(royaltyId, cancelamento);
    }
}
