using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Excecoes;
using ConectaFranquias.Api.Extensoes;
using ConectaFranquias.Api.Mapeamentos;
using ConectaFranquias.Api.Repositories;
using ConectaFranquias.Api.Services.Seguranca;

namespace ConectaFranquias.Api.Services;

public interface IServicoVenda
{
    Task<ResultadoPaginado<VendaResumoResposta>> BuscarAsync(FiltroVendas filtro, CancellationToken cancelamento = default);

    Task<VendaResposta> ObterPorIdAsync(int vendaId, CancellationToken cancelamento = default);

    Task<VendaResposta> RegistrarAsync(CriarVendaRequisicao requisicao, CancellationToken cancelamento = default);

    Task<VendaResposta> ConfirmarAsync(int vendaId, CancellationToken cancelamento = default);

    Task<VendaResposta> CancelarAsync(int vendaId, CancellationToken cancelamento = default);
}

public class ServicoVenda(
    IRepositorioVenda repositorioVenda,
    IRepositorioUnidade repositorioUnidade,
    IRepositorioCatalogo repositorioCatalogo,
    IRepositorioEstoque repositorioEstoque,
    IUnidadeDeTrabalho unidadeDeTrabalho,
    IContextoUsuario contextoUsuario) : IServicoVenda
{
    public async Task<ResultadoPaginado<VendaResumoResposta>> BuscarAsync(
        FiltroVendas filtro,
        CancellationToken cancelamento = default)
    {
        filtro.UnidadeId = contextoUsuario.ResolverUnidadeDaConsulta(filtro.UnidadeId);

        if (filtro.DataInicio.HasValue && filtro.DataFim.HasValue && filtro.DataInicio > filtro.DataFim)
        {
            throw new ExcecaoNegocio("A data de início não pode ser posterior à data de fim.");
        }

        var pagina = await repositorioVenda.BuscarAsync(filtro, cancelamento);
        return pagina.ParaResultadoPaginado(filtro, item => item.ParaResumo());
    }

    public async Task<VendaResposta> ObterPorIdAsync(int vendaId, CancellationToken cancelamento = default)
    {
        var venda = await repositorioVenda.ObterComItensAsync(vendaId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Venda", vendaId);

        contextoUsuario.GarantirAcessoAUnidade(venda.UnidadeId);

        return venda.ParaResposta();
    }

    public async Task<VendaResposta> RegistrarAsync(
        CriarVendaRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        contextoUsuario.GarantirAcessoAUnidade(requisicao.UnidadeId);

        var unidade = await repositorioUnidade.ObterPorIdAsync(requisicao.UnidadeId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Unidade", requisicao.UnidadeId);

        if (!unidade.PodeRegistrarVendas)
        {
            throw new ExcecaoNegocio(
                $"A unidade '{unidade.NomeFantasia}' está com situação '{unidade.Situacao}' e não pode registrar vendas.");
        }

        var produtos = await CarregarProdutosValidadosAsync(requisicao.Itens, cancelamento);

        var vendaId = await unidadeDeTrabalho.ExecutarEmTransacaoAsync(async () =>
        {
            var numeroVenda = await repositorioVenda.GerarProximoNumeroAsync(requisicao.UnidadeId, cancelamento);

            var venda = new Venda(
                requisicao.UnidadeId,
                contextoUsuario.UsuarioId,
                numeroVenda,
                requisicao.FormaPagamento,
                DateTime.UtcNow);

            foreach (var item in requisicao.Itens)
            {
                var produto = produtos[item.ProdutoServicoId];
                venda.AdicionarItem(produto.ProdutoServicoId, item.Quantidade, produto.PrecoBase);
            }

            await repositorioVenda.AdicionarAsync(venda, cancelamento);

            if (requisicao.ConfirmarImediatamente)
            {
                venda.Confirmar();
            }

            // Grava antes de mexer no estoque para que a venda já tenha
            // identificador e as movimentações possam apontar para ela.
            await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);

            if (requisicao.ConfirmarImediatamente)
            {
                await BaixarEstoqueAsync(venda, produtos, cancelamento);
                await unidadeDeTrabalho.SalvarAlteracoesAsync(cancelamento);
            }

            return venda.VendaId;
        }, cancelamento);

        return await ObterPorIdAsync(vendaId, cancelamento);
    }

    public async Task<VendaResposta> ConfirmarAsync(int vendaId, CancellationToken cancelamento = default)
    {
        var venda = await repositorioVenda.ObterComItensAsync(vendaId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Venda", vendaId);

        contextoUsuario.GarantirAcessoAUnidade(venda.UnidadeId);

        if (!venda.Unidade.PodeRegistrarVendas)
        {
            throw new ExcecaoNegocio(
                $"A unidade '{venda.Unidade.NomeFantasia}' está com situação '{venda.Unidade.Situacao}' e não pode confirmar vendas.");
        }

        var produtos = venda.Itens.ToDictionary(item => item.ProdutoServicoId, item => item.ProdutoServico);

        await unidadeDeTrabalho.ExecutarEmTransacaoAsync(async () =>
        {
            venda.Confirmar();
            await BaixarEstoqueAsync(venda, produtos, cancelamento);
            return venda.VendaId;
        }, cancelamento);

        return await ObterPorIdAsync(vendaId, cancelamento);
    }

    public async Task<VendaResposta> CancelarAsync(int vendaId, CancellationToken cancelamento = default)
    {
        var venda = await repositorioVenda.ObterComItensAsync(vendaId, cancelamento)
            ?? throw new ExcecaoNaoEncontrado("Venda", vendaId);

        contextoUsuario.GarantirAcessoAUnidade(venda.UnidadeId);

        // Só devolve ao estoque o que de fato saiu na confirmação.
        var precisaEstornar = venda.Situacao == SituacaoVenda.Confirmada;

        await unidadeDeTrabalho.ExecutarEmTransacaoAsync(async () =>
        {
            venda.Cancelar();

            if (precisaEstornar)
            {
                await EstornarEstoqueAsync(venda, cancelamento);
            }

            return venda.VendaId;
        }, cancelamento);

        return await ObterPorIdAsync(vendaId, cancelamento);
    }

    private async Task<Dictionary<int, ProdutoServico>> CarregarProdutosValidadosAsync(
        IReadOnlyCollection<ItemVendaRequisicao> itens,
        CancellationToken cancelamento)
    {
        var identificadores = itens.Select(item => item.ProdutoServicoId).Distinct().ToList();
        var produtos = await repositorioCatalogo.ObterVariosPorIdAsync(identificadores, cancelamento);
        var mapa = produtos.ToDictionary(produto => produto.ProdutoServicoId);

        var inexistentes = identificadores.Where(id => !mapa.ContainsKey(id)).ToList();

        if (inexistentes.Count > 0)
        {
            throw new ExcecaoNaoEncontrado(
                $"Item(ns) do catálogo não encontrado(s): {string.Join(", ", inexistentes)}.");
        }

        var inativos = mapa.Values.Where(produto => !produto.Ativo).Select(produto => produto.Nome).ToList();

        if (inativos.Count > 0)
        {
            throw new ExcecaoNegocio(
                $"Item(ns) inativo(s) no catálogo não podem ser vendidos: {string.Join(", ", inativos)}.");
        }

        return mapa;
    }

    private async Task BaixarEstoqueAsync(
        Venda venda,
        IReadOnlyDictionary<int, ProdutoServico> produtos,
        CancellationToken cancelamento)
    {
        var itensComEstoque = venda.Itens
            .Where(item => produtos[item.ProdutoServicoId].ControlaEstoque)
            .ToList();

        if (itensComEstoque.Count == 0)
        {
            return;
        }

        var estoques = await repositorioEstoque.ObterParaMovimentacaoAsync(
            venda.UnidadeId,
            itensComEstoque.Select(item => item.ProdutoServicoId),
            cancelamento);

        var mapaEstoques = estoques.ToDictionary(estoque => estoque.ProdutoServicoId);

        var semControle = itensComEstoque
            .Where(item => !mapaEstoques.ContainsKey(item.ProdutoServicoId))
            .Select(item => produtos[item.ProdutoServicoId].Nome)
            .ToList();

        if (semControle.Count > 0)
        {
            throw new ExcecaoNegocio(
                $"Os itens a seguir não possuem controle de estoque nesta unidade: {string.Join(", ", semControle)}. " +
                "Cadastre o estoque antes de vender.");
        }

        foreach (var item in itensComEstoque)
        {
            mapaEstoques[item.ProdutoServicoId].Movimentar(
                TipoMovimentacaoEstoque.Saida,
                item.Quantidade,
                $"Baixa automática da venda {venda.NumeroVenda}",
                contextoUsuario.UsuarioId,
                venda.VendaId);
        }
    }

    private async Task EstornarEstoqueAsync(Venda venda, CancellationToken cancelamento)
    {
        var itensComEstoque = venda.Itens
            .Where(item => item.ProdutoServico.ControlaEstoque)
            .ToList();

        if (itensComEstoque.Count == 0)
        {
            return;
        }

        var estoques = await repositorioEstoque.ObterParaMovimentacaoAsync(
            venda.UnidadeId,
            itensComEstoque.Select(item => item.ProdutoServicoId),
            cancelamento);

        var mapaEstoques = estoques.ToDictionary(estoque => estoque.ProdutoServicoId);

        foreach (var item in itensComEstoque)
        {
            // Se o controle de estoque foi removido depois da venda, não há o que estornar.
            if (mapaEstoques.TryGetValue(item.ProdutoServicoId, out var estoque))
            {
                estoque.Movimentar(
                    TipoMovimentacaoEstoque.Entrada,
                    item.Quantidade,
                    $"Estorno do cancelamento da venda {venda.NumeroVenda}",
                    contextoUsuario.UsuarioId,
                    venda.VendaId);
            }
        }
    }
}
