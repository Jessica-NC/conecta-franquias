using System.ComponentModel.DataAnnotations;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Validacoes;

namespace ConectaFranquias.Api.DTOs;

/// <summary>Cadastro ou atualização de uma categoria do catálogo.</summary>
public class CategoriaRequisicao
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(80, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(200, MinimumLength = 3)]
    public string Descricao { get; set; } = string.Empty;
}

public class CategoriaResposta
{
    public int CategoriaId { get; init; }

    public string Nome { get; init; } = string.Empty;

    public string Descricao { get; init; } = string.Empty;

    public int TotalProdutos { get; init; }
}

/// <summary>Cadastro de um item do catálogo padronizado da rede.</summary>
public class CriarProdutoServicoRequisicao
{
    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "O código é obrigatório.")]
    [StringLength(20, MinimumLength = 2)]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(400, MinimumLength = 3)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo é obrigatório.")]
    public TipoProdutoServico Tipo { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço base deve ser maior que zero.")]
    public decimal PrecoBase { get; set; }
}

/// <summary>Atualização de um item do catálogo. O código não é editável.</summary>
public class AtualizarProdutoServicoRequisicao
{
    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(400, MinimumLength = 3)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo é obrigatório.")]
    public TipoProdutoServico Tipo { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço base deve ser maior que zero.")]
    public decimal PrecoBase { get; set; }
}

public class ProdutoServicoResposta
{
    public int ProdutoServicoId { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string Nome { get; init; } = string.Empty;

    public string Descricao { get; init; } = string.Empty;

    public TipoProdutoServico Tipo { get; init; }

    public decimal PrecoBase { get; init; }

    public bool Ativo { get; init; }

    public bool ControlaEstoque { get; init; }

    public int CategoriaId { get; init; }

    public string CategoriaNome { get; init; } = string.Empty;
}

/// <summary>Filtros de busca do catálogo: nome, categoria e situação.</summary>
public class FiltroProdutosServicos : ParametrosConsulta
{
    public int? CategoriaId { get; set; }

    public TipoProdutoServico? Tipo { get; set; }

    public bool? Ativo { get; set; }
}

/// <summary>Cadastro de um fornecedor homologado.</summary>
public class CriarFornecedorRequisicao
{
    [Required(ErrorMessage = "A razão social é obrigatória.")]
    [StringLength(150, MinimumLength = 3)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome fantasia é obrigatório.")]
    [StringLength(120, MinimumLength = 2)]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CNPJ é obrigatório.")]
    [Cnpj]
    public string Cnpj { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [StringLength(20, MinimumLength = 8)]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [StringLength(80)]
    public string Cidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Informe a sigla do estado com 2 letras.")]
    public string Estado { get; set; } = string.Empty;
}

/// <summary>Atualização de um fornecedor. O CNPJ não é editável.</summary>
public class AtualizarFornecedorRequisicao
{
    [Required(ErrorMessage = "A razão social é obrigatória.")]
    [StringLength(150, MinimumLength = 3)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome fantasia é obrigatório.")]
    [StringLength(120, MinimumLength = 2)]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [StringLength(20, MinimumLength = 8)]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [StringLength(80)]
    public string Cidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Informe a sigla do estado com 2 letras.")]
    public string Estado { get; set; } = string.Empty;
}

/// <summary>Vínculo comercial entre um fornecedor e um item do catálogo.</summary>
public class VincularProdutoRequisicao
{
    [Required(ErrorMessage = "O produto/serviço é obrigatório.")]
    public int ProdutoServicoId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço de custo deve ser maior que zero.")]
    public decimal PrecoCusto { get; set; }
}

public class FornecedorProdutoResposta
{
    public int ProdutoServicoId { get; init; }

    public string ProdutoCodigo { get; init; } = string.Empty;

    public string ProdutoNome { get; init; } = string.Empty;

    public decimal PrecoCusto { get; init; }
}

public class FornecedorResposta
{
    public int FornecedorId { get; init; }

    public string RazaoSocial { get; init; } = string.Empty;

    public string NomeFantasia { get; init; } = string.Empty;

    public string Cnpj { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Telefone { get; init; } = string.Empty;

    public string Cidade { get; init; } = string.Empty;

    public string Estado { get; init; } = string.Empty;

    public bool Ativo { get; init; }

    public IReadOnlyList<FornecedorProdutoResposta> Produtos { get; init; } = [];
}

/// <summary>Filtros de busca de fornecedores: nome, CNPJ e situação.</summary>
public class FiltroFornecedores : ParametrosConsulta
{
    public bool? Ativo { get; set; }

    [StringLength(18)]
    public string? Cnpj { get; set; }

    /// <summary>Lista apenas fornecedores que atendem o item informado.</summary>
    public int? ProdutoServicoId { get; set; }
}
