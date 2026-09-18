using System.Linq.Expressions;
using System.Reflection;
using ConectaFranquias.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ConectaFranquias.Api.Extensoes;

public static class ConsultaExtensoes
{
    public static IQueryable<T> FiltrarQuando<T>(
        this IQueryable<T> consulta,
        bool condicao,
        Expression<Func<T, bool>> filtro) =>
        condicao ? consulta.Where(filtro) : consulta;

    public static IQueryable<T> OrdenarPorCampo<T>(
        this IQueryable<T> consulta,
        string? campo,
        bool descendente,
        string campoPadrao)
    {
        var propriedade = LocalizarPropriedade<T>(campo) ?? LocalizarPropriedade<T>(campoPadrao);

        if (propriedade is null)
        {
            return consulta;
        }

        var parametro = Expression.Parameter(typeof(T), "entidade");
        var acesso = Expression.MakeMemberAccess(parametro, propriedade);
        var seletor = Expression.Lambda(acesso, parametro);

        var metodo = descendente ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

        var chamada = Expression.Call(
            typeof(Queryable),
            metodo,
            [typeof(T), propriedade.PropertyType],
            consulta.Expression,
            Expression.Quote(seletor));

        return consulta.Provider.CreateQuery<T>(chamada);
    }

    public static async Task<(IReadOnlyList<T> Itens, int Total)> ObterPaginaAsync<T>(
        this IQueryable<T> consulta,
        ParametrosConsulta parametros,
        CancellationToken cancelamento = default)
    {
        var totalRegistros = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .Skip(parametros.RegistrosParaPular)
            .Take(parametros.TamanhoPagina)
            .ToListAsync(cancelamento);

        return (itens, totalRegistros);
    }

    public static ResultadoPaginado<TDestino> ParaResultadoPaginado<TOrigem, TDestino>(
        this (IReadOnlyList<TOrigem> Itens, int Total) pagina,
        ParametrosConsulta parametros,
        Func<TOrigem, TDestino> mapeador) =>
        new(
            pagina.Itens.Select(mapeador).ToList(),
            pagina.Total,
            parametros.Pagina,
            parametros.TamanhoPagina);

    private static PropertyInfo? LocalizarPropriedade<T>(string? nome) =>
        string.IsNullOrWhiteSpace(nome)
            ? null
            : typeof(T).GetProperty(nome, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
}
