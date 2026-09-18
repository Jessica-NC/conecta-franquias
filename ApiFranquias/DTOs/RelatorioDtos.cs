using ConectaFranquias.Api.Entities;

namespace ConectaFranquias.Api.DTOs;

/// <summary>Faturamento consolidado de uma unidade em um período.</summary>
public class FaturamentoUnidadeResposta
{
    public int UnidadeId { get; init; }

    public string UnidadeCodigo { get; init; } = string.Empty;

    public string UnidadeNome { get; init; } = string.Empty;

    public SituacaoUnidade Situacao { get; init; }

    public int TotalVendas { get; init; }

    /// <summary>Soma das vendas confirmadas: base de cálculo dos royalties.</summary>
    public decimal Faturamento { get; init; }

    public decimal TicketMedio { get; init; }

    public decimal PercentualRoyalty { get; init; }

    /// <summary>Royalty projetado sobre o faturamento do período.</summary>
    public decimal RoyaltyEstimado { get; init; }
}

/// <summary>Posição de uma unidade no ranking de faturamento.</summary>
public class RankingUnidadeResposta
{
    public int Posicao { get; init; }

    public int UnidadeId { get; init; }

    public string UnidadeCodigo { get; init; } = string.Empty;

    public string UnidadeNome { get; init; } = string.Empty;

    public int TotalVendas { get; init; }

    public decimal Faturamento { get; init; }

    /// <summary>Participação da unidade no faturamento total da rede no período.</summary>
    public decimal PercentualDaRede { get; init; }
}

/// <summary>Item do catálogo com maior volume de vendas no período.</summary>
public class ProdutoMaisVendidoResposta
{
    public int Posicao { get; init; }

    public int ProdutoServicoId { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string Nome { get; init; } = string.Empty;

    public string CategoriaNome { get; init; } = string.Empty;

    public int QuantidadeVendida { get; init; }

    public decimal ValorTotalVendido { get; init; }
}

/// <summary>Item cujo saldo está abaixo do ponto de reposição.</summary>
public class EstoqueCriticoResposta
{
    public int UnidadeId { get; init; }

    public string UnidadeNome { get; init; } = string.Empty;

    public int ProdutoServicoId { get; init; }

    public string ProdutoCodigo { get; init; } = string.Empty;

    public string ProdutoNome { get; init; } = string.Empty;

    public int Quantidade { get; init; }

    public int QuantidadeMinima { get; init; }

    /// <summary>Quanto falta para atingir o estoque mínimo.</summary>
    public int QuantidadeARepor { get; init; }
}

/// <summary>Consolidação dos royalties gerados em um período.</summary>
public class ResumoRoyaltiesResposta
{
    public int TotalCobrancas { get; init; }

    public decimal FaturamentoBaseTotal { get; init; }

    public decimal TotalRoyalties { get; init; }

    public decimal TotalTaxasFranquia { get; init; }

    public decimal ValorTotalGerado { get; init; }

    public decimal ValorPago { get; init; }

    public decimal ValorEmAberto { get; init; }
}

/// <summary>Contagem de chamados agrupada por situação.</summary>
public class ChamadosPorSituacaoResposta
{
    public SituacaoChamado Situacao { get; init; }

    public int Quantidade { get; init; }
}
