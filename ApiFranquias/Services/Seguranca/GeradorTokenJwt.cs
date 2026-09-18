using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConectaFranquias.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ConectaFranquias.Api.Services.Seguranca;

public interface IGeradorTokenJwt
{
    (string Token, DateTime ExpiraEm) Gerar(Usuario usuario);
}

public class GeradorTokenJwt(IOptions<OpcoesJwt> opcoes) : IGeradorTokenJwt
{
    private readonly OpcoesJwt _opcoes = opcoes.Value;

    public (string Token, DateTime ExpiraEm) Gerar(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_opcoes.MinutosValidade);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.UsuarioId.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(TiposClaim.UsuarioId, usuario.UsuarioId.ToString()),
            new(TiposClaim.Perfil, usuario.Perfil.Tipo.ToString()),
            // A claim de papel alimenta as políticas de autorização do ASP.NET Core.
            new(ClaimTypes.Role, usuario.Perfil.Tipo.ToString())
        };

        if (usuario.UnidadeId.HasValue)
        {
            claims.Add(new Claim(TiposClaim.UnidadeId, usuario.UnidadeId.Value.ToString()));
        }

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opcoes.ChaveSecreta));

        var token = new JwtSecurityToken(
            issuer: _opcoes.Emissor,
            audience: _opcoes.Audiencia,
            claims: claims,
            expires: expiraEm,
            signingCredentials: new SigningCredentials(chave, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
