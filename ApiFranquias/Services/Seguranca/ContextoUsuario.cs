using System.Security.Claims;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Excecoes;

namespace ConectaFranquias.Api.Services.Seguranca;

public interface IContextoUsuario
{
    int UsuarioId { get; }

    int? UnidadeId { get; }

    bool EhAdministrador { get; }

    void GarantirAcessoAUnidade(int unidadeId);

    int? ResolverUnidadeDaConsulta(int? unidadeSolicitada);
}

public class ContextoUsuario(IHttpContextAccessor acessorContexto) : IContextoUsuario
{
    private readonly ClaimsPrincipal? _usuario = acessorContexto.HttpContext?.User;

    public int UsuarioId => LerInteiro(TiposClaim.UsuarioId)
        ?? throw new ExcecaoAutorizacao("Não foi possível identificar o usuário autenticado.");

    public int? UnidadeId => LerInteiro(TiposClaim.UnidadeId);

    public bool EhAdministrador =>
        _usuario?.FindFirstValue(TiposClaim.Perfil) == nameof(TipoPerfil.Administrador);

    public void GarantirAcessoAUnidade(int unidadeId)
    {
        if (EhAdministrador)
        {
            return;
        }

        if (UnidadeId != unidadeId)
        {
            throw new ExcecaoAutorizacao("Seu perfil permite acessar apenas os dados da sua própria unidade.");
        }
    }

    public int? ResolverUnidadeDaConsulta(int? unidadeSolicitada)
    {
        if (EhAdministrador)
        {
            return unidadeSolicitada;
        }

        if (unidadeSolicitada.HasValue && unidadeSolicitada != UnidadeId)
        {
            throw new ExcecaoAutorizacao("Seu perfil permite consultar apenas os dados da sua própria unidade.");
        }

        return UnidadeId;
    }

    private int? LerInteiro(string tipoClaim) =>
        int.TryParse(_usuario?.FindFirstValue(tipoClaim), out var valor) ? valor : null;
}
