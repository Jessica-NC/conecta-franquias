using ConectaFranquias.Api.Data;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Extensoes;
using Microsoft.EntityFrameworkCore;

namespace ConectaFranquias.Api.Repositories;

public record FaturamentoBrutoUnidade(
    int UnidadeId,
    string Codigo,
    string NomeFantasia,
    SituacaoUnidade Situacao,
    decimal PercentualRoyalty,
    int TotalVendas,
    decimal Faturamento);

public record ProdutoVendidoBruto(
    int ProdutoServicoId,
    string Codigo,
    string Nome,
    string CategoriaNome,
    int QuantidadeVendida,
    decimal ValorTotalVendido);

public record ResumoRoyaltiesBruto(
    int TotalCobrancas,
    decimal FaturamentoBaseTotal,
    decimal TotalRoyalties,
    decimal TotalTaxasFranquia,
    decimal ValorTotalGerado,
    decimal ValorPago);

public interface IRepositorioRelatorio
{
    Task<IReadOnlyList<FaturamentoBrutoUnidade>> ObterFaturamentoPorUnidadeAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int? unidadeId,
        CancellationToken cancelamento = default);

    Task<IReadOnlyList<ProdutoVendidoBruto>> ObterProdutosMaisVendidosAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int? unidadeId,
        int quantidade,
        CancellationToken cancelamento = default);

    Task<ResumoRoyaltiesBruto> ObterResumoRoyaltiesAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int? unidadeId,
        CancellationToken cancelamento = default);
}

public class RepositorioRelatorio(ContextoFranquias contexto) : IRepositorioRelatorio
{
    public async Task<IReadOnlyList<FaturamentoBrutoUnidade>> ObterFaturamentoPorUnidadeAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int? unidadeId,
        CancellationToken cancelamento = default)
    {
        var inicio = dataInicio.Date;
        var fim = dataFim.Date.AddDays(1).AddTicks(-1);

        // A consulta parte das unidades para que unidades sem vendas no período
        // apareçam no relatório com faturamento zero, em vez de sumirem.
        return await contexto.Unidades
            .AsNoTracking()
            .FiltrarQuando(unidadeId.HasValue, unidade => unidade.UnidadeId == unidadeId)
            .Select(unidade => new FaturamentoBrutoUnidade(
                unidade.UnidadeId,
                unidade.Codigo,
                unidade.NomeFantasia,
                unidade.Situacao,
                unidade.PercentualRoyalty,
                unidade.Vendas.Count(venda =>
                    venda.Situacao == SituacaoVenda.Confirmada &&
                    venda.DataVenda >= inicio &&
                    venda.DataVenda <= fim),
                unidade.Vendas
                    .Where(venda =>
                        venda.Situacao == SituacaoVenda.Confirmada &&
                        venda.DataVenda >= inicio &&
                        venda.DataVenda <= fim)
                    .Sum(venda => (decimal?)venda.ValorTotal) ?? 0m))
            .ToListAsync(cancelamento);
    }

    public async Task<IReadOnlyList<ProdutoVendidoBruto>> ObterProdutosMaisVendidosAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int? unidadeId,
        int quantidade,
        CancellationToken cancelamento = default)
    {
        var inicio = dataInicio.Date;
        var fim = dataFim.Date.AddDays(1).AddTicks(-1);

        // A agregação roda no banco sobre uma chave simples; os dados descritivos
        // do produto são anexados depois, porque o SQLite não traduz um GROUP BY
        // sobre colunas vindas de join.
        var agregados = await contexto.ItensVenda
            .AsNoTracking()
            .Where(item => item.Venda.Situacao == SituacaoVenda.Confirmada &&
                           item.Venda.DataVenda >= inicio &&
                           item.Venda.DataVenda <= fim)
            .FiltrarQuando(unidadeId.HasValue, item => item.Venda.UnidadeId == unidadeId)
            .GroupBy(item => item.ProdutoServicoId)
            .Select(grupo => new
            {
                ProdutoServicoId = grupo.Key,
                QuantidadeVendida = grupo.Sum(item => item.Quantidade),
                ValorTotalVendido = grupo.Sum(item => item.ValorTotal)
            })
            .OrderByDescending(produto => produto.QuantidadeVendida)
            .Take(quantidade)
            .ToListAsync(cancelamento);

        if (agregados.Count == 0)
        {
            return [];
        }

        var identificadores = agregados.Select(item => item.ProdutoServicoId).ToList();

        var produtos = await contexto.ProdutosServicos
            .AsNoTracking()
            .Include(produto => produto.Categoria)
            .Where(produto => identificadores.Contains(produto.ProdutoServicoId))
            .ToDictionaryAsync(produto => produto.ProdutoServicoId, cancelamento);

        return agregados
            .Select(item =>
            {
                var produto = produtos[item.ProdutoServicoId];

                return new ProdutoVendidoBruto(
                    produto.ProdutoServicoId,
                    produto.Codigo,
                    produto.Nome,
                    produto.Categoria.Nome,
                    item.QuantidadeVendida,
                    item.ValorTotalVendido);
            })
            .ToList();
    }

    public async Task<ResumoRoyaltiesBruto> ObterResumoRoyaltiesAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int? unidadeId,
        CancellationToken cancelamento = default)
    {
        var inicio = dataInicio.Date;
        var fim = dataFim.Date.AddDays(1).AddTicks(-1);

        // A competência entra no período quando seu intervalo se sobrepõe a ele.
        var resumo = await contexto.Royalties
            .AsNoTracking()
            .Where(royalty => royalty.DataInicio <= fim && royalty.DataFim >= inicio)
            .FiltrarQuando(unidadeId.HasValue, royalty => royalty.UnidadeId == unidadeId)
            .GroupBy(royalty => 1)
            .Select(grupo => new ResumoRoyaltiesBruto(
                grupo.Count(),
                grupo.Sum(royalty => royalty.FaturamentoBase),
                grupo.Sum(royalty => royalty.ValorRoyalty),
                grupo.Sum(royalty => royalty.TaxaFranquia),
                grupo.Sum(royalty => royalty.ValorTotal),
                grupo.Where(royalty => royalty.Situacao == SituacaoRoyalty.Pago)
                    .Sum(royalty => (decimal?)royalty.ValorPago) ?? 0m))
            .FirstOrDefaultAsync(cancelamento);

        return resumo ?? new ResumoRoyaltiesBruto(0, 0m, 0m, 0m, 0m, 0m);
    }
}
