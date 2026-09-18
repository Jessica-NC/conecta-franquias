using System.ComponentModel.DataAnnotations;

namespace ConectaFranquias.Api.Services.Seguranca;

public class OpcoesJwt
{
    public const string SecaoConfiguracao = "Jwt";

    [Required]
    [MinLength(32, ErrorMessage = "A chave do JWT deve ter no mínimo 32 caracteres.")]
    public string ChaveSecreta { get; set; } = string.Empty;

    [Required]
    public string Emissor { get; set; } = string.Empty;

    [Required]
    public string Audiencia { get; set; } = string.Empty;

    [Range(5, 1440)]
    public int MinutosValidade { get; set; } = 480;
}

public static class TiposClaim
{
    public const string UsuarioId = "usuario_id";

    public const string UnidadeId = "unidade_id";

    public const string Perfil = "perfil";
}

public static class Politicas
{
    public const string SomenteAdministrador = "SomenteAdministrador";

    public const string AdministradorOuGestor = "AdministradorOuGestor";
}
