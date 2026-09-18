using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Excecoes;
using ConectaFranquias.Api.Extensoes;
using ConectaFranquias.Api.Mapeamentos;
using ConectaFranquias.Api.Repositories;
using ConectaFranquias.Api.Services.Seguranca;

namespace ConectaFranquias.Api.Services;

public interface IServicoEstoque
{
    Task<ResultadoPaginado<EstoqueResposta>> BuscarAsync(FiltroEstoques filtro, CancellationToken cancelamento = default);

    Task<EstoqueResposta> ObterPorIdAsync(int estoqueId, CancellationToken cancelamento = default);

    Task<EstoqueResposta> CriarAsync(CriarEstoqueRequisicao requisicao, CancellationToken cancelamento = default);

    Task<MovimentacaoEstoqueResposta> MovimentarAsync(int estoqueId, MovimentarEstoqueRequisicao requisicao, CancellationToken cancelamento = default);

    Task<ResultadoPaginado<MovimentacaoEstoqueResposta>> BuscarMovimentacoesAsync(FiltroMovimentacoes filtro, CancellationToken cancelamento = default);
}

public class ServicoEstoque(
    IRepositorioEstoque repositorioEstoque,
    IRepositorioUnidade repositorioUnidade,
    IRepositorioCatalogo repositorioCatalogo,
    IUnidadeDeTrabalho unidadeDeTrabalho,
    IContextoUsuario contextoUsuario) : IServicoEstoque
{
    public async Task<ResultadoPaginado<EstoqueResposta>> BuscarAsync(
        FiltroEstoques filtro,
        CancellationToken cancelamento = default)
    {
        filtro.UnidadeId = contextoUsuario.ResolverUnidadeDaConsulta(filtro.UnidadeId);

        var pagina = await repositorioEstoque.BuscarAsync(filtro, cancelamento);
        return pagina.ParaResultadoPaginado(filtro, item => item.ParaResposta());
    }

    public async Task<EstoqueResposta> ObterPorIdAsync(int estoqueId, CancellationToken cancelamento = default)
    {
        var estoque = await repositorioEstoque.ObterComRelacionamentosAsync(estoqueId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Registro de estoque", estoqueId);

        contextoUsuario.GarantirAcessoAUnidade(estoque.UnidadeId);

        return estoque.ParaResposta();
    }

    public async Task<EstoqueResposta> CriarAsync(
        CriarEstoqueRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        contextoUsuario.GarantirAcessoAUnidade(requisicao.UnidadeId);

        if (!await repositorioUnidade.ExisteAsync(requisicao.UnidadeId, cancelamento))
        {
            throw new ExcecaoNaoEncontrado("Unidade", requisicao.UnidadeId);
        }

        var produto = await repositorioCatalogo.ObterPorIdAsync(requisicao.ProdutoServicoId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Produto/serviço", requisicao.ProdutoServicoId);

        if (!produto.ControlaEstoque)
        {
            throw new ExcecaoNegocio($"'{produto.Nome}' é um serviço e não possui controle de estoque.");
        }

        var jaExiste = await repositorioEstoque.ObterPorUnidadeEProdutoAsync(
            requisicao.UnidadeId,
            requisicao.ProdutoServicoId,
            cancelamento);

        if (jaExiste is not null)
        {
            throw new ExcecaoConflito(
                $"O item '{produto.Nome}' já possui controle de estoque nesta unidade. " +
                "Use a movimentação de entrada para repor o saldo.");
        }

        var estoque = new Estoque(requisicao.UnidadeId, requisicao.ProdutoServicoId, requisicao.QuantidadeMinima);

        await repositorioEstoque.AdicionarAsync(estoque, cancelamento);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return await ObterPorIdAsync(estoque.EstoqueId, cancelamento);
    }

    public async Task<MovimentacaoEstoqueResposta> MovimentarAsync(
        int estoqueId,
        MovimentarEstoqueRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        var estoque = await repositorioEstoque.ObterComRelacionamentosAsync(estoqueId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Registro de estoque", estoqueId);

        contextoUsuario.GarantirAcessoAUnidade(estoque.UnidadeId);

        // A própria entidade recusa a operação se o saldo ficaria negativo.
        var movimentacao = estoque.Movimentar(
            requisicao.Tipo,
            requisicao.Quantidade,
            requisicao.Motivo.Trim(),
            contextoUsuario.UsuarioId);

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

        return movimentacao.ParaResposta();
    }

    public async Task<ResultadoPaginado<MovimentacaoEstoqueResposta>> BuscarMovimentacoesAsync(
        FiltroMovimentacoes filtro,
        CancellationToken cancelamento = default)
    {
        filtro.UnidadeId = contextoUsuario.ResolverUnidadeDaConsulta(filtro.UnidadeId);

        var pagina = await repositorioEstoque.BuscarMovimentacoesAsync(filtro, cancelamento);
        return pagina.ParaResultadoPaginado(filtro, item => item.ParaResposta());
    }
}
