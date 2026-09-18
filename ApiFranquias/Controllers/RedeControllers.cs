using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Services;
using ConectaFranquias.Api.Services.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConectaFranquias.Api.Controllers;

/// <summary>Cadastro da rede/franqueadora.</summary>
[Authorize(Policy = Politicas.SomenteAdministrador)]
[Tags("Franqueadoras")]
public class FranqueadorasController(IServicoRede servico) : ControllerBaseApi
{
    /// <summary>Lista as franqueadoras cadastradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FranqueadoraResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FranqueadoraResposta>>> Listar(CancellationToken cancelamento) =>
        Ok(await servico.ListarFranqueadorasAsync(cancelamento));

    /// <summary>Obtém uma franqueadora pelo identificador.</summary>
    [HttpGet("{franqueadoraId:int}")]
    [ProducesResponseType(typeof(FranqueadoraResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<FranqueadoraResposta>> ObterPorId(
        int franqueadoraId,
        CancellationToken cancelamento) =>
        Ok(await servico.ObterFranqueadoraAsync(franqueadoraId, cancelamento));

    /// <summary>Cadastra uma nova franqueadora.</summary>
    /// <response code="409">Já existe uma franqueadora com o CNPJ informado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(FranqueadoraResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FranqueadoraResposta>> Criar(
        [FromBody] CriarFranqueadoraRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await servico.CriarFranqueadoraAsync(requisicao, cancelamento);
        return CreatedAtAction(nameof(ObterPorId), new { franqueadoraId = resposta.FranqueadoraId }, resposta);
    }

    /// <summary>Atualiza os dados da franqueadora. O CNPJ não é editável.</summary>
    [HttpPut("{franqueadoraId:int}")]
    [ProducesResponseType(typeof(FranqueadoraResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<FranqueadoraResposta>> Atualizar(
        int franqueadoraId,
        [FromBody] AtualizarFranqueadoraRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AtualizarFranqueadoraAsync(franqueadoraId, requisicao, cancelamento));
}

/// <summary>Cadastro dos franqueados titulares dos contratos de unidade.</summary>
[Authorize(Policy = Politicas.SomenteAdministrador)]
[Tags("Franqueados")]
public class FranqueadosController(IServicoRede servico) : ControllerBaseApi
{
    /// <summary>Lista franqueados com busca, filtros, ordenação e paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<FranqueadoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<FranqueadoResposta>>> Buscar(
        [FromQuery] FiltroFranqueados filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.BuscarFranqueadosAsync(filtro, cancelamento));

    /// <summary>Obtém um franqueado pelo identificador.</summary>
    [HttpGet("{franqueadoId:int}")]
    [ProducesResponseType(typeof(FranqueadoResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<FranqueadoResposta>> ObterPorId(
        int franqueadoId,
        CancellationToken cancelamento) =>
        Ok(await servico.ObterFranqueadoAsync(franqueadoId, cancelamento));

    /// <summary>Cadastra um novo franqueado.</summary>
    /// <response code="409">Já existe um franqueado com o CPF informado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(FranqueadoResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FranqueadoResposta>> Criar(
        [FromBody] CriarFranqueadoRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await servico.CriarFranqueadoAsync(requisicao, cancelamento);
        return CreatedAtAction(nameof(ObterPorId), new { franqueadoId = resposta.FranqueadoId }, resposta);
    }

    /// <summary>Atualiza os dados do franqueado. O CPF não é editável.</summary>
    [HttpPut("{franqueadoId:int}")]
    [ProducesResponseType(typeof(FranqueadoResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<FranqueadoResposta>> Atualizar(
        int franqueadoId,
        [FromBody] AtualizarFranqueadoRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AtualizarFranqueadoAsync(franqueadoId, requisicao, cancelamento));
}

/// <summary>Cadastro das unidades franqueadas e de seus responsáveis.</summary>
[Authorize]
[Tags("Unidades")]
public class UnidadesController(IServicoRede servico) : ControllerBaseApi
{
    /// <summary>
    /// Lista unidades com busca por nome, cidade, CNPJ ou responsável, além de
    /// filtro por situação, ordenação e paginação.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<UnidadeResumoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<UnidadeResumoResposta>>> Buscar(
        [FromQuery] FiltroUnidades filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.BuscarUnidadesAsync(filtro, cancelamento));

    /// <summary>Obtém uma unidade com endereço, franqueado e responsáveis.</summary>
    [HttpGet("{unidadeId:int}")]
    [ProducesResponseType(typeof(UnidadeResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<UnidadeResposta>> ObterPorId(int unidadeId, CancellationToken cancelamento) =>
        Ok(await servico.ObterUnidadeAsync(unidadeId, cancelamento));

    /// <summary>Cadastra uma nova unidade franqueada.</summary>
    /// <response code="409">Já existe uma unidade com o CNPJ ou o código informado.</response>
    [HttpPost]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(UnidadeResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UnidadeResposta>> Criar(
        [FromBody] CriarUnidadeRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await servico.CriarUnidadeAsync(requisicao, cancelamento);
        return CreatedAtAction(nameof(ObterPorId), new { unidadeId = resposta.UnidadeId }, resposta);
    }

    /// <summary>Atualiza a unidade. Código e CNPJ não são editáveis.</summary>
    [HttpPut("{unidadeId:int}")]
    [Authorize(Policy = Politicas.AdministradorOuGestor)]
    [ProducesResponseType(typeof(UnidadeResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<UnidadeResposta>> Atualizar(
        int unidadeId,
        [FromBody] AtualizarUnidadeRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AtualizarUnidadeAsync(unidadeId, requisicao, cancelamento));

    /// <summary>
    /// Altera a situação contratual da unidade. Uma unidade inativa ou suspensa
    /// deixa de registrar novas vendas.
    /// </summary>
    [HttpPatch("{unidadeId:int}/situacao")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(UnidadeResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<UnidadeResposta>> AlterarSituacao(
        int unidadeId,
        [FromBody] AlterarSituacaoUnidadeRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AlterarSituacaoAsync(unidadeId, requisicao, cancelamento));

    /// <summary>Cadastra um responsável pela operação da unidade.</summary>
    [HttpPost("{unidadeId:int}/responsaveis")]
    [Authorize(Policy = Politicas.AdministradorOuGestor)]
    [ProducesResponseType(typeof(ResponsavelResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResponsavelResposta>> AdicionarResponsavel(
        int unidadeId,
        [FromBody] CriarResponsavelRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AdicionarResponsavelAsync(unidadeId, requisicao, cancelamento));

    /// <summary>Atualiza os dados de um responsável.</summary>
    [HttpPut("{unidadeId:int}/responsaveis/{responsavelId:int}")]
    [Authorize(Policy = Politicas.AdministradorOuGestor)]
    [ProducesResponseType(typeof(ResponsavelResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponsavelResposta>> AtualizarResponsavel(
        int unidadeId,
        int responsavelId,
        [FromBody] AtualizarResponsavelRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AtualizarResponsavelAsync(unidadeId, responsavelId, requisicao, cancelamento));

    /// <summary>Inativa um responsável da unidade.</summary>
    [HttpDelete("{unidadeId:int}/responsaveis/{responsavelId:int}")]
    [Authorize(Policy = Politicas.AdministradorOuGestor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> InativarResponsavel(
        int unidadeId,
        int responsavelId,
        CancellationToken cancelamento)
    {
        await servico.InativarResponsavelAsync(unidadeId, responsavelId, cancelamento);
        return NoContent();
    }
}
