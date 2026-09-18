using System.ComponentModel.DataAnnotations;
using ConectaFranquias.Api.Entities;

namespace ConectaFranquias.Api.DTOs;

/// <summary>Abertura do controle de estoque de um item em uma unidade.</summary>
public class CriarEstoqueRequisicao
{
    [Required(ErrorMessage = "A unidade é obrigatória.")]
    public int UnidadeId { get; set; }

    [Required(ErrorMessage = "O produto é obrigatório.")]
    public int ProdutoServicoId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "A quantidade mínima não pode ser negativa.")]
    public int QuantidadeMinima { get; set; }
}

/// <summary>Entrada ou saída de estoque.</summary>
public class MovimentarEstoqueRequisicao
{
    [Required(ErrorMessage = "O tipo de movimentação é obrigatório.")]
    public TipoMovimentacaoEstoque Tipo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade movimentada deve ser maior que zero.")]
    public int Quantidade { get; set; }

    [Required(ErrorMessage = "O motivo é obrigatório.")]
    [StringLength(200, MinimumLength = 3)]
    public string Motivo { get; set; } = string.Empty;
}

public class EstoqueResposta
{
    public int EstoqueId { get; init; }

    public int UnidadeId { get; init; }

    public string UnidadeNome { get; init; } = string.Empty;

    public int ProdutoServicoId { get; init; }

    public string ProdutoCodigo { get; init; } = string.Empty;

    public string ProdutoNome { get; init; } = string.Empty;

    public int Quantidade { get; init; }

    public int QuantidadeMinima { get; init; }

    public bool AbaixoDoMinimo { get; init; }

    public DateTime DataAtualizacao { get; init; }
}

public class MovimentacaoEstoqueResposta
{
    public int MovimentacaoEstoqueId { get; init; }

    public int EstoqueId { get; init; }

    public string ProdutoNome { get; init; } = string.Empty;

    public TipoMovimentacaoEstoque Tipo { get; init; }

    public int Quantidade { get; init; }

    public int QuantidadeAnterior { get; init; }

    public int QuantidadeResultante { get; init; }

    public string Motivo { get; init; } = string.Empty;

    public int? VendaId { get; init; }

    public DateTime DataMovimentacao { get; init; }
}

public class FiltroEstoques : ParametrosConsulta
{
    public int? UnidadeId { get; set; }

    public int? ProdutoServicoId { get; set; }

    /// <summary>Quando verdadeiro, retorna apenas itens abaixo do estoque mínimo.</summary>
    public bool? AbaixoDoMinimo { get; set; }
}

public class FiltroMovimentacoes : ParametrosConsulta
{
    public int? UnidadeId { get; set; }

    public int? ProdutoServicoId { get; set; }

    public TipoMovimentacaoEstoque? Tipo { get; set; }
}

/// <summary>Item informado no registro de uma venda.</summary>
public class ItemVendaRequisicao
{
    [Required(ErrorMessage = "O produto/serviço é obrigatório.")]
    public int ProdutoServicoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }
}

/// <summary>
/// Registro de uma venda. O valor total nunca é informado: é calculado a partir
/// dos itens, quantidades e preços do catálogo.
/// </summary>
public class CriarVendaRequisicao : IValidatableObject
{
    [Required(ErrorMessage = "A unidade é obrigatória.")]
    public int UnidadeId { get; set; }

    [Required(ErrorMessage = "A forma de pagamento é obrigatória.")]
    public FormaPagamento FormaPagamento { get; set; }

    /// <summary>
    /// Quando verdadeiro, a venda já nasce confirmada e o estoque é baixado
    /// imediatamente. Caso contrário, fica pendente aguardando confirmação.
    /// </summary>
    public bool ConfirmarImediatamente { get; set; } = true;

    [Required(ErrorMessage = "A venda deve possuir itens.")]
    [MinLength(1, ErrorMessage = "A venda deve possuir ao menos um item.")]
    public List<ItemVendaRequisicao> Itens { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        var duplicados = Itens
            .GroupBy(item => item.ProdutoServicoId)
            .Where(grupo => grupo.Count() > 1)
            .Select(grupo => grupo.Key)
            .ToList();

        if (duplicados.Count > 0)
        {
            yield return new ValidationResult(
                $"Há itens repetidos na venda (produto(s): {string.Join(", ", duplicados)}). Some as quantidades em um único item.",
                [nameof(Itens)]);
        }
    }
}

public class ItemVendaResposta
{
    public int ProdutoServicoId { get; init; }

    public string ProdutoCodigo { get; init; } = string.Empty;

    public string ProdutoNome { get; init; } = string.Empty;

    public int Quantidade { get; init; }

    public decimal PrecoUnitario { get; init; }

    public decimal ValorTotal { get; init; }
}

public class VendaResposta
{
    public int VendaId { get; init; }

    public string NumeroVenda { get; init; } = string.Empty;

    public int UnidadeId { get; init; }

    public string UnidadeNome { get; init; } = string.Empty;

    public DateTime DataVenda { get; init; }

    public FormaPagamento FormaPagamento { get; init; }

    public SituacaoVenda Situacao { get; init; }

    public decimal ValorTotal { get; init; }

    public IReadOnlyList<ItemVendaResposta> Itens { get; init; } = [];
}

/// <summary>Versão enxuta usada em listagens.</summary>
public class VendaResumoResposta
{
    public int VendaId { get; init; }

    public string NumeroVenda { get; init; } = string.Empty;

    public int UnidadeId { get; init; }

    public string UnidadeNome { get; init; } = string.Empty;

    public DateTime DataVenda { get; init; }

    public SituacaoVenda Situacao { get; init; }

    public decimal ValorTotal { get; init; }

    public int TotalItens { get; init; }
}

/// <summary>Filtros de busca de vendas: unidade, período e situação.</summary>
public class FiltroVendas : ParametrosConsulta
{
    public int? UnidadeId { get; set; }

    public SituacaoVenda? Situacao { get; set; }

    public DateTime? DataInicio { get; set; }

    public DateTime? DataFim { get; set; }
}

/// <summary>
/// Apuração do royalty de uma unidade em uma competência. O faturamento base é
/// calculado pela API a partir das vendas confirmadas do período.
/// </summary>
public class ApurarRoyaltyRequisicao
{
    [Required(ErrorMessage = "A unidade é obrigatória.")]
    public int UnidadeId { get; set; }

    [Range(2000, 2100, ErrorMessage = "Informe um ano de competência válido.")]
    public int CompetenciaAno { get; set; }

    [Range(1, 12, ErrorMessage = "O mês de competência deve estar entre 1 e 12.")]
    public int CompetenciaMes { get; set; }

    /// <summary>
    /// Quando verdadeiro, reapura uma competência já existente em vez de recusar
    /// a operação por duplicidade.
    /// </summary>
    public bool ReapurarSeExistir { get; set; }
}

/// <summary>Baixa de pagamento de um royalty apurado.</summary>
public class RegistrarPagamentoRoyaltyRequisicao
{
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor pago deve ser maior que zero.")]
    public decimal ValorPago { get; set; }

    /// <summary>Quando omitida, assume a data atual.</summary>
    public DateTime? DataPagamento { get; set; }
}

public class RoyaltyResposta
{
    public int RoyaltyId { get; init; }

    public int UnidadeId { get; init; }

    public string UnidadeNome { get; init; } = string.Empty;

    public string Competencia { get; init; } = string.Empty;

    public DateTime DataInicio { get; init; }

    public DateTime DataFim { get; init; }

    public decimal FaturamentoBase { get; init; }

    public decimal PercentualAplicado { get; init; }

    public decimal ValorRoyalty { get; init; }

    public decimal TaxaFranquia { get; init; }

    public decimal ValorTotal { get; init; }

    public SituacaoRoyalty Situacao { get; init; }

    public DateTime DataVencimento { get; init; }

    public DateTime? DataPagamento { get; init; }

    public decimal? ValorPago { get; init; }
}

/// <summary>Consulta de valores devidos e pagos por unidade.</summary>
public class FiltroRoyalties : ParametrosConsulta
{
    public int? UnidadeId { get; set; }

    public SituacaoRoyalty? Situacao { get; set; }

    public int? CompetenciaAno { get; set; }

    public int? CompetenciaMes { get; set; }
}

/// <summary>Abertura de um chamado da unidade para a franqueadora.</summary>
public class AbrirChamadoRequisicao
{
    [Required(ErrorMessage = "A unidade é obrigatória.")]
    public int UnidadeId { get; set; }

    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(150, MinimumLength = 5)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(2000, MinimumLength = 10)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public CategoriaChamado Categoria { get; set; }

    [Required(ErrorMessage = "A prioridade é obrigatória.")]
    public PrioridadeChamado Prioridade { get; set; }
}

/// <summary>Reclassificação de um chamado ainda em aberto.</summary>
public class AtualizarChamadoRequisicao
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(150, MinimumLength = 5)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(2000, MinimumLength = 10)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public CategoriaChamado Categoria { get; set; }

    [Required(ErrorMessage = "A prioridade é obrigatória.")]
    public PrioridadeChamado Prioridade { get; set; }
}

/// <summary>Nova manifestação em um chamado, com avanço opcional de status.</summary>
public class RegistrarInteracaoRequisicao
{
    [Required(ErrorMessage = "A mensagem é obrigatória.")]
    [StringLength(2000, MinimumLength = 3)]
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>Quando informada, move o chamado para esta situação.</summary>
    public SituacaoChamado? NovaSituacao { get; set; }
}

/// <summary>Encerramento de um chamado com o registro da solução aplicada.</summary>
public class EncerrarChamadoRequisicao
{
    [Required(ErrorMessage = "A solução aplicada é obrigatória.")]
    [StringLength(2000, MinimumLength = 5)]
    public string SolucaoAplicada { get; set; } = string.Empty;
}

public class InteracaoChamadoResposta
{
    public int UsuarioId { get; init; }

    public string UsuarioNome { get; init; } = string.Empty;

    public string Mensagem { get; init; } = string.Empty;

    public SituacaoChamado SituacaoAnterior { get; init; }

    public SituacaoChamado SituacaoAtual { get; init; }

    public DateTime DataRegistro { get; init; }
}

public class ChamadoResposta
{
    public int ChamadoSuporteId { get; init; }

    public string Protocolo { get; init; } = string.Empty;

    public int UnidadeId { get; init; }

    public string UnidadeNome { get; init; } = string.Empty;

    public string Titulo { get; init; } = string.Empty;

    public string Descricao { get; init; } = string.Empty;

    public CategoriaChamado Categoria { get; init; }

    public PrioridadeChamado Prioridade { get; init; }

    public SituacaoChamado Situacao { get; init; }

    public DateTime DataAbertura { get; init; }

    public DateTime? DataEncerramento { get; init; }

    public string? SolucaoAplicada { get; init; }

    public IReadOnlyList<InteracaoChamadoResposta> Interacoes { get; init; } = [];
}

/// <summary>Versão enxuta usada em listagens.</summary>
public class ChamadoResumoResposta
{
    public int ChamadoSuporteId { get; init; }

    public string Protocolo { get; init; } = string.Empty;

    public int UnidadeId { get; init; }

    public string UnidadeNome { get; init; } = string.Empty;

    public string Titulo { get; init; } = string.Empty;

    public CategoriaChamado Categoria { get; init; }

    public PrioridadeChamado Prioridade { get; init; }

    public SituacaoChamado Situacao { get; init; }

    public DateTime DataAbertura { get; init; }
}

/// <summary>Filtros de busca de chamados: unidade, prioridade e situação.</summary>
public class FiltroChamados : ParametrosConsulta
{
    public int? UnidadeId { get; set; }

    public SituacaoChamado? Situacao { get; set; }

    public PrioridadeChamado? Prioridade { get; set; }

    public CategoriaChamado? Categoria { get; set; }

    /// <summary>Quando verdadeiro, retorna apenas chamados ainda não encerrados.</summary>
    public bool? SomenteEmAberto { get; set; }
}
