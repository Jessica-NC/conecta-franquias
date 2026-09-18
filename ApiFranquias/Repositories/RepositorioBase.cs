using ConectaFranquias.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ConectaFranquias.Api.Repositories;

public interface IRepositorioBase<T> where T : class
{
    Task<T?> ObterPorIdAsync(int id, CancellationToken cancelamento = default);

    Task AdicionarAsync(T entidade, CancellationToken cancelamento = default);

    Task<bool> ExisteAsync(int id, CancellationToken cancelamento = default);
}

public abstract class RepositorioBase<T>(ContextoFranquias contexto) : IRepositorioBase<T> where T : class
{
    protected ContextoFranquias Contexto { get; } = contexto;

    protected DbSet<T> Conjunto { get; } = contexto.Set<T>();

    public async Task<T?> ObterPorIdAsync(int id, CancellationToken cancelamento = default) =>
        await Conjunto.FindAsync([id], cancelamento);

    public async Task AdicionarAsync(T entidade, CancellationToken cancelamento = default) =>
        await Conjunto.AddAsync(entidade, cancelamento);

    public async Task<bool> ExisteAsync(int id, CancellationToken cancelamento = default) =>
        await Conjunto.FindAsync([id], cancelamento) is not null;

    protected IQueryable<T> ConsultaSemRastreamento() => Conjunto.AsNoTracking();
}
