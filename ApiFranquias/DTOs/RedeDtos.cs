using System.ComponentModel.DataAnnotations;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Validacoes;

namespace ConectaFranquias.Api.DTOs;

/// <summary>Cadastro da rede/franqueadora.</summary>
public class CriarFranqueadoraRequisicao
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

    [Required(ErrorMessage = "A data de fundação é obrigatória.")]
    public DateTime DataFundacao { get; set; }
}

/// <summary>Atualização da franqueadora. O CNPJ não é editável.</summary>
public class AtualizarFranqueadoraRequisicao
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
}

public class FranqueadoraResposta
{
    public int FranqueadoraId { get; init; }

    public string RazaoSocial { get; init; } = string.Empty;

    public string NomeFantasia { get; init; } = string.Empty;

    public string Cnpj { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Telefone { get; init; } = string.Empty;

    public DateTime DataFundacao { get; init; }

    public int TotalUnidades { get; init; }
}

/// <summary>Cadastro do franqueado titular do contrato de unidade.</summary>
public class CriarFranqueadoRequisicao
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [Cpf]
    public string Cpf { get; set; } = string.Empty;

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

/// <summary>Atualização do franqueado. O CPF não é editável.</summary>
public class AtualizarFranqueadoRequisicao
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

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

public class FranqueadoResposta
{
    public int FranqueadoId { get; init; }

    public string Nome { get; init; } = string.Empty;

    public string Cpf { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Telefone { get; init; } = string.Empty;

    public string Cidade { get; init; } = string.Empty;

    public string Estado { get; init; } = string.Empty;

    public int TotalUnidades { get; init; }
}

public class FiltroFranqueados : ParametrosConsulta
{
    [StringLength(2)]
    public string? Estado { get; set; }
}

/// <summary>Endereço da unidade, usado na entrada e na saída.</summary>
public class EnderecoDto
{
    [Required(ErrorMessage = "O logradouro é obrigatório.")]
    [StringLength(150)]
    public string Logradouro { get; set; } = string.Empty;

    [Required(ErrorMessage = "O número é obrigatório.")]
    [StringLength(15)]
    public string Numero { get; set; } = string.Empty;

    [Required(ErrorMessage = "O bairro é obrigatório.")]
    [StringLength(80)]
    public string Bairro { get; set; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [StringLength(80)]
    public string Cidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Informe a sigla do estado com 2 letras.")]
    public string Estado { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CEP é obrigatório.")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Informe um CEP válido no formato 00000-000.")]
    public string Cep { get; set; } = string.Empty;
}

/// <summary>Cadastro de uma nova unidade franqueada.</summary>
public class CriarUnidadeRequisicao
{
    [Required(ErrorMessage = "A franqueadora é obrigatória.")]
    public int FranqueadoraId { get; set; }

    [Required(ErrorMessage = "O franqueado é obrigatório.")]
    public int FranqueadoId { get; set; }

    [Required(ErrorMessage = "O código da unidade é obrigatório.")]
    [StringLength(20, MinimumLength = 2)]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome fantasia é obrigatório.")]
    [StringLength(120, MinimumLength = 2)]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required(ErrorMessage = "A razão social é obrigatória.")]
    [StringLength(150, MinimumLength = 3)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CNPJ é obrigatório.")]
    [Cnpj]
    public string Cnpj { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [StringLength(20, MinimumLength = 8)]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "O endereço é obrigatório.")]
    public EnderecoDto Endereco { get; set; } = new();

    [Required(ErrorMessage = "A data de início do contrato é obrigatória.")]
    public DateTime DataInicioContrato { get; set; }

    [Range(0, 100, ErrorMessage = "O percentual de royalty deve estar entre 0 e 100.")]
    public decimal PercentualRoyalty { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "A taxa de franquia não pode ser negativa.")]
    public decimal TaxaFranquiaMensal { get; set; }
}

/// <summary>Atualização de uma unidade. Código e CNPJ não são editáveis.</summary>
public class AtualizarUnidadeRequisicao
{
    [Required(ErrorMessage = "O franqueado é obrigatório.")]
    public int FranqueadoId { get; set; }

    [Required(ErrorMessage = "O nome fantasia é obrigatório.")]
    [StringLength(120, MinimumLength = 2)]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required(ErrorMessage = "A razão social é obrigatória.")]
    [StringLength(150, MinimumLength = 3)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [StringLength(20, MinimumLength = 8)]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "O endereço é obrigatório.")]
    public EnderecoDto Endereco { get; set; } = new();

    [Range(0, 100, ErrorMessage = "O percentual de royalty deve estar entre 0 e 100.")]
    public decimal PercentualRoyalty { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "A taxa de franquia não pode ser negativa.")]
    public decimal TaxaFranquiaMensal { get; set; }
}

/// <summary>Mudança de situação contratual da unidade.</summary>
public class AlterarSituacaoUnidadeRequisicao
{
    [Required(ErrorMessage = "A situação é obrigatória.")]
    public SituacaoUnidade Situacao { get; set; }
}

public class UnidadeResposta
{
    public int UnidadeId { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string NomeFantasia { get; init; } = string.Empty;

    public string RazaoSocial { get; init; } = string.Empty;

    public string Cnpj { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Telefone { get; init; } = string.Empty;

    public EnderecoDto Endereco { get; init; } = new();

    public DateTime DataInicioContrato { get; init; }

    public decimal PercentualRoyalty { get; init; }

    public decimal TaxaFranquiaMensal { get; init; }

    public SituacaoUnidade Situacao { get; init; }

    public bool PodeRegistrarVendas { get; init; }

    public int FranqueadoraId { get; init; }

    public int FranqueadoId { get; init; }

    public string FranqueadoNome { get; init; } = string.Empty;

    public IReadOnlyList<ResponsavelResposta> Responsaveis { get; init; } = [];
}

/// <summary>Versão enxuta usada em listagens.</summary>
public class UnidadeResumoResposta
{
    public int UnidadeId { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string NomeFantasia { get; init; } = string.Empty;

    public string Cnpj { get; init; } = string.Empty;

    public string Cidade { get; init; } = string.Empty;

    public string Estado { get; init; } = string.Empty;

    public SituacaoUnidade Situacao { get; init; }

    public decimal PercentualRoyalty { get; init; }

    public string FranqueadoNome { get; init; } = string.Empty;
}

/// <summary>Filtros de busca de unidades.</summary>
public class FiltroUnidades : ParametrosConsulta
{
    public SituacaoUnidade? Situacao { get; set; }

    [StringLength(80)]
    public string? Cidade { get; set; }

    [StringLength(18)]
    public string? Cnpj { get; set; }

    /// <summary>Busca pelo nome do franqueado ou de um responsável da unidade.</summary>
    [StringLength(120)]
    public string? Responsavel { get; set; }
}

/// <summary>Cadastro de um responsável pela operação da unidade.</summary>
public class CriarResponsavelRequisicao
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [Cpf]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "O cargo é obrigatório.")]
    [StringLength(60)]
    public string Cargo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [StringLength(20, MinimumLength = 8)]
    public string Telefone { get; set; } = string.Empty;
}

/// <summary>Atualização de um responsável. O CPF não é editável.</summary>
public class AtualizarResponsavelRequisicao
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O cargo é obrigatório.")]
    [StringLength(60)]
    public string Cargo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [StringLength(20, MinimumLength = 8)]
    public string Telefone { get; set; } = string.Empty;
}

public class ResponsavelResposta
{
    public int ResponsavelId { get; init; }

    public int UnidadeId { get; init; }

    public string Nome { get; init; } = string.Empty;

    public string Cpf { get; init; } = string.Empty;

    public string Cargo { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Telefone { get; init; } = string.Empty;

    public bool Ativo { get; init; }
}
