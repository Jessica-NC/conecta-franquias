using ConectaFranquias.Api.Data;
using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Extensoes;
using Microsoft.EntityFrameworkCore;

namespace ConectaFranquias.Api.Repositories;

public interface IRepositorioEstoque : IRepositorioBase<Estoque>
{
    Task<Estoque?> ObterPorUnidadeEProdutoAsync(int unidadeId, int produtoServicoId, CancellationToken cancelamento = default);

    Task<Estoque?> ObterComRelacionamentosAsync(int estoqueId, CancellationToken cancelamento = default);

    Task<IReadOnlyList<Estoque>> ObterParaMovimentacaoAsync(int unidadeId, IEnumerable<int> produtosServicosIds, CancellationToken cancelamento = default);

    Task<(IReadOnlyList<Estoque> Itens, int Total)> BuscarAsync(FiltroEstoques filtro, CancellationToken cancelamento = default);

    Task<IReadOnlyList<Estoque>> ListarAbaixoDoMinimoAsync(int? unidadeId, CancellationToken cancelamento = default);

    Task<(IReadOnlyList<MovimentacaoEstoque> Itens, int Total)> BuscarMovimentacoesAsync(FiltroMovimentacoes filtro, CancellationToken cancelamento = default);

    Task AdicionarMovimentacoesAsync(IEnumerable<MovimentacaoEstoque> movimentacoes, CancellationToken cancelamento = default);
}

public class RepositorioEstoque(ContextoFranquias contexto)
    : RepositorioBase<Estoque>(contexto), IRepositorioEstoque
{
    public async Task<Estoque?> ObterPorUnidadeEProdutoAsync(
        int unidadeId,
        int produtoServicoId,
        CancellationToken cancelamento = default) =>
        await Conjunto.FirstOrDefaultAsync(
            estoque => estoque.UnidadeId == unidadeId && estoque.ProdutoServicoId == produtoServicoId,
            cancelamento);

    public async Task<Estoque?> ObterComRelacionamentosAsync(int estoqueId, CancellationToken cancelamento = default) =>
        await Conjunto
            .Include(estoque => estoque.Unidade)
            .Include(estoque => estoque.ProdutoServico)
            .FirstOrDefaultAsync(estoque => estoque.EstoqueId == estoqueId, cancelamento);

    public async Task<IReadOnlyList<Estoque>> ObterParaMovimentacaoAsync(
        int unidadeId,
        IEnumerable<int> produtosServicosIds,
        CancellationToken cancelamento = default)
    {
        var identificadores = produtosServicosIds.Distinct().ToList();

        return await Conjunto
            .Include(estoque => estoque.ProdutoServico)
            .Where(estoque => estoque.UnidadeId == unidadeId && identificadores.Contains(estoque.ProdutoServicoId))
            .ToListAsync(cancelamento);
    }

    public async Task<(IReadOnlyList<Estoque> Itens, int Total)> BuscarAsync(
        FiltroEstoques filtro,
        CancellationToken cancelamento = default)
    {
        var termo = filtro.Termo?.Trim().ToLowerInvariant();

        var consulta = ConsultaSemRastreamento()
            .Include(estoque => estoque.Unidade)
            .Include(estoque => estoque.ProdutoServico)
            .FiltrarQuando(filtro.UnidadeId.HasValue, estoque => estoque.UnidadeId == filtro.UnidadeId)
            .FiltrarQuando(filtro.ProdutoServicoId.HasValue, estoque => estoque.ProdutoServicoId == filtro.ProdutoServicoId)
            .FiltrarQuando(filtro.AbaixoDoMinimo == true, estoque => estoque.Quantidade < estoque.QuantidadeMinima)
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(termo),
                estoque => estoque.ProdutoServico.Nome.ToLower().Contains(termo!) ||
                           estoque.ProdutoServico.Codigo.ToLower().Contains(termo!))
            .OrdenarPorCampo(filtro.OrdenarPor, filtro.Descendente, nameof(Estoque.EstoqueId));

        return await consulta.ObterPaginaAsync(filtro, cancelamento);
    }

    public async Task<IReadOnlyList<Estoque>> ListarAbaixoDoMinimoAsync(
        int? unidadeId,
        CancellationToken cancelamento = default) =>
        await ConsultaSemRastreamento()
            .Include(estoque => estoque.Unidade)
            .Include(estoque => estoque.ProdutoServico)
            .Where(estoque => estoque.Quantidade < estoque.QuantidadeMinima)
            .FiltrarQuando(unidadeId.HasValue, estoque => estoque.UnidadeId == unidadeId)
            .OrderBy(estoque => estoque.Quantidade - estoque.QuantidadeMinima)
            .ToListAsync(cancelamento);

    public async Task<(IReadOnlyList<MovimentacaoEstoque> Itens, int Total)> BuscarMovimentacoesAsync(
        FiltroMovimentacoes filtro,
        CancellationToken cancelamento = default)
    {
        var consulta = Contexto.MovimentacoesEstoque
            .AsNoTracking()
            .Include(movimentacao => movimentacao.Estoque)
                .ThenInclude(estoque => estoque.ProdutoServico)
            .FiltrarQuando(filtro.UnidadeId.HasValue, movimentacao => movimentacao.Estoque.UnidadeId == filtro.UnidadeId)
            .FiltrarQuando(
                filtro.ProdutoServicoId.HasValue,
                movimentacao => movimentacao.Estoque.ProdutoServicoId == filtro.ProdutoServicoId)
            .FiltrarQuando(filtro.Tipo.HasValue, movimentacao => movimentacao.Tipo == filtro.Tipo!.Value)
            .OrdenarPorCampo(
                filtro.OrdenarPor,
                // Sem ordenação explícita, a movimentação mais recente vem primeiro.
                string.IsNullOrWhiteSpace(filtro.OrdenarPor) || filtro.Descendente,
                nameof(MovimentacaoEstoque.DataMovimentacao));

        return await consulta.ObterPaginaAsync(filtro, cancelamento);
    }

    public async Task AdicionarMovimentacoesAsync(
        IEnumerable<MovimentacaoEstoque> movimentacoes,
        CancellationToken cancelamento = default) =>
        await Contexto.MovimentacoesEstoque.AddRangeAsync(movimentacoes, cancelamento);
}

public interface IRepositorioVenda : IRepositorioBase<Venda>
{
    Task<Venda?> ObterComItensAsync(int vendaId, CancellationToken cancelamento = default);

    Task<(IReadOnlyList<Venda> Itens, int Total)> BuscarAsync(FiltroVendas filtro, CancellationToken cancelamento = default);

    Task<string> GerarProximoNumeroAsync(int unidadeId, CancellationToken cancelamento = default);

    Task<decimal> CalcularFaturamentoAsync(int unidadeId, DateTime dataInicio, DateTime dataFim, CancellationToken cancelamento = default);
}

public class RepositorioVenda(ContextoFranquias contexto)
    : RepositorioBase<Venda>(contexto), IRepositorioVenda
{
    public async Task<Venda?> ObterComItensAsync(int vendaId, CancellationToken cancelamento = default) =>
        await Conjunto
            .Include(venda => venda.Unidade)
            .Include(venda => venda.Itens)
                .ThenInclude(item => item.ProdutoServico)
            .FirstOrDefaultAsync(venda => venda.VendaId == vendaId, cancelamento);

    public async Task<(IReadOnlyList<Venda> Itens, int Total)> BuscarAsync(
        FiltroVendas filtro,
        CancellationToken cancelamento = default)
    {
        var termo = filtro.Termo?.Trim().ToLowerInvariant();
        var dataInicio = filtro.DataInicio?.Date;
        var dataFim = filtro.DataFim?.Date.AddDays(1).AddTicks(-1);

        var consulta = ConsultaSemRastreamento()
            .Include(venda => venda.Unidade)
            .Include(venda => venda.Itens)
            .FiltrarQuando(filtro.UnidadeId.HasValue, venda => venda.UnidadeId == filtro.UnidadeId)
            .FiltrarQuando(filtro.Situacao.HasValue, venda => venda.Situacao == filtro.Situacao!.Value)
            .FiltrarQuando(dataInicio.HasValue, venda => venda.DataVenda >= dataInicio!.Value)
            .FiltrarQuando(dataFim.HasValue, venda => venda.DataVenda <= dataFim!.Value)
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(termo),
                venda => venda.NumeroVenda.ToLower().Contains(termo!))
            .OrdenarPorCampo(
                filtro.OrdenarPor,
                // Sem ordenação explícita, a venda mais recente vem primeiro.
                string.IsNullOrWhiteSpace(filtro.OrdenarPor) || filtro.Descendente,
                nameof(Venda.DataVenda));

        return await consulta.ObterPaginaAsync(filtro, cancelamento);
    }

    public async Task<string> GerarProximoNumeroAsync(int unidadeId, CancellationToken cancelamento = default)
    {
        var totalNaUnidade = await Conjunto
            .AsNoTracking()
            .CountAsync(venda => venda.UnidadeId == unidadeId, cancelamento);

        return $"V{unidadeId:D3}-{totalNaUnidade + 1:D6}";
    }

    public async Task<decimal> CalcularFaturamentoAsync(
        int unidadeId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancelamento = default)
    {
        var inicio = dataInicio.Date;
        var fim = dataFim.Date.AddDays(1).AddTicks(-1);

        return await ConsultaSemRastreamento()
            .Where(venda => venda.UnidadeId == unidadeId &&
                            venda.Situacao == SituacaoVenda.Confirmada &&
                            venda.DataVenda >= inicio &&
                            venda.DataVenda <= fim)
            .SumAsync(venda => (decimal?)venda.ValorTotal, cancelamento) ?? 0m;
    }
}

public interface IRepositorioRoyalty : IRepositorioBase<Royalty>
{
    Task<Royalty?> ObterComUnidadeAsync(int royaltyId, CancellationToken cancelamento = default);

    Task<Royalty?> ObterPorCompetenciaAsync(int unidadeId, int ano, int mes, CancellationToken cancelamento = default);

    Task<(IReadOnlyList<Royalty> Itens, int Total)> BuscarAsync(FiltroRoyalties filtro, CancellationToken cancelamento = default);
}

public class RepositorioRoyalty(ContextoFranquias contexto)
    : RepositorioBase<Royalty>(contexto), IRepositorioRoyalty
{
    public async Task<Royalty?> ObterComUnidadeAsync(int royaltyId, CancellationToken cancelamento = default) =>
        await Conjunto
            .Include(royalty => royalty.Unidade)
            .FirstOrDefaultAsync(royalty => royalty.RoyaltyId == royaltyId, cancelamento);

    public async Task<Royalty?> ObterPorCompetenciaAsync(
        int unidadeId,
        int ano,
        int mes,
        CancellationToken cancelamento = default) =>
        await Conjunto
            .Include(royalty => royalty.Unidade)
            .FirstOrDefaultAsync(
                royalty => royalty.UnidadeId == unidadeId &&
                           royalty.CompetenciaAno == ano &&
                           royalty.CompetenciaMes == mes,
                cancelamento);

    public async Task<(IReadOnlyList<Royalty> Itens, int Total)> BuscarAsync(
        FiltroRoyalties filtro,
        CancellationToken cancelamento = default)
    {
        var consulta = ConsultaSemRastreamento()
            .Include(royalty => royalty.Unidade)
            .FiltrarQuando(filtro.UnidadeId.HasValue, royalty => royalty.UnidadeId == filtro.UnidadeId)
            .FiltrarQuando(filtro.Situacao.HasValue, royalty => royalty.Situacao == filtro.Situacao!.Value)
            .FiltrarQuando(filtro.CompetenciaAno.HasValue, royalty => royalty.CompetenciaAno == filtro.CompetenciaAno)
            .FiltrarQuando(filtro.CompetenciaMes.HasValue, royalty => royalty.CompetenciaMes == filtro.CompetenciaMes)
            .OrdenarPorCampo(
                filtro.OrdenarPor,
                string.IsNullOrWhiteSpace(filtro.OrdenarPor) || filtro.Descendente,
                nameof(Royalty.DataVencimento));

        return await consulta.ObterPaginaAsync(filtro, cancelamento);
    }
}

public interface IRepositorioChamado : IRepositorioBase<ChamadoSuporte>
{
    Task<ChamadoSuporte?> ObterComInteracoesAsync(int chamadoId, CancellationToken cancelamento = default);

    Task<(IReadOnlyList<ChamadoSuporte> Itens, int Total)> BuscarAsync(FiltroChamados filtro, CancellationToken cancelamento = default);

    Task<string> GerarProtocoloAsync(CancellationToken cancelamento = default);

    Task<IReadOnlyDictionary<SituacaoChamado, int>> ContarPorSituacaoAsync(int? unidadeId, CancellationToken cancelamento = default);
}

public class RepositorioChamado(ContextoFranquias contexto)
    : RepositorioBase<ChamadoSuporte>(contexto), IRepositorioChamado
{
    private static readonly SituacaoChamado[] SituacoesEmAberto =
        [SituacaoChamado.Aberto, SituacaoChamado.EmAndamento];

    public async Task<ChamadoSuporte?> ObterComInteracoesAsync(int chamadoId, CancellationToken cancelamento = default) =>
        await Conjunto
            .Include(chamado => chamado.Unidade)
            .Include(chamado => chamado.Interacoes)
                .ThenInclude(interacao => interacao.Usuario)
            .FirstOrDefaultAsync(chamado => chamado.ChamadoSuporteId == chamadoId, cancelamento);

    public async Task<(IReadOnlyList<ChamadoSuporte> Itens, int Total)> BuscarAsync(
        FiltroChamados filtro,
        CancellationToken cancelamento = default)
    {
        var termo = filtro.Termo?.Trim().ToLowerInvariant();

        var consulta = ConsultaSemRastreamento()
            .Include(chamado => chamado.Unidade)
            .FiltrarQuando(filtro.UnidadeId.HasValue, chamado => chamado.UnidadeId == filtro.UnidadeId)
            .FiltrarQuando(filtro.Situacao.HasValue, chamado => chamado.Situacao == filtro.Situacao!.Value)
            .FiltrarQuando(filtro.Prioridade.HasValue, chamado => chamado.Prioridade == filtro.Prioridade!.Value)
            .FiltrarQuando(filtro.Categoria.HasValue, chamado => chamado.Categoria == filtro.Categoria!.Value)
            .FiltrarQuando(filtro.SomenteEmAberto == true, chamado => SituacoesEmAberto.Contains(chamado.Situacao))
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(termo),
                chamado => chamado.Titulo.ToLower().Contains(termo!) ||
                           chamado.Protocolo.ToLower().Contains(termo!))
            .OrdenarPorCampo(
                filtro.OrdenarPor,
                string.IsNullOrWhiteSpace(filtro.OrdenarPor) || filtro.Descendente,
                nameof(ChamadoSuporte.DataAbertura));

        return await consulta.ObterPaginaAsync(filtro, cancelamento);
    }

    public async Task<string> GerarProtocoloAsync(CancellationToken cancelamento = default)
    {
        var ano = DateTime.UtcNow.Year;

        var totalNoAno = await Conjunto
            .AsNoTracking()
            .CountAsync(chamado => chamado.DataAbertura.Year == ano, cancelamento);

        return $"CH-{ano}-{totalNoAno + 1:D6}";
    }

    public async Task<IReadOnlyDictionary<SituacaoChamado, int>> ContarPorSituacaoAsync(
        int? unidadeId,
        CancellationToken cancelamento = default)
    {
        var agrupamento = await ConsultaSemRastreamento()
            .FiltrarQuando(unidadeId.HasValue, chamado => chamado.UnidadeId == unidadeId)
            .GroupBy(chamado => chamado.Situacao)
            .Select(grupo => new { Situacao = grupo.Key, Quantidade = grupo.Count() })
            .ToListAsync(cancelamento);

        return agrupamento.ToDictionary(item => item.Situacao, item => item.Quantidade);
    }
}
