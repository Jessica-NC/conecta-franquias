using ConectaFranquias.Api.Excecoes;

namespace ConectaFranquias.Api.Entities;

public class ItemVenda
{
    protected ItemVenda()
    {
    }

    internal ItemVenda(int vendaId, int produtoServicoId, int quantidade, decimal precoUnitario)
    {
        if (quantidade <= 0)
        {
            throw new ExcecaoNegocio("A quantidade do item deve ser maior que zero.");
        }

        if (precoUnitario < 0)
        {
            throw new ExcecaoNegocio("O preço unitário do item não pode ser negativo.");
        }

        VendaId = vendaId;
        ProdutoServicoId = produtoServicoId;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        ValorTotal = precoUnitario * quantidade;
    }

    public int ItemVendaId { get; private set; }

    public int VendaId { get; private set; }

    public Venda Venda { get; private set; } = null!;

    public int ProdutoServicoId { get; private set; }

    public ProdutoServico ProdutoServico { get; private set; } = null!;

    public int Quantidade { get; private set; }

    public decimal PrecoUnitario { get; private set; }

    public decimal ValorTotal { get; private set; }
}
