using ConectaFranquias.Api.Excecoes;

namespace ConectaFranquias.Api.Entities;

public class Estoque
{
    private readonly List<MovimentacaoEstoque> _movimentacoes = [];

    protected Estoque()
    {
    }

    public Estoque(int unidadeId, int produtoServicoId, int quantidadeMinima)
    {
        if (quantidadeMinima < 0)
        {
            throw new ExcecaoNegocio("A quantidade mínima de estoque não pode ser negativa.");
        }

        UnidadeId = unidadeId;
        ProdutoServicoId = produtoServicoId;
        Quantidade = 0;
        QuantidadeMinima = quantidadeMinima;
        DataAtualizacao = DateTime.UtcNow;
    }

    public int EstoqueId { get; private set; }

    public int UnidadeId { get; private set; }

    public UnidadeFranqueada Unidade { get; private set; } = null!;

    public int ProdutoServicoId { get; private set; }

    public ProdutoServico ProdutoServico { get; private set; } = null!;

    public int Quantidade { get; private set; }

    public int QuantidadeMinima { get; private set; }

    public DateTime DataAtualizacao { get; private set; }

    public IReadOnlyCollection<MovimentacaoEstoque> Movimentacoes => _movimentacoes;

    public bool AbaixoDoMinimo => Quantidade < QuantidadeMinima;

    public MovimentacaoEstoque Movimentar(
        TipoMovimentacaoEstoque tipo,
        int quantidade,
        string motivo,
        int? usuarioId,
        int? vendaId = null)
    {
        if (quantidade <= 0)
        {
            throw new ExcecaoNegocio("A quantidade movimentada deve ser maior que zero.");
        }

        var quantidadeAnterior = Quantidade;

        var novaQuantidade = tipo == TipoMovimentacaoEstoque.Entrada
            ? Quantidade + quantidade
            : Quantidade - quantidade;

        if (novaQuantidade < 0)
        {
            throw new ExcecaoNegocio(
                $"Estoque insuficiente: saldo atual de {quantidadeAnterior} unidade(s) não comporta a saída de {quantidade}.");
        }

        Quantidade = novaQuantidade;
        DataAtualizacao = DateTime.UtcNow;

        var movimentacao = new MovimentacaoEstoque(
            EstoqueId,
            tipo,
            quantidade,
            quantidadeAnterior,
            novaQuantidade,
            motivo,
            usuarioId,
            vendaId);

        _movimentacoes.Add(movimentacao);
        return movimentacao;
    }
}
