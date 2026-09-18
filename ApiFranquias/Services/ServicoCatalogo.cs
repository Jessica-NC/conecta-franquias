using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Excecoes;
using ConectaFranquias.Api.Extensoes;
using ConectaFranquias.Api.Mapeamentos;
using ConectaFranquias.Api.Repositories;

namespace ConectaFranquias.Api.Services;

public interface IServicoCatalogo
{
    Task<ResultadoPaginado<ProdutoServicoResposta>> BuscarProdutosAsync(FiltroProdutosServicos filtro, CancellationToken cancelamento = default);

    Task<ProdutoServicoResposta> ObterProdutoAsync(int produtoServicoId, CancellationToken cancelamento = default);

    Task<ProdutoServicoResposta> CriarProdutoAsync(CriarProdutoServicoRequisicao requisicao, CancellationToken cancelamento = default);

    Task<ProdutoServicoResposta> AtualizarProdutoAsync(int produtoServicoId, AtualizarProdutoServicoRequisicao requisicao, CancellationToken cancelamento = default);

    Task InativarProdutoAsync(int produtoServicoId, CancellationToken cancelamento = default);

    Task<IReadOnlyList<CategoriaResposta>> ListarCategoriasAsync(CancellationToken cancelamento = default);

    Task<CategoriaResposta> CriarCategoriaAsync(CategoriaRequisicao requisicao, CancellationToken cancelamento = default);

    Task<CategoriaResposta> AtualizarCategoriaAsync(int categoriaId, CategoriaRequisicao requisicao, CancellationToken cancelamento = default);

    Task RemoverCategoriaAsync(int categoriaId, CancellationToken cancelamento = default);
}

public class ServicoCatalogo(IRepositorioCatalogo repositorio, IUnidadeDeTrabalho unidadeDeTrabalho) : IServicoCatalogo
{
    public async Task<ResultadoPaginado<ProdutoServicoResposta>> BuscarProdutosAsync(
        FiltroProdutosServicos filtro,
        CancellationToken cancelamento = default)
    {
        var pagina = await repositorio.BuscarAsync(filtro, cancelamento);
        return pagina.ParaResultadoPaginado(filtro, item => item.ParaResposta());
    }

    public async Task<ProdutoServicoResposta> ObterProdutoAsync(
        int produtoServicoId,
        CancellationToken cancelamento = default)
    {
        var produto = await repositorio.ObterComCategoriaAsync(produtoServicoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Produto/serviço", produtoServicoId);

        return produto.ParaResposta();
    }

    public async Task<ProdutoServicoResposta> CriarProdutoAsync(
        CriarProdutoServicoRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        if (await repositorio.CodigoJaCadastradoAsync(requisicao.Codigo, cancelamento))
        {
            throw new ExcecaoConflito($"Já existe um item no catálogo com o código '{requisicao.Codigo}'.");
        }

        var categoria = await repositorio.ObterCategoriaAsync(requisicao.CategoriaId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Categoria", requisicao.CategoriaId);

        var produto = new ProdutoServico(
            categoria.CategoriaId,
            requisicao.Codigo,
            requisicao.Nome.Trim(),
            requisicao.Descricao.Trim(),
            requisicao.Tipo,
            requisicao.PrecoBase);

        await repositorio.AdicionarAsync(produto, cancelamento);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterProdutoAsync(produto.ProdutoServicoId, cancelamento);
    }

    public async Task<ProdutoServicoResposta> AtualizarProdutoAsync(
        int produtoServicoId,
        AtualizarProdutoServicoRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var produto = await repositorio.ObterPorIdAsync(produtoServicoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Produto/serviço", produtoServicoId);

        var categoria = await repositorio.ObterCategoriaAsync(requisicao.CategoriaId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Categoria", requisicao.CategoriaId);

        produto.AtualizarDados(
            categoria.CategoriaId,
            requisicao.Nome.Trim(),
            requisicao.Descricao.Trim(),
            requisicao.Tipo,
            requisicao.PrecoBase);

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterProdutoAsync(produtoServicoId, cancelamento);
    }

    public async Task InativarProdutoAsync(int produtoServicoId, CancellationToken cancelamento = default)
    {
        var produto = await repositorio.ObterPorIdAsync(produtoServicoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Produto/serviço", produtoServicoId);

        if (!produto.Ativo)
        {
            throw new ExcecaoNegocio("Este item já está inativo.");
        }

        // O item nunca é apagado: vendas e estoques passados continuam íntegros.
        produto.Inativar();
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);
    }

    public async Task<IReadOnlyList<CategoriaResposta>> ListarCategoriasAsync(CancellationToken cancelamento = default)
    {
        var categorias = await repositorio.ListarCategoriasAsync(cancelamento);
        return categorias.Select(item => item.ParaResposta(item.Produtos.Count)).ToList();
    }

    public async Task<CategoriaResposta> CriarCategoriaAsync(
        CategoriaRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        if (await repositorio.CategoriaJaCadastradaAsync(requisicao.Nome, cancelamento: cancelamento))
        {
            throw new ExcecaoConflito($"Já existe uma categoria chamada '{requisicao.Nome}'.");
        }

        var categoria = new Categoria(requisicao.Nome.Trim(), requisicao.Descricao.Trim());

        await repositorio.AdicionarCategoriaAsync(categoria, cancelamento);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return categoria.ParaResposta(0);
    }

    public async Task<CategoriaResposta> AtualizarCategoriaAsync(
        int categoriaId,
        CategoriaRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var categoria = await repositorio.ObterCategoriaAsync(categoriaId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Categoria", categoriaId);

        if (await repositorio.CategoriaJaCadastradaAsync(requisicao.Nome, categoriaId, cancelamento))
        {
            throw new ExcecaoConflito($"Já existe outra categoria chamada '{requisicao.Nome}'.");
        }

        categoria.AtualizarDados(requisicao.Nome.Trim(), requisicao.Descricao.Trim());
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        var totalProdutos = await repositorio.ContarProdutosDaCategoriaAsync(categoriaId, cancelamento);
        return categoria.ParaResposta(totalProdutos);
    }

    public async Task RemoverCategoriaAsync(int categoriaId, CancellationToken cancelamento = default)
    {
        var categoria = await repositorio.ObterCategoriaAsync(categoriaId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Categoria", categoriaId);

        var totalProdutos = await repositorio.ContarProdutosDaCategoriaAsync(categoriaId, cancelamento);

        if (totalProdutos > 0)
        {
            throw new ExcecaoNegocio(
                $"A categoria possui {totalProdutos} item(ns) vinculado(s) e não pode ser removida.");
        }

        repositorio.RemoverCategoria(categoria);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);
    }
}

public interface IServicoFornecedor
{
    Task<ResultadoPaginado<FornecedorResposta>> BuscarAsync(FiltroFornecedores filtro, CancellationToken cancelamento = default);

    Task<FornecedorResposta> ObterPorIdAsync(int fornecedorId, CancellationToken cancelamento = default);

    Task<FornecedorResposta> CriarAsync(CriarFornecedorRequisicao requisicao, CancellationToken cancelamento = default);

    Task<FornecedorResposta> AtualizarAsync(int fornecedorId, AtualizarFornecedorRequisicao requisicao, CancellationToken cancelamento = default);

    Task InativarAsync(int fornecedorId, CancellationToken cancelamento = default);

    Task VincularProdutoAsync(int fornecedorId, VincularProdutoRequisicao requisicao, CancellationToken cancelamento = default);

    Task DesvincularProdutoAsync(int fornecedorId, int produtoServicoId, CancellationToken cancelamento = default);
}

public class ServicoFornecedor(
    IRepositorioFornecedor repositorioFornecedor,
    IRepositorioCatalogo repositorioCatalogo,
    IUnidadeDeTrabalho unidadeDeTrabalho) : IServicoFornecedor
{
    public async Task<ResultadoPaginado<FornecedorResposta>> BuscarAsync(
        FiltroFornecedores filtro,
        CancellationToken cancelamento = default)
    {
        var pagina = await repositorioFornecedor.BuscarAsync(filtro, cancelamento);
        return pagina.ParaResultadoPaginado(filtro, item => item.ParaResposta());
    }

    public async Task<FornecedorResposta> ObterPorIdAsync(int fornecedorId, CancellationToken cancelamento = default)
    {
        var fornecedor = await repositorioFornecedor.ObterComProdutosAsync(fornecedorId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Fornecedor", fornecedorId);

        return fornecedor.ParaResposta();
    }

    public async Task<FornecedorResposta> CriarAsync(
        CriarFornecedorRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        if (await repositorioFornecedor.CnpjJaCadastradoAsync(requisicao.Cnpj, cancelamento))
        {
            throw new ExcecaoConflito($"Já existe um fornecedor cadastrado com o CNPJ '{requisicao.Cnpj}'.");
        }

        var fornecedor = new Fornecedor(
            requisicao.RazaoSocial.Trim(),
            requisicao.NomeFantasia.Trim(),
            Validacoes.DocumentoValidador.FormatarCnpj(requisicao.Cnpj),
            requisicao.Email,
            requisicao.Telefone.Trim(),
            requisicao.Cidade.Trim(),
            requisicao.Estado.Trim());

        await repositorioFornecedor.AdicionarAsync(fornecedor, cancelamento);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return fornecedor.ParaResposta();
    }

    public async Task<FornecedorResposta> AtualizarAsync(
        int fornecedorId,
        AtualizarFornecedorRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var fornecedor = await repositorioFornecedor.ObterPorIdAsync(fornecedorId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Fornecedor", fornecedorId);

        fornecedor.AtualizarDados(
            requisicao.RazaoSocial.Trim(),
            requisicao.NomeFantasia.Trim(),
            requisicao.Email,
            requisicao.Telefone.Trim(),
            requisicao.Cidade.Trim(),
            requisicao.Estado.Trim());

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterPorIdAsync(fornecedorId, cancelamento);
    }

    public async Task InativarAsync(int fornecedorId, CancellationToken cancelamento = default)
    {
        var fornecedor = await repositorioFornecedor.ObterPorIdAsync(fornecedorId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Fornecedor", fornecedorId);

        if (!fornecedor.Ativo)
        {
            throw new ExcecaoNegocio("Este fornecedor já está inativo.");
        }

        fornecedor.Inativar();
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);
    }

    public async Task VincularProdutoAsync(
        int fornecedorId,
        VincularProdutoRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var fornecedor = await repositorioFornecedor.ObterPorIdAsync(fornecedorId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Fornecedor", fornecedorId);

        if (!fornecedor.Ativo)
        {
            throw new ExcecaoNegocio("Não é possível vincular itens a um fornecedor inativo.");
        }

        var produto = await repositorioCatalogo.ObterPorIdAsync(requisicao.ProdutoServicoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Produto/serviço", requisicao.ProdutoServicoId);

        // Serviços prestados pela própria unidade não são fornecidos por terceiros.
        if (produto.Tipo == TipoProdutoServico.Servico)
        {
            throw new ExcecaoNegocio("Somente produtos podem ser vinculados a fornecedores.");
        }

        var vinculo = await repositorioFornecedor.ObterVinculoAsync(
            fornecedorId,
            requisicao.ProdutoServicoId,
            cancelamento);

        if (vinculo is not null)
        {
            // Revincular o mesmo item apenas renegocia o preço de custo.
            vinculo.AtualizarPrecoCusto(requisicao.PrecoCusto);
        }
        else
        {
            await repositorioFornecedor.AdicionarVinculoAsync(
                new FornecedorProduto(fornecedorId, requisicao.ProdutoServicoId, requisicao.PrecoCusto),
                cancelamento);
        }

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);
    }

    public async Task DesvincularProdutoAsync(
        int fornecedorId,
        int produtoServicoId,
        CancellationToken cancelamento = default)
    {
        var vinculo = await repositorioFornecedor.ObterVinculoAsync(fornecedorId, produtoServicoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado(
                $"O produto {produtoServicoId} não está vinculado ao fornecedor {fornecedorId}.");

        repositorioFornecedor.RemoverVinculo(vinculo);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);
    }
}
