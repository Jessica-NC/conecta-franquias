using ConectaFranquias.Api.Excecoes;

namespace ConectaFranquias.Api.Entities;

public class Venda
{
    private readonly List<ItemVenda> _itens = [];

    protected Venda()
    {
    }

    public Venda(int unidadeId, int usuarioId, string numeroVenda, FormaPagamento formaPagamento, DateTime dataVenda)
    {
        UnidadeId = unidadeId;
        UsuarioId = usuarioId;
        NumeroVenda = numeroVenda;
        FormaPagamento = formaPagamento;
        DataVenda = dataVenda;
        Situacao = SituacaoVenda.Pendente;
    }

    public int VendaId { get; private set; }

    public int UnidadeId { get; private set; }

    public UnidadeFranqueada Unidade { get; private set; } = null!;

    public int UsuarioId { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    public string NumeroVenda { get; private set; } = string.Empty;

    public DateTime DataVenda { get; private set; }

    public FormaPagamento FormaPagamento { get; private set; }

    public SituacaoVenda Situacao { get; private set; }

    public decimal ValorTotal { get; private set; }

    public IReadOnlyCollection<ItemVenda> Itens => _itens;

    public ItemVenda AdicionarItem(int produtoServicoId, int quantidade, decimal precoUnitario)
    {
        if (Situacao != SituacaoVenda.Pendente)
        {
            throw new ExcecaoNegocio("Somente vendas pendentes podem receber novos itens.");
        }

        var item = new ItemVenda(VendaId, produtoServicoId, quantidade, precoUnitario);
        _itens.Add(item);
        RecalcularTotal();
        return item;
    }

    public void Confirmar()
    {
        if (Situacao == SituacaoVenda.Cancelada)
        {
            throw new ExcecaoNegocio("Uma venda cancelada não pode ser confirmada.");
        }

        if (Situacao == SituacaoVenda.Confirmada)
        {
            throw new ExcecaoNegocio("Esta venda já está confirmada.");
        }

        if (_itens.Count == 0)
        {
            throw new ExcecaoNegocio("A venda deve possuir ao menos um item para ser confirmada.");
        }

        RecalcularTotal();
        Situacao = SituacaoVenda.Confirmada;
    }

    public void Cancelar()
    {
        if (Situacao == SituacaoVenda.Cancelada)
        {
            throw new ExcecaoNegocio("Esta venda já está cancelada.");
        }

        Situacao = SituacaoVenda.Cancelada;
    }

    private void RecalcularTotal() => ValorTotal = _itens.Sum(item => item.ValorTotal);
}
