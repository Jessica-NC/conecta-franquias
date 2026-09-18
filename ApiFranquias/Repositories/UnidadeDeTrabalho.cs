using ConectaFranquias.Api.Data;

namespace ConectaFranquias.Api.Repositories;

public interface IUnidadeDeTrabalho
{
    Task SalvarAlteracoesAsync(CancellationToken cancelamento = default);

    Task<T> ExecutarEmTransacaoAsync<T>(Func<Task<T>> operacao, CancellationToken cancelamento = default);
}

public class UnidadeDeTrabalho(ContextoFranquias contexto) : IUnidadeDeTrabalho
{
    public Task SalvarAlteracoesAsync(CancellationToken cancelamento = default) =>
        contexto.SaveChangesAsync(cancelamento);

    public async Task<T> ExecutarEmTransacaoAsync<T>(Func<Task<T>> operacao, CancellationToken cancelamento = default)
    {
        await using var transacao = await contexto.Database.BeginTransactionAsync(cancelamento);

        try
        {
            var resultado = await operacao();
            await contexto.SaveChangesAsync(cancelamento);
            await transacao.CommitAsync(cancelamento);
            return resultado;
        }
        catch
        {
            await transacao.RollbackAsync(cancelamento);
            throw;
        }
    }
}
