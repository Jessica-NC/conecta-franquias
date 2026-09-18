using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Services;
using ConectaFranquias.Api.Services.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConectaFranquias.Api.Controllers;

/// <summary>Catálogo de produtos e serviços padronizados pela rede.</summary>
[Authorize]
[Tags("Produtos e serviços")]
public class ProdutosController(IServicoCatalogo servico) : ControllerBaseApi
{
    /// <summary>Consulta o catálogo por nome, categoria, tipo e situação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<ProdutoServicoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<ProdutoServicoResposta>>> Buscar(
        [FromQuery] FiltroProdutosServicos filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.BuscarProdutosAsync(filtro, cancelamento));

    /// <summary>Obtém um item do catálogo pelo identificador.</summary>
    [HttpGet("{produtoServicoId:int}")]
    [ProducesResponseType(typeof(ProdutoServicoResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProdutoServicoResposta>> ObterPorId(
        int produtoServicoId,
        CancellationToken cancelamento) =>
        Ok(await servico.ObterProdutoAsync(produtoServicoId, cancelamento));

    /// <summary>Inclui um item no catálogo da rede.</summary>
    /// <response code="409">Já existe um item com o código informado.</response>
    [HttpPost]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(ProdutoServicoResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProdutoServicoResposta>> Criar(
        [FromBody] CriarProdutoServicoRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await servico.CriarProdutoAsync(requisicao, cancelamento);
        return CreatedAtAction(nameof(ObterPorId), new { produtoServicoId = resposta.ProdutoServicoId }, resposta);
    }

    /// <summary>Atualiza um item do catálogo. O código não é editável.</summary>
    [HttpPut("{produtoServicoId:int}")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(ProdutoServicoResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProdutoServicoResposta>> Atualizar(
        int produtoServicoId,
        [FromBody] AtualizarProdutoServicoRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AtualizarProdutoAsync(produtoServicoId, requisicao, cancelamento));

    /// <summary>Inativa o item, preservando o histórico de vendas e estoque.</summary>
    [HttpDelete("{produtoServicoId:int}")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Inativar(int produtoServicoId, CancellationToken cancelamento)
    {
        await servico.InativarProdutoAsync(produtoServicoId, cancelamento);
        return NoContent();
    }
}

/// <summary>Categorias que classificam o catálogo da rede.</summary>
[Authorize]
[Tags("Categorias")]
public class CategoriasController(IServicoCatalogo servico) : ControllerBaseApi
{
    /// <summary>Lista as categorias cadastradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoriaResposta>>> Listar(CancellationToken cancelamento) =>
        Ok(await servico.ListarCategoriasAsync(cancelamento));

    /// <summary>Cadastra uma nova categoria.</summary>
    /// <response code="409">Já existe uma categoria com o nome informado.</response>
    [HttpPost]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(CategoriaResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaResposta>> Criar(
        [FromBody] CategoriaRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await servico.CriarCategoriaAsync(requisicao, cancelamento);
        return CreatedAtAction(nameof(Listar), null, resposta);
    }

    /// <summary>Atualiza uma categoria existente.</summary>
    [HttpPut("{categoriaId:int}")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(CategoriaResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoriaResposta>> Atualizar(
        int categoriaId,
        [FromBody] CategoriaRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AtualizarCategoriaAsync(categoriaId, requisicao, cancelamento));

    /// <summary>Remove a categoria. Só é permitido quando não há itens vinculados.</summary>
    [HttpDelete("{categoriaId:int}")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remover(int categoriaId, CancellationToken cancelamento)
    {
        await servico.RemoverCategoriaAsync(categoriaId, cancelamento);
        return NoContent();
    }
}

/// <summary>Fornecedores homologados e seus vínculos com o catálogo.</summary>
[Authorize]
[Tags("Fornecedores")]
public class FornecedoresController(IServicoFornecedor servico) : ControllerBaseApi
{
    /// <summary>Consulta fornecedores por nome, CNPJ, produto atendido e situação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<FornecedorResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<FornecedorResposta>>> Buscar(
        [FromQuery] FiltroFornecedores filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.BuscarAsync(filtro, cancelamento));

    /// <summary>Obtém um fornecedor com os itens que ele atende.</summary>
    [HttpGet("{fornecedorId:int}")]
    [ProducesResponseType(typeof(FornecedorResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<FornecedorResposta>> ObterPorId(
        int fornecedorId,
        CancellationToken cancelamento) =>
        Ok(await servico.ObterPorIdAsync(fornecedorId, cancelamento));

    /// <summary>Cadastra um novo fornecedor.</summary>
    /// <response code="409">Já existe um fornecedor com o CNPJ informado.</response>
    [HttpPost]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(FornecedorResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FornecedorResposta>> Criar(
        [FromBody] CriarFornecedorRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await servico.CriarAsync(requisicao, cancelamento);
        return CreatedAtAction(nameof(ObterPorId), new { fornecedorId = resposta.FornecedorId }, resposta);
    }

    /// <summary>Atualiza os dados do fornecedor. O CNPJ não é editável.</summary>
    [HttpPut("{fornecedorId:int}")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(FornecedorResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<FornecedorResposta>> Atualizar(
        int fornecedorId,
        [FromBody] AtualizarFornecedorRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AtualizarAsync(fornecedorId, requisicao, cancelamento));

    /// <summary>Inativa o fornecedor, preservando o histórico de vínculos.</summary>
    [HttpDelete("{fornecedorId:int}")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Inativar(int fornecedorId, CancellationToken cancelamento)
    {
        await servico.InativarAsync(fornecedorId, cancelamento);
        return NoContent();
    }

    /// <summary>Vincula um produto ao fornecedor com o preço de custo negociado.</summary>
    [HttpPost("{fornecedorId:int}/produtos")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> VincularProduto(
        int fornecedorId,
        [FromBody] VincularProdutoRequisicao requisicao,
        CancellationToken cancelamento)
    {
        await servico.VincularProdutoAsync(fornecedorId, requisicao, cancelamento);
        return NoContent();
    }

    /// <summary>Desfaz o vínculo entre o fornecedor e um item do catálogo.</summary>
    [HttpDelete("{fornecedorId:int}/produtos/{produtoServicoId:int}")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DesvincularProduto(
        int fornecedorId,
        int produtoServicoId,
        CancellationToken cancelamento)
    {
        await servico.DesvincularProdutoAsync(fornecedorId, produtoServicoId, cancelamento);
        return NoContent();
    }
}
