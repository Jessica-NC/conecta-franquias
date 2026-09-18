namespace ConectaFranquias.Api.Entities;

public class Categoria
{
    private readonly List<ProdutoServico> _produtos = [];

    protected Categoria()
    {
    }

    public Categoria(string nome, string descricao)
    {
        Nome = nome;
        Descricao = descricao;
    }

    public int CategoriaId { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Descricao { get; private set; } = string.Empty;

    public IReadOnlyCollection<ProdutoServico> Produtos => _produtos;

    public void AtualizarDados(string nome, string descricao)
    {
        Nome = nome;
        Descricao = descricao;
    }
}
