using ConectaFranquias.Api.Data;
using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Extensoes;
using ConectaFranquias.Api.Validacoes;
using Microsoft.EntityFrameworkCore;

namespace ConectaFranquias.Api.Repositories;

public interface IRepositorioFranqueadora : IRepositorioBase<Franqueadora>
{
    Task<bool> CnpjJaCadastradoAsync(string cnpj, CancellationToken cancelamento = default);

    Task<IReadOnlyList<Franqueadora>> ListarComUnidadesAsync(CancellationToken cancelamento = default);

    Task<Franqueadora?> ObterComUnidadesAsync(int franqueadoraId, CancellationToken cancelamento = default);

    Task<int> ContarUnidadesAsync(int franqueadoraId, CancellationToken cancelamento = default);
}

public class RepositorioFranqueadora(ContextoFranquias contexto)
    : RepositorioBase<Franqueadora>(contexto), IRepositorioFranqueadora
{
    public async Task<bool> CnpjJaCadastradoAsync(string cnpj, CancellationToken cancelamento = default)
    {
        var cnpjFormatado = DocumentoValidador.FormatarCnpj(cnpj);

        return await Conjunto
            .AsNoTracking()
            .AnyAsync(franqueadora => franqueadora.Cnpj == cnpjFormatado, cancelamento);
    }

    public async Task<IReadOnlyList<Franqueadora>> ListarComUnidadesAsync(CancellationToken cancelamento = default) =>
        await ConsultaSemRastreamento()
            .Include(franqueadora => franqueadora.Unidades)
            .OrderBy(franqueadora => franqueadora.NomeFantasia)
            .ToListAsync(cancelamento);

    public async Task<Franqueadora?> ObterComUnidadesAsync(int franqueadoraId, CancellationToken cancelamento = default) =>
        await Conjunto
            .Include(franqueadora => franqueadora.Unidades)
            .FirstOrDefaultAsync(franqueadora => franqueadora.FranqueadoraId == franqueadoraId, cancelamento);

    public async Task<int> ContarUnidadesAsync(int franqueadoraId, CancellationToken cancelamento = default) =>
        await Contexto.Unidades
            .AsNoTracking()
            .CountAsync(unidade => unidade.FranqueadoraId == franqueadoraId, cancelamento);
}

public interface IRepositorioFranqueado : IRepositorioBase<Franqueado>
{
    Task<bool> CpfJaCadastradoAsync(string cpf, CancellationToken cancelamento = default);

    Task<Franqueado?> ObterComUnidadesAsync(int franqueadoId, CancellationToken cancelamento = default);

    Task<(IReadOnlyList<Franqueado> Itens, int Total)> BuscarAsync(FiltroFranqueados filtro, CancellationToken cancelamento = default);

    Task<int> ContarUnidadesAsync(int franqueadoId, CancellationToken cancelamento = default);
}

public class RepositorioFranqueado(ContextoFranquias contexto)
    : RepositorioBase<Franqueado>(contexto), IRepositorioFranqueado
{
    public async Task<bool> CpfJaCadastradoAsync(string cpf, CancellationToken cancelamento = default)
    {
        var cpfFormatado = DocumentoValidador.FormatarCpf(cpf);

        return await Conjunto
            .AsNoTracking()
            .AnyAsync(franqueado => franqueado.Cpf == cpfFormatado, cancelamento);
    }

    public async Task<Franqueado?> ObterComUnidadesAsync(int franqueadoId, CancellationToken cancelamento = default) =>
        await Conjunto
            .Include(franqueado => franqueado.Unidades)
            .FirstOrDefaultAsync(franqueado => franqueado.FranqueadoId == franqueadoId, cancelamento);

    public async Task<(IReadOnlyList<Franqueado> Itens, int Total)> BuscarAsync(
        FiltroFranqueados filtro,
        CancellationToken cancelamento = default)
    {
        var termo = filtro.Termo?.Trim().ToLowerInvariant();
        var estado = filtro.Estado?.Trim().ToUpperInvariant();

        var consulta = ConsultaSemRastreamento()
            .Include(franqueado => franqueado.Unidades)
            .FiltrarQuando(!string.IsNullOrWhiteSpace(estado), franqueado => franqueado.Estado == estado)
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(termo),
                franqueado => franqueado.Nome.ToLower().Contains(termo!) ||
                              franqueado.Email.Contains(termo!) ||
                              franqueado.Cpf.Contains(termo!))
            .OrdenarPorCampo(filtro.OrdenarPor, filtro.Descendente, nameof(Franqueado.Nome));

        return await consulta.ObterPaginaAsync(filtro, cancelamento);
    }

    public async Task<int> ContarUnidadesAsync(int franqueadoId, CancellationToken cancelamento = default) =>
        await Contexto.Unidades
            .AsNoTracking()
            .CountAsync(unidade => unidade.FranqueadoId == franqueadoId, cancelamento);
}

public interface IRepositorioUnidade : IRepositorioBase<UnidadeFranqueada>
{
    Task<bool> CnpjJaCadastradoAsync(string cnpj, CancellationToken cancelamento = default);

    Task<bool> CodigoJaCadastradoAsync(string codigo, CancellationToken cancelamento = default);

    Task<UnidadeFranqueada?> ObterComRelacionamentosAsync(int unidadeId, CancellationToken cancelamento = default);

    Task<(IReadOnlyList<UnidadeFranqueada> Itens, int Total)> BuscarAsync(FiltroUnidades filtro, CancellationToken cancelamento = default);

    Task<Responsavel?> ObterResponsavelAsync(int responsavelId, CancellationToken cancelamento = default);

    Task<bool> ResponsavelJaCadastradoAsync(int unidadeId, string cpf, CancellationToken cancelamento = default);

    Task AdicionarResponsavelAsync(Responsavel responsavel, CancellationToken cancelamento = default);
}

public class RepositorioUnidade(ContextoFranquias contexto)
    : RepositorioBase<UnidadeFranqueada>(contexto), IRepositorioUnidade
{
    public async Task<bool> CnpjJaCadastradoAsync(string cnpj, CancellationToken cancelamento = default)
    {
        var cnpjFormatado = DocumentoValidador.FormatarCnpj(cnpj);

        return await Conjunto
            .AsNoTracking()
            .AnyAsync(unidade => unidade.Cnpj == cnpjFormatado, cancelamento);
    }

    public async Task<bool> CodigoJaCadastradoAsync(string codigo, CancellationToken cancelamento = default)
    {
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();

        return await Conjunto
            .AsNoTracking()
            .AnyAsync(unidade => unidade.Codigo == codigoNormalizado, cancelamento);
    }

    public async Task<UnidadeFranqueada?> ObterComRelacionamentosAsync(
        int unidadeId,
        CancellationToken cancelamento = default) =>
        await Conjunto
            .Include(unidade => unidade.Franqueado)
            .Include(unidade => unidade.Responsaveis)
            .FirstOrDefaultAsync(unidade => unidade.UnidadeId == unidadeId, cancelamento);

    public async Task<(IReadOnlyList<UnidadeFranqueada> Itens, int Total)> BuscarAsync(
        FiltroUnidades filtro,
        CancellationToken cancelamento = default)
    {
        var termo = filtro.Termo?.Trim().ToLowerInvariant();
        var cidade = filtro.Cidade?.Trim().ToLowerInvariant();
        var cnpj = string.IsNullOrWhiteSpace(filtro.Cnpj) ? null : DocumentoValidador.ApenasDigitos(filtro.Cnpj);
        var responsavel = filtro.Responsavel?.Trim().ToLowerInvariant();

        var consulta = ConsultaSemRastreamento()
            .Include(unidade => unidade.Franqueado)
            .Include(unidade => unidade.Responsaveis)
            .FiltrarQuando(filtro.Situacao.HasValue, unidade => unidade.Situacao == filtro.Situacao!.Value)
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(cidade),
                unidade => unidade.Endereco.Cidade.ToLower().Contains(cidade!))
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(cnpj),
                unidade => unidade.Cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Contains(cnpj!))
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(responsavel),
                unidade => unidade.Franqueado.Nome.ToLower().Contains(responsavel!) ||
                           unidade.Responsaveis.Any(item => item.Nome.ToLower().Contains(responsavel!)))
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(termo),
                unidade => unidade.NomeFantasia.ToLower().Contains(termo!) ||
                           unidade.RazaoSocial.ToLower().Contains(termo!) ||
                           unidade.Codigo.ToLower().Contains(termo!) ||
                           unidade.Endereco.Cidade.ToLower().Contains(termo!) ||
                           unidade.Cnpj.Contains(termo!))
            .OrdenarPorCampo(filtro.OrdenarPor, filtro.Descendente, nameof(UnidadeFranqueada.NomeFantasia));

        return await consulta.ObterPaginaAsync(filtro, cancelamento);
    }

    public async Task<Responsavel?> ObterResponsavelAsync(int responsavelId, CancellationToken cancelamento = default) =>
        await Contexto.Responsaveis
            .FirstOrDefaultAsync(responsavel => responsavel.ResponsavelId == responsavelId, cancelamento);

    public async Task<bool> ResponsavelJaCadastradoAsync(int unidadeId, string cpf, CancellationToken cancelamento = default)
    {
        var cpfFormatado = DocumentoValidador.FormatarCpf(cpf);

        return await Contexto.Responsaveis
            .AsNoTracking()
            .AnyAsync(
                responsavel => responsavel.UnidadeId == unidadeId && responsavel.Cpf == cpfFormatado,
                cancelamento);
    }

    public async Task AdicionarResponsavelAsync(Responsavel responsavel, CancellationToken cancelamento = default) =>
        await Contexto.Responsaveis.AddAsync(responsavel, cancelamento);
}
