using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Services;
using ConectaFranquias.Api.Services.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConectaFranquias.Api.Controllers;

/// <summary>Saldo de estoque por unidade e suas movimentações.</summary>
[Authorize]
[Tags("Estoques")]
public class EstoquesController(IServicoEstoque servico) : ControllerBaseApi
{
    /// <summary>
    /// Consulta saldos por unidade e produto. Use "abaixoDoMinimo=true" para
    /// listar apenas os itens que precisam de reposição.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<EstoqueResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<EstoqueResposta>>> Buscar(
        [FromQuery] FiltroEstoques filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.BuscarAsync(filtro, cancelamento));

    /// <summary>Obtém um registro de estoque pelo identificador.</summary>
    [HttpGet("{estoqueId:int}")]
    [ProducesResponseType(typeof(EstoqueResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<EstoqueResposta>> ObterPorId(int estoqueId, CancellationToken cancelamento) =>
        Ok(await servico.ObterPorIdAsync(estoqueId, cancelamento));

    /// <summary>Abre o controle de estoque de um produto em uma unidade.</summary>
    /// <response code="409">O produto já possui controle de estoque nesta unidade.</response>
    [HttpPost]
    [Authorize(Policy = Politicas.AdministradorOuGestor)]
    [ProducesResponseType(typeof(EstoqueResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EstoqueResposta>> Criar(
        [FromBody] CriarEstoqueRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await servico.CriarAsync(requisicao, cancelamento);
        return CreatedAtAction(nameof(ObterPorId), new { estoqueId = resposta.EstoqueId }, resposta);
    }

    /// <summary>
    /// Registra uma entrada ou saída de estoque. Operações que deixariam o saldo
    /// negativo são recusadas.
    /// </summary>
    /// <response code="400">A movimentação deixaria o saldo negativo.</response>
    [HttpPost("{estoqueId:int}/movimentacoes")]
    [ProducesResponseType(typeof(MovimentacaoEstoqueResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<MovimentacaoEstoqueResposta>> Movimentar(
        int estoqueId,
        [FromBody] MovimentarEstoqueRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.MovimentarAsync(estoqueId, requisicao, cancelamento));

    /// <summary>Histórico de entradas e saídas, com filtros por unidade, produto e tipo.</summary>
    [HttpGet("movimentacoes")]
    [ProducesResponseType(typeof(ResultadoPaginado<MovimentacaoEstoqueResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<MovimentacaoEstoqueResposta>>> BuscarMovimentacoes(
        [FromQuery] FiltroMovimentacoes filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.BuscarMovimentacoesAsync(filtro, cancelamento));
}

/// <summary>Registro e acompanhamento das vendas das unidades.</summary>
[Authorize]
[Tags("Vendas")]
public class VendasController(IServicoVenda servico) : ControllerBaseApi
{
    /// <summary>Consulta vendas por unidade, intervalo de datas e situação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<VendaResumoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<VendaResumoResposta>>> Buscar(
        [FromQuery] FiltroVendas filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.BuscarAsync(filtro, cancelamento));

    /// <summary>Obtém a venda com todos os seus itens.</summary>
    [HttpGet("{vendaId:int}")]
    [ProducesResponseType(typeof(VendaResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<VendaResposta>> ObterPorId(int vendaId, CancellationToken cancelamento) =>
        Ok(await servico.ObterPorIdAsync(vendaId, cancelamento));

    /// <summary>
    /// Registra uma venda. O valor total é calculado a partir dos itens e, quando
    /// confirmada, o estoque é baixado na mesma transação.
    /// </summary>
    /// <response code="400">Unidade inativa, item inexistente ou estoque insuficiente.</response>
    [HttpPost]
    [ProducesResponseType(typeof(VendaResposta), StatusCodes.Status201Created)]
    public async Task<ActionResult<VendaResposta>> Registrar(
        [FromBody] CriarVendaRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await servico.RegistrarAsync(requisicao, cancelamento);
        return CreatedAtAction(nameof(ObterPorId), new { vendaId = resposta.VendaId }, resposta);
    }

    /// <summary>Confirma uma venda pendente e baixa o estoque correspondente.</summary>
    [HttpPatch("{vendaId:int}/confirmar")]
    [ProducesResponseType(typeof(VendaResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<VendaResposta>> Confirmar(int vendaId, CancellationToken cancelamento) =>
        Ok(await servico.ConfirmarAsync(vendaId, cancelamento));

    /// <summary>
    /// Cancela a venda preservando o registro histórico. Vendas já confirmadas
    /// têm o estoque estornado.
    /// </summary>
    [HttpPatch("{vendaId:int}/cancelar")]
    [ProducesResponseType(typeof(VendaResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<VendaResposta>> Cancelar(int vendaId, CancellationToken cancelamento) =>
        Ok(await servico.CancelarAsync(vendaId, cancelamento));
}

/// <summary>Apuração e cobrança de royalties e taxas de franquia.</summary>
[Authorize]
[Tags("Royalties")]
public class RoyaltiesController(IServicoRoyalty servico) : ControllerBaseApi
{
    /// <summary>Consulta valores devidos e pagos por unidade, competência e situação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<RoyaltyResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<RoyaltyResposta>>> Buscar(
        [FromQuery] FiltroRoyalties filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.BuscarAsync(filtro, cancelamento));

    /// <summary>Obtém uma cobrança pelo identificador.</summary>
    [HttpGet("{royaltyId:int}")]
    [ProducesResponseType(typeof(RoyaltyResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoyaltyResposta>> ObterPorId(int royaltyId, CancellationToken cancelamento) =>
        Ok(await servico.ObterPorIdAsync(royaltyId, cancelamento));

    /// <summary>
    /// Apura a cobrança de uma unidade em uma competência, somando o faturamento
    /// confirmado do período e aplicando o percentual configurado.
    /// </summary>
    /// <response code="409">A competência já foi apurada; use "reapurarSeExistir".</response>
    [HttpPost]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(RoyaltyResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoyaltyResposta>> Apurar(
        [FromBody] ApurarRoyaltyRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await servico.ApurarAsync(requisicao, cancelamento);
        return CreatedAtAction(nameof(ObterPorId), new { royaltyId = resposta.RoyaltyId }, resposta);
    }

    /// <summary>Registra o pagamento de uma cobrança.</summary>
    [HttpPut("{royaltyId:int}/pagamento")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(typeof(RoyaltyResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoyaltyResposta>> RegistrarPagamento(
        int royaltyId,
        [FromBody] RegistrarPagamentoRoyaltyRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.RegistrarPagamentoAsync(royaltyId, requisicao, cancelamento));
}

/// <summary>Chamados de suporte abertos pelas unidades para a franqueadora.</summary>
[Authorize]
[Tags("Chamados")]
public class ChamadosController(IServicoChamado servico) : ControllerBaseApi
{
    /// <summary>
    /// Consulta chamados por unidade, situação, prioridade e categoria. Use
    /// "somenteEmAberto=true" para listar apenas os pendentes.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<ChamadoResumoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<ChamadoResumoResposta>>> Buscar(
        [FromQuery] FiltroChamados filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.BuscarAsync(filtro, cancelamento));

    /// <summary>Obtém o chamado com todo o histórico de interações.</summary>
    [HttpGet("{chamadoId:int}")]
    [ProducesResponseType(typeof(ChamadoResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChamadoResposta>> ObterPorId(int chamadoId, CancellationToken cancelamento) =>
        Ok(await servico.ObterPorIdAsync(chamadoId, cancelamento));

    /// <summary>Abre um chamado para a franqueadora.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChamadoResposta), StatusCodes.Status201Created)]
    public async Task<ActionResult<ChamadoResposta>> Abrir(
        [FromBody] AbrirChamadoRequisicao requisicao,
        CancellationToken cancelamento)
    {
        var resposta = await servico.AbrirAsync(requisicao, cancelamento);
        return CreatedAtAction(nameof(ObterPorId), new { chamadoId = resposta.ChamadoSuporteId }, resposta);
    }

    /// <summary>Reclassifica título, descrição, categoria e prioridade de um chamado aberto.</summary>
    [HttpPut("{chamadoId:int}")]
    [ProducesResponseType(typeof(ChamadoResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChamadoResposta>> Atualizar(
        int chamadoId,
        [FromBody] AtualizarChamadoRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.AtualizarAsync(chamadoId, requisicao, cancelamento));

    /// <summary>Registra uma atualização no chamado, opcionalmente avançando o status.</summary>
    [HttpPost("{chamadoId:int}/interacoes")]
    [ProducesResponseType(typeof(ChamadoResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChamadoResposta>> RegistrarInteracao(
        int chamadoId,
        [FromBody] RegistrarInteracaoRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.RegistrarInteracaoAsync(chamadoId, requisicao, cancelamento));

    /// <summary>Encerra o chamado registrando a solução aplicada.</summary>
    [HttpPatch("{chamadoId:int}/encerrar")]
    [ProducesResponseType(typeof(ChamadoResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChamadoResposta>> Encerrar(
        int chamadoId,
        [FromBody] EncerrarChamadoRequisicao requisicao,
        CancellationToken cancelamento) =>
        Ok(await servico.EncerrarAsync(chamadoId, requisicao, cancelamento));
}

/// <summary>Consultas e indicadores gerenciais da rede.</summary>
[Authorize]
[Tags("Relatórios")]
public class RelatoriosController(IServicoRelatorio servico) : ControllerBaseApi
{
    /// <summary>Faturamento por unidade em um período, com ticket médio e royalty estimado.</summary>
    [HttpGet("faturamento")]
    [ProducesResponseType(typeof(IReadOnlyList<FaturamentoUnidadeResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FaturamentoUnidadeResposta>>> ObterFaturamento(
        [FromQuery] FiltroPeriodo filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.ObterFaturamentoAsync(filtro, cancelamento));

    /// <summary>Ranking das unidades por faturamento no período.</summary>
    [HttpGet("ranking-unidades")]
    [ProducesResponseType(typeof(IReadOnlyList<RankingUnidadeResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RankingUnidadeResposta>>> ObterRanking(
        [FromQuery] FiltroPeriodo filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.ObterRankingUnidadesAsync(filtro, cancelamento));

    /// <summary>Produtos e serviços mais vendidos no período.</summary>
    [HttpGet("produtos-mais-vendidos")]
    [ProducesResponseType(typeof(IReadOnlyList<ProdutoMaisVendidoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProdutoMaisVendidoResposta>>> ObterProdutosMaisVendidos(
        [FromQuery] FiltroPeriodo filtro,
        CancellationToken cancelamento,
        [FromQuery] int quantidade = 10) =>
        Ok(await servico.ObterProdutosMaisVendidosAsync(filtro, quantidade, cancelamento));

    /// <summary>Itens com saldo abaixo do estoque mínimo.</summary>
    [HttpGet("estoque-critico")]
    [ProducesResponseType(typeof(IReadOnlyList<EstoqueCriticoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EstoqueCriticoResposta>>> ObterEstoqueCritico(
        [FromQuery] int? unidadeId,
        CancellationToken cancelamento) =>
        Ok(await servico.ObterEstoqueCriticoAsync(unidadeId, cancelamento));

    /// <summary>Total de royalties gerados, pagos e em aberto no período.</summary>
    [HttpGet("royalties")]
    [ProducesResponseType(typeof(ResumoRoyaltiesResposta), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumoRoyaltiesResposta>> ObterResumoRoyalties(
        [FromQuery] FiltroPeriodo filtro,
        CancellationToken cancelamento) =>
        Ok(await servico.ObterResumoRoyaltiesAsync(filtro, cancelamento));

    /// <summary>Quantidade de chamados por situação.</summary>
    [HttpGet("chamados-por-situacao")]
    [ProducesResponseType(typeof(IReadOnlyList<ChamadosPorSituacaoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ChamadosPorSituacaoResposta>>> ObterChamadosPorSituacao(
        [FromQuery] int? unidadeId,
        CancellationToken cancelamento) =>
        Ok(await servico.ObterChamadosPorSituacaoAsync(unidadeId, cancelamento));
}
