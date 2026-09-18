namespace ConectaFranquias.Api.Entities;

public class FornecedorProduto
{
    protected FornecedorProduto()
    {
    }

    public FornecedorProduto(int fornecedorId, int produtoServicoId, decimal precoCusto)
    {
        FornecedorId = fornecedorId;
        ProdutoServicoId = produtoServicoId;
        PrecoCusto = precoCusto;
    }

    public int FornecedorProdutoId { get; private set; }

    public int FornecedorId { get; private set; }

    public Fornecedor Fornecedor { get; private set; } = null!;

    public int ProdutoServicoId { get; private set; }

    public ProdutoServico ProdutoServico { get; private set; } = null!;

    public decimal PrecoCusto { get; private set; }

    public void AtualizarPrecoCusto(decimal precoCusto) => PrecoCusto = precoCusto;
}
