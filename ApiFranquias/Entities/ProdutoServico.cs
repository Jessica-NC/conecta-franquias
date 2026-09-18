namespace ConectaFranquias.Api.Entities;

public class ProdutoServico
{
    private readonly List<FornecedorProduto> _fornecedores = [];
    private readonly List<Estoque> _estoques = [];

    protected ProdutoServico()
    {
    }

    public ProdutoServico(
        int categoriaId,
        string codigo,
        string nome,
        string descricao,
        TipoProdutoServico tipo,
        decimal precoBase)
    {
        CategoriaId = categoriaId;
        Codigo = codigo.Trim().ToUpperInvariant();
        Nome = nome;
        Descricao = descricao;
        Tipo = tipo;
        PrecoBase = precoBase;
        Ativo = true;
    }

    public int ProdutoServicoId { get; private set; }

    public int CategoriaId { get; private set; }

    public Categoria Categoria { get; private set; } = null!;

    public string Codigo { get; private set; } = string.Empty;

    public string Nome { get; private set; } = string.Empty;

    public string Descricao { get; private set; } = string.Empty;

    public TipoProdutoServico Tipo { get; private set; }

    public decimal PrecoBase { get; private set; }

    public bool Ativo { get; private set; }

    public IReadOnlyCollection<FornecedorProduto> Fornecedores => _fornecedores;

    public IReadOnlyCollection<Estoque> Estoques => _estoques;

    public bool ControlaEstoque => Tipo == TipoProdutoServico.Produto;

    public void AtualizarDados(
        int categoriaId,
        string nome,
        string descricao,
        TipoProdutoServico tipo,
        decimal precoBase)
    {
        CategoriaId = categoriaId;
        Nome = nome;
        Descricao = descricao;
        Tipo = tipo;
        PrecoBase = precoBase;
    }

    public void Inativar() => Ativo = false;
}
