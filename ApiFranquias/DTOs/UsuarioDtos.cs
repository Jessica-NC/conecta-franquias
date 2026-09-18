using System.ComponentModel.DataAnnotations;
using ConectaFranquias.Api.Entities;

namespace ConectaFranquias.Api.DTOs;

/// <summary>Credenciais enviadas no login.</summary>
public class LoginRequisicao
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>Token emitido após um login bem-sucedido.</summary>
public class LoginResposta
{
    public string Token { get; init; } = string.Empty;

    public DateTime ExpiraEm { get; init; }

    public int UsuarioId { get; init; }

    public string Nome { get; init; } = string.Empty;

    public TipoPerfil Perfil { get; init; }

    public int? UnidadeId { get; init; }
}

/// <summary>Cadastro de um novo usuário do sistema.</summary>
public class CriarUsuarioRequisicao : IValidatableObject
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "O perfil é obrigatório.")]
    public TipoPerfil Perfil { get; set; }

    /// <summary>Obrigatório para gestores e operadores; vazio para administradores.</summary>
    public int? UnidadeId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto) =>
        ValidacaoUsuario.ValidarVinculoDeUnidade(Perfil, UnidadeId);
}

/// <summary>Atualização cadastral de um usuário existente.</summary>
public class AtualizarUsuarioRequisicao : IValidatableObject
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O perfil é obrigatório.")]
    public TipoPerfil Perfil { get; set; }

    public int? UnidadeId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto) =>
        ValidacaoUsuario.ValidarVinculoDeUnidade(Perfil, UnidadeId);
}

/// <summary>Representação pública de um usuário — nunca expõe o hash da senha.</summary>
public class UsuarioResposta
{
    public int UsuarioId { get; init; }

    public string Nome { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public TipoPerfil Perfil { get; init; }

    public int? UnidadeId { get; init; }

    public string? UnidadeNome { get; init; }

    public bool Ativo { get; init; }

    public DateTime DataCadastro { get; init; }
}

/// <summary>Filtros aceitos na listagem de usuários.</summary>
public class FiltroUsuarios : ParametrosConsulta
{
    public TipoPerfil? Perfil { get; set; }

    public int? UnidadeId { get; set; }

    public bool? Ativo { get; set; }
}

/// <summary>
/// Regra de consistência compartilhada entre a criação e a atualização de usuários.
/// </summary>
internal static class ValidacaoUsuario
{
    public static IEnumerable<ValidationResult> ValidarVinculoDeUnidade(TipoPerfil perfil, int? unidadeId)
    {
        if (perfil is TipoPerfil.GestorUnidade or TipoPerfil.Operador && unidadeId is null)
        {
            yield return new ValidationResult(
                "Usuários com perfil de gestor ou operador devem estar vinculados a uma unidade.",
                [nameof(CriarUsuarioRequisicao.UnidadeId)]);
        }

        if (perfil == TipoPerfil.Administrador && unidadeId is not null)
        {
            yield return new ValidationResult(
                "Usuários administradores da franqueadora não podem ser vinculados a uma unidade.",
                [nameof(CriarUsuarioRequisicao.UnidadeId)]);
        }
    }
}
