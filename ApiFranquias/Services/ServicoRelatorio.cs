using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Repositories;
using ConectaFranquias.Api.Services.Seguranca;

namespace ConectaFranquias.Api.Services;

public interface IServicoRelatorio
{
    Task<IReadOnlyList<FaturamentoUnidadeResposta>> ObterFaturamentoAsync(FiltroPeriodo filtro, CancellationToken cancelamento = default);

    Task<IReadOnlyList<RankingUnidadeResposta>> ObterRankingUnidadesAsync(FiltroPeriodo filtro, CancellationToken cancelamento = default);

    Task<IReadOnlyList<ProdutoMaisVendidoResposta>> ObterProdutosMaisVendidosAsync(FiltroPeriodo filtro, int quantidade, CancellationToken cancelamento = default);

    Task<IReadOnlyList<EstoqueCriticoResposta>> ObterEstoqueCriticoAsync(int? unidadeId, CancellationToken cancelamento = default);

    Task<ResumoRoyaltiesResposta> ObterResumoRoyaltiesAsync(FiltroPeriodo filtro, CancellationToken cancelamento = default);

    Task<IReadOnlyList<ChamadosPorSituacaoResposta>> ObterChamadosPorSituacaoAsync(int? unidadeId, CancellationToken cancelamento = default);
}

public class ServicoRelatorio(
    IRepositorioRelatorio repositorioRelatorio,
    IRepositorioEstoque repositorioEstoque,
    IRepositorioChamado repositorioChamado,
    IContextoUsuario contextoUsuario) : IServicoRelatorio
{
    public async Task<IReadOnlyList<FaturamentoUnidadeResposta>> ObterFaturamentoAsync(
        FiltroPeriodo filtro,
        CancellationToken cancelamento = default)
    {
        var unidadeId = contextoUsuario.ResolverUnidadeDaConsulta(filtro.UnidadeId);

        var dados = await repositorioRelatorio.ObterFaturamentoPorUnidadeAsync(
            filtro.DataInicio,
            filtro.DataFim,
            unidadeId,
            cancelamento);

        return dados
            .Select(item => new FaturamentoUnidadeResposta
            {
                UnidadeId = item.UnidadeId,
                UnidadeCodigo = item.Codigo,
                UnidadeNome = item.NomeFantasia,
                Situacao = item.Situacao,
                TotalVendas = item.TotalVendas,
                Faturamento = item.Faturamento,
                TicketMedio = CalcularTicketMedio(item.Faturamento, item.TotalVendas),
                PercentualRoyalty = item.PercentualRoyalty,
                RoyaltyEstimado = Arredondar(item.Faturamento * (item.PercentualRoyalty / 100m))
            })
            .OrderByDescending(item => item.Faturamento)
            .ToList();
    }

    public async Task<IReadOnlyList<RankingUnidadeResposta>> ObterRankingUnidadesAsync(
        FiltroPeriodo filtro,
        CancellationToken cancelamento = default)
    {
        if (!contextoUsuario.EhAdministrador)
        {
            throw new Excecoes.ExcecaoAutorizacao(
                "O ranking de unidades está disponível apenas para administradores da franqueadora.");
        }

        var dados = await repositorioRelatorio.ObterFaturamentoPorUnidadeAsync(
            filtro.DataInicio,
            filtro.DataFim,
            filtro.UnidadeId,
            cancelamento);

        var faturamentoDaRede = dados.Sum(item => item.Faturamento);

        return dados
            .OrderByDescending(item => item.Faturamento)
            .ThenBy(item => item.NomeFantasia)
            .Select((item, indice) => new RankingUnidadeResposta
            {
                Posicao = indice + 1,
                UnidadeId = item.UnidadeId,
                UnidadeCodigo = item.Codigo,
                UnidadeNome = item.NomeFantasia,
                TotalVendas = item.TotalVendas,
                Faturamento = item.Faturamento,
                PercentualDaRede = faturamentoDaRede == 0m
                    ? 0m
                    : Arredondar(item.Faturamento / faturamentoDaRede * 100m)
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ProdutoMaisVendidoResposta>> ObterProdutosMaisVendidosAsync(
        FiltroPeriodo filtro,
        int quantidade,
        CancellationToken cancelamento = default)
    {
        var unidadeId = contextoUsuario.ResolverUnidadeDaConsulta(filtro.UnidadeId);

        var dados = await repositorioRelatorio.ObterProdutosMaisVendidosAsync(
            filtro.DataInicio,
            filtro.DataFim,
            unidadeId,
            quantidade,
            cancelamento);

        return dados
            .Select((item, indice) => new ProdutoMaisVendidoResposta
            {
                Posicao = indice + 1,
                ProdutoServicoId = item.ProdutoServicoId,
                Codigo = item.Codigo,
                Nome = item.Nome,
                CategoriaNome = item.CategoriaNome,
                QuantidadeVendida = item.QuantidadeVendida,
                ValorTotalVendido = item.ValorTotalVendido
            })
            .ToList();
    }

    public async Task<IReadOnlyList<EstoqueCriticoResposta>> ObterEstoqueCriticoAsync(
        int? unidadeId,
        CancellationToken cancelamento = default)
    {
        var unidadeConsulta = contextoUsuario.ResolverUnidadeDaConsulta(unidadeId);

        var estoques = await repositorioEstoque.ListarAbaixoDoMinimoAsync(unidadeConsulta, cancelamento);

        return estoques
            .Select(estoque => new EstoqueCriticoResposta
            {
                UnidadeId = estoque.UnidadeId,
                UnidadeNome = estoque.Unidade.NomeFantasia,
                ProdutoServicoId = estoque.ProdutoServicoId,
                ProdutoCodigo = estoque.ProdutoServico.Codigo,
                ProdutoNome = estoque.ProdutoServico.Nome,
                Quantidade = estoque.Quantidade,
                QuantidadeMinima = estoque.QuantidadeMinima,
                QuantidadeARepor = estoque.QuantidadeMinima - estoque.Quantidade
            })
            .ToList();
    }

    public async Task<ResumoRoyaltiesResposta> ObterResumoRoyaltiesAsync(
        FiltroPeriodo filtro,
        CancellationToken cancelamento = default)
    {
        var unidadeId = contextoUsuario.ResolverUnidadeDaConsulta(filtro.UnidadeId);

        var resumo = await repositorioRelatorio.ObterResumoRoyaltiesAsync(
            filtro.DataInicio,
            filtro.DataFim,
            unidadeId,
            cancelamento);

        return new ResumoRoyaltiesResposta
        {
            TotalCobrancas = resumo.TotalCobrancas,
            FaturamentoBaseTotal = resumo.FaturamentoBaseTotal,
            TotalRoyalties = resumo.TotalRoyalties,
            TotalTaxasFranquia = resumo.TotalTaxasFranquia,
            ValorTotalGerado = resumo.ValorTotalGerado,
            ValorPago = resumo.ValorPago,
            ValorEmAberto = resumo.ValorTotalGerado - resumo.ValorPago
        };
    }

    public async Task<IReadOnlyList<ChamadosPorSituacaoResposta>> ObterChamadosPorSituacaoAsync(
        int? unidadeId,
        CancellationToken cancelamento = default)
    {
        var unidadeConsulta = contextoUsuario.ResolverUnidadeDaConsulta(unidadeId);

        var contagens = await repositorioChamado.ContarPorSituacaoAsync(unidadeConsulta, cancelamento);

        // Situações sem chamados aparecem com zero para o indicador ficar completo.
        return Enum.GetValues<SituacaoChamado>()
            .Select(situacao => new ChamadosPorSituacaoResposta
            {
                Situacao = situacao,
                Quantidade = contagens.TryGetValue(situacao, out var valor) ? valor : 0
            })
            .ToList();
    }

    private static decimal CalcularTicketMedio(decimal faturamento, int totalVendas) =>
        totalVendas == 0 ? 0m : Arredondar(faturamento / totalVendas);

    private static decimal Arredondar(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
