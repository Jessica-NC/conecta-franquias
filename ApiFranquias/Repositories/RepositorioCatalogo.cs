using ConectaFranquias.Api.Data;
using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Extensoes;
using ConectaFranquias.Api.Validacoes;
using Microsoft.EntityFrameworkCore;

namespace ConectaFranquias.Api.Repositories;

public interface IRepositorioCatalogo : IRepositorioBase<ProdutoServico>
{
    Task<ProdutoServico?> ObterComCategoriaAsync(int produtoServicoId, CancellationToken cancelamento = default);

    Task<IReadOnlyList<ProdutoServico>> ObterVariosPorIdAsync(IEnumerable<int> identificadores, CancellationToken cancelamento = default);

    Task<bool> CodigoJaCadastradoAsync(string codigo, CancellationToken cancelamento = default);

    Task<(IReadOnlyList<ProdutoServico> Itens, int Total)> BuscarAsync(FiltroProdutosServicos filtro, CancellationToken cancelamento = default);

    Task<Categoria?> ObterCategoriaAsync(int categoriaId, CancellationToken cancelamento = default);

    Task<bool> CategoriaJaCadastradaAsync(string nome, int? ignorarCategoriaId = null, CancellationToken cancelamento = default);

    Task AdicionarCategoriaAsync(Categoria categoria, CancellationToken cancelamento = default);

    void RemoverCategoria(Categoria categoria);

    Task<IReadOnlyList<Categoria>> ListarCategoriasAsync(CancellationToken cancelamento = default);

    Task<int> ContarProdutosDaCategoriaAsync(int categoriaId, CancellationToken cancelamento = default);
}

public class RepositorioCatalogo(ContextoFranquias contexto)
    : RepositorioBase<ProdutoServico>(contexto), IRepositorioCatalogo
{
    public async Task<ProdutoServico?> ObterComCategoriaAsync(
        int produtoServicoId,
        CancellationToken cancelamento = default) =>
        await Conjunto
            .Include(produto => produto.Categoria)
            .FirstOrDefaultAsync(produto => produto.ProdutoServicoId == produtoServicoId, cancelamento);

    public async Task<IReadOnlyList<ProdutoServico>> ObterVariosPorIdAsync(
        IEnumerable<int> identificadores,
        CancellationToken cancelamento = default)
    {
        var lista = identificadores.Distinct().ToList();

        return await Conjunto
            .Where(produto => lista.Contains(produto.ProdutoServicoId))
            .ToListAsync(cancelamento);
    }

    public async Task<bool> CodigoJaCadastradoAsync(string codigo, CancellationToken cancelamento = default)
    {
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();

        return await Conjunto
            .AsNoTracking()
            .AnyAsync(produto => produto.Codigo == codigoNormalizado, cancelamento);
    }

    public async Task<(IReadOnlyList<ProdutoServico> Itens, int Total)> BuscarAsync(
        FiltroProdutosServicos filtro,
        CancellationToken cancelamento = default)
    {
        var termo = filtro.Termo?.Trim().ToLowerInvariant();

        var consulta = ConsultaSemRastreamento()
            .Include(produto => produto.Categoria)
            .FiltrarQuando(filtro.CategoriaId.HasValue, produto => produto.CategoriaId == filtro.CategoriaId)
            .FiltrarQuando(filtro.Tipo.HasValue, produto => produto.Tipo == filtro.Tipo!.Value)
            .FiltrarQuando(filtro.Ativo.HasValue, produto => produto.Ativo == filtro.Ativo!.Value)
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(termo),
                produto => produto.Nome.ToLower().Contains(termo!) ||
                           produto.Codigo.ToLower().Contains(termo!) ||
                           produto.Categoria.Nome.ToLower().Contains(termo!))
            .OrdenarPorCampo(filtro.OrdenarPor, filtro.Descendente, nameof(ProdutoServico.Nome));

        return await consulta.ObterPaginaAsync(filtro, cancelamento);
    }

    public async Task<Categoria?> ObterCategoriaAsync(int categoriaId, CancellationToken cancelamento = default) =>
        await Contexto.Categorias.FirstOrDefaultAsync(categoria => categoria.CategoriaId == categoriaId, cancelamento);

    public async Task<bool> CategoriaJaCadastradaAsync(
        string nome,
        int? ignorarCategoriaId = null,
        CancellationToken cancelamento = default)
    {
        var nomeNormalizado = nome.Trim().ToLowerInvariant();

        return await Contexto.Categorias
            .AsNoTracking()
            .AnyAsync(
                categoria => categoria.Nome.ToLower() == nomeNormalizado &&
                             (ignorarCategoriaId == null || categoria.CategoriaId != ignorarCategoriaId),
                cancelamento);
    }

    public async Task AdicionarCategoriaAsync(Categoria categoria, CancellationToken cancelamento = default) =>
        await Contexto.Categorias.AddAsync(categoria, cancelamento);

    public void RemoverCategoria(Categoria categoria) => Contexto.Categorias.Remove(categoria);

    public async Task<IReadOnlyList<Categoria>> ListarCategoriasAsync(CancellationToken cancelamento = default) =>
        await Contexto.Categorias
            .AsNoTracking()
            .Include(categoria => categoria.Produtos)
            .OrderBy(categoria => categoria.Nome)
            .ToListAsync(cancelamento);

    public async Task<int> ContarProdutosDaCategoriaAsync(int categoriaId, CancellationToken cancelamento = default) =>
        await Conjunto
            .AsNoTracking()
            .CountAsync(produto => produto.CategoriaId == categoriaId, cancelamento);
}

public interface IRepositorioFornecedor : IRepositorioBase<Fornecedor>
{
    Task<bool> CnpjJaCadastradoAsync(string cnpj, CancellationToken cancelamento = default);

    Task<Fornecedor?> ObterComProdutosAsync(int fornecedorId, CancellationToken cancelamento = default);

    Task<(IReadOnlyList<Fornecedor> Itens, int Total)> BuscarAsync(FiltroFornecedores filtro, CancellationToken cancelamento = default);

    Task<FornecedorProduto?> ObterVinculoAsync(int fornecedorId, int produtoServicoId, CancellationToken cancelamento = default);

    Task AdicionarVinculoAsync(FornecedorProduto vinculo, CancellationToken cancelamento = default);

    void RemoverVinculo(FornecedorProduto vinculo);
}

public class RepositorioFornecedor(ContextoFranquias contexto)
    : RepositorioBase<Fornecedor>(contexto), IRepositorioFornecedor
{
    public async Task<bool> CnpjJaCadastradoAsync(string cnpj, CancellationToken cancelamento = default)
    {
        var cnpjFormatado = DocumentoValidador.FormatarCnpj(cnpj);

        return await Conjunto
            .AsNoTracking()
            .AnyAsync(fornecedor => fornecedor.Cnpj == cnpjFormatado, cancelamento);
    }

    public async Task<Fornecedor?> ObterComProdutosAsync(int fornecedorId, CancellationToken cancelamento = default) =>
        await Conjunto
            .Include(fornecedor => fornecedor.Produtos)
                .ThenInclude(vinculo => vinculo.ProdutoServico)
            .FirstOrDefaultAsync(fornecedor => fornecedor.FornecedorId == fornecedorId, cancelamento);

    public async Task<(IReadOnlyList<Fornecedor> Itens, int Total)> BuscarAsync(
        FiltroFornecedores filtro,
        CancellationToken cancelamento = default)
    {
        var termo = filtro.Termo?.Trim().ToLowerInvariant();
        var cnpj = string.IsNullOrWhiteSpace(filtro.Cnpj) ? null : DocumentoValidador.ApenasDigitos(filtro.Cnpj);

        var consulta = ConsultaSemRastreamento()
            .Include(fornecedor => fornecedor.Produtos)
                .ThenInclude(vinculo => vinculo.ProdutoServico)
            .FiltrarQuando(filtro.Ativo.HasValue, fornecedor => fornecedor.Ativo == filtro.Ativo!.Value)
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(cnpj),
                fornecedor => fornecedor.Cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Contains(cnpj!))
            .FiltrarQuando(
                filtro.ProdutoServicoId.HasValue,
                fornecedor => fornecedor.Produtos.Any(vinculo => vinculo.ProdutoServicoId == filtro.ProdutoServicoId))
            .FiltrarQuando(
                !string.IsNullOrWhiteSpace(termo),
                fornecedor => fornecedor.NomeFantasia.ToLower().Contains(termo!) ||
                              fornecedor.RazaoSocial.ToLower().Contains(termo!) ||
                              fornecedor.Cnpj.Contains(termo!))
            .OrdenarPorCampo(filtro.OrdenarPor, filtro.Descendente, nameof(Fornecedor.NomeFantasia));

        return await consulta.ObterPaginaAsync(filtro, cancelamento);
    }

    public async Task<FornecedorProduto?> ObterVinculoAsync(
        int fornecedorId,
        int produtoServicoId,
        CancellationToken cancelamento = default) =>
        await Contexto.FornecedoresProdutos
            .FirstOrDefaultAsync(
                vinculo => vinculo.FornecedorId == fornecedorId && vinculo.ProdutoServicoId == produtoServicoId,
                cancelamento);

    public async Task AdicionarVinculoAsync(FornecedorProduto vinculo, CancellationToken cancelamento = default) =>
        await Contexto.FornecedoresProdutos.AddAsync(vinculo, cancelamento);

    public void RemoverVinculo(FornecedorProduto vinculo) => Contexto.FornecedoresProdutos.Remove(vinculo);
}
