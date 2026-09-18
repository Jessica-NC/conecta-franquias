using System.ComponentModel.DataAnnotations;

namespace ConectaFranquias.Api.DTOs;

/// <summary>
/// Parâmetros de paginação, ordenação e busca compartilhados por todas as
/// consultas em lista da API.
/// </summary>
public class ParametrosConsulta
{
    private const int TamanhoMaximoPagina = 100;

    private int _tamanhoPagina = 20;

    /// <summary>Página desejada, iniciando em 1.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior ou igual a 1.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de registros por página (máximo de 100).</summary>
    [Range(1, TamanhoMaximoPagina, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina
    {
        get => _tamanhoPagina;
        set => _tamanhoPagina = value > TamanhoMaximoPagina ? TamanhoMaximoPagina : value;
    }

    /// <summary>Termo de busca livre aplicado aos campos textuais do recurso.</summary>
    [StringLength(120)]
    public string? Termo { get; set; }

    /// <summary>Campo usado para ordenar o resultado.</summary>
    [StringLength(50)]
    public string? OrdenarPor { get; set; }

    /// <summary>Quando verdadeiro, ordena de forma decrescente.</summary>
    public bool Descendente { get; set; }

    /// <summary>Quantidade de registros a pular, derivada da página atual.</summary>
    public int RegistrosParaPular => (Pagina - 1) * TamanhoPagina;
}

/// <summary>
/// Envelope padrão das consultas paginadas.
/// </summary>
/// <typeparam name="T">Tipo do item retornado.</typeparam>
public class ResultadoPaginado<T>(IReadOnlyList<T> itens, int totalRegistros, int pagina, int tamanhoPagina)
{
    public IReadOnlyList<T> Itens { get; } = itens;

    public int TotalRegistros { get; } = totalRegistros;

    public int Pagina { get; } = pagina;

    public int TamanhoPagina { get; } = tamanhoPagina;

    public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina);

    public bool TemProximaPagina => Pagina < TotalPaginas;
}

/// <summary>
/// Corpo padronizado devolvido em qualquer resposta de erro da API.
/// </summary>
public class RespostaErro(int status, string titulo, string mensagem, IDictionary<string, string[]>? erros = null)
{
    public int Status { get; } = status;

    public string Titulo { get; } = titulo;

    public string Mensagem { get; } = mensagem;

    /// <summary>Erros por campo, preenchido apenas em falhas de validação.</summary>
    public IDictionary<string, string[]>? Erros { get; } = erros;
}

/// <summary>Intervalo de datas comum aos relatórios gerenciais.</summary>
public class FiltroPeriodo : IValidatableObject
{
    [Required(ErrorMessage = "A data de início é obrigatória.")]
    public DateTime DataInicio { get; set; }

    [Required(ErrorMessage = "A data de fim é obrigatória.")]
    public DateTime DataFim { get; set; }

    /// <summary>Quando informada, restringe o relatório a uma única unidade.</summary>
    public int? UnidadeId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (DataFim.Date < DataInicio.Date)
        {
            yield return new ValidationResult(
                "A data de fim deve ser igual ou posterior à data de início.",
                [nameof(DataFim)]);
        }
    }
}
