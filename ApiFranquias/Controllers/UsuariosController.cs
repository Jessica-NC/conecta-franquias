using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Services;
using ConectaFranquias.Api.Services.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConectaFranquias.Api.Controllers;

/// <summary>
/// Base dos controllers da API: convenção de rota e tipos de erro documentados
/// no Swagger.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(RespostaErro), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(RespostaErro), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
public abstract class ControllerBaseApi : ControllerBase;

/// <summary>Login na API.</summary>
[Route("api/auth")]
[Tags("Autenticação")]
public class AutenticacaoController(IServicoUsuario servico) : ControllerBaseApi
{
    /// <summary>Autentica o usuário e devolve o token de acesso.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResposta>> Login(
        [FromBody] LoginRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AutenticarAsync(requisicao, cancelamento));
}

/// <summary>Cadastro e manutenção dos usuários do sistema.</summary>
[Authorize(Policy = Politicas.AdministradorOuGestor)]
[Tags("Usuários")]
public class UsuariosController(IServicoUsuario servico) : ControllerBaseApi
{
    /// <summary>Lista usuários com busca, filtros, ordenação e paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<UsuarioResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<UsuarioResposta>>> Buscar(
        [FromQuery] FiltroUsuarios filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.BuscarAsync(filtro, cancelamento));

    /// <summary>Obtém um usuário pelo identificador.</summary>
    [HttpGet("{usuarioId:int}")]
    [ProducesResponseType(typeof(UsuarioResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioResposta>> ObterPorId(int usuarioId, CancellationToken cancelamento) =>
        Ok(await servico.ObterPorIdAsync(usuarioId, cancelamento));

    /// <summary>Cadastra um novo usuário.</summary>
    /// <response code="409">Já existe um usuário com o e-mail informado.</response>
    [HttpPost]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(UsuarioResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResposta>> Criar(
        [FromBody] CriarUsuarioRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await servico.CriarAsync(requisicao, cancelamento);
        return CreatedAtAction(nameof(ObterPorId), new { usuarioId = resposta.UsuarioId }, resposta);
    }

    /// <summary>Atualiza os dados cadastrais de um usuário.</summary>
    [HttpPut("{usuarioId:int}")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(UsuarioResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioResposta>> Atualizar(
        int usuarioId,
        [FromBody] AtualizarUsuarioRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AtualizarAsync(usuarioId, requisicao, cancelamento));

    /// <summary>Inativa o usuário, preservando o vínculo com vendas e chamados.</summary>
    [HttpDelete("{usuarioId:int}")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Inativar(int usuarioId, CancellationToken cancelamento)
    {
        await servico.InativarAsync(usuarioId, cancelamento);
        return NoContent();
    }

    /// <summary>Reativa um usuário previamente inativado.</summary>
    [HttpPatch("{usuarioId:int}/reativar")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reativar(int usuarioId, CancellationToken cancelamento)
    {
        await servico.ReativarAsync(usuarioId, cancelamento);
        return NoContent();
    }
}
