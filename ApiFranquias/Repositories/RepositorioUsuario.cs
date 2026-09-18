using ConectaFranquias.Api.Data;
using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Extensoes;
using Microsoft.EntityFrameworkCore;

namespace ConectaFranquias.Api.Repositories;

public interface IRepositorioUsuario : IRepositorioBase<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancelamento = default);

    Task<Usuario?> ObterComRelacionamentosAsync(int usuarioId, CancellationToken cancelamento = default);

    Task<bool> EmailJaCadastradoAsync(string email, int? ignorarUsuarioId = null, CancellationToken cancelamento = default);

    Task<(IReadOnlyList<Usuario> Itens, int Total)> BuscarAsync(FiltroUsuarios filtro, CancellationToken cancelamento = default);

    Task<Perfil?> ObterPerfilPorTipoAsync(TipoPerfil tipo, CancellationToken cancelamento = default);
}

public class RepositorioUsuario(ContextoFranquias contexto)
    : RepositorioBase<Usuario>(contexto), IRepositorioUsuario
{
    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancelamento = default)
    {
        var emailNormalizado = email.Trim().ToLowerInvariant();

        return await Conjunto
            .Include(usuario => usuario.Perfil)
            .Include(usuario => usuario.Unidade)
            .FirstOrDefaultAsync(usuario => usuario.Email == emailNormalizado, cancelamento);
    }

    public async Task<Usuario?> ObterComRelacionamentosAsync(int usuarioId, CancellationToken cancelamento = default) =>
        await Conjunto
            .Include(usuario => usuario.Perfil)
            .Include(usuario => usuario.Unidade)
            .FirstOrDefaultAsync(usuario => usuario.UsuarioId == usuarioId, cancelamento);

    public async Task<bool> EmailJaCadastradoAsync(
        string email,
        int? ignorarUsuarioId = null,
        CancellationToken cancelamento = default)
    {
        var emailNormalizado = email.Trim().ToLowerInvariant();

        return await Conjunto
            .AsNoTracking()
            .AnyAsync(
                usuario => usuario.Email == emailNormalizado &&
                           (ignorarUsuarioId == null || usuario.UsuarioId != ignorarUsuarioId),
                cancelamento);
    }

    public async Task<(IReadOnlyList<Usuario> Itens, int Total)> BuscarAsync(
        FiltroUsuarios filtro,
        CancellationToken cancelamento = default)
    {
        var termo = filtro.Termo?.Trim().ToLowerInvariant();

        var consulta = ConsultaSemRastreamento()
            .Include(usuario => usuario.Perfil)
            .Include(usuario => usuario.Unidade)
            .FiltrarQuando(filtro.Ativo.HasValue, usuario => usuario.Ativo == filtro.Ativo!.Value)
            .FiltrarQuando(filtro.UnidadeId.HasValue, usuario => usuario.UnidadeId == filtro.UnidadeId)
            .FiltrarQuando(filtro.Perfil.HasValue, usuario => usuario.Perfil.Tipo == filtro.Perfil!.Value)
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(termo),
                usuario => usuario.Nome.ToLower().Contains(termo!) || usuario.Email.Contains(termo!))
            .OrdenarPorCampo(filtro.OrdenarPor, filtro.Descendente, nameof(Usuario.Nome));

        return await consulta.ObterPaginaAsync(filtro, cancelamento);
    }

    public async Task<Perfil?> ObterPerfilPorTipoAsync(TipoPerfil tipo, CancellationToken cancelamento = default) =>
        await Contexto.Perfis.FirstOrDefaultAsync(perfil => perfil.Tipo == tipo, cancelamento);
}
