namespace ConectaFranquias.Api.Entities;

public class Fornecedor
{
    private readonly List<FornecedorProduto> _produtos = [];

    protected Fornecedor()
    {
    }

    public Fornecedor(
        string razaoSocial,
        string nomeFantasia,
        string cnpj,
        string email,
        string telefone,
        string cidade,
        string estado)
    {
        RazaoSocial = razaoSocial;
        NomeFantasia = nomeFantasia;
        Cnpj = cnpj;
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone;
        Cidade = cidade;
        Estado = estado.ToUpperInvariant();
        Ativo = true;
    }

    public int FornecedorId { get; private set; }

    public string RazaoSocial { get; private set; } = string.Empty;

    public string NomeFantasia { get; private set; } = string.Empty;

    public string Cnpj { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Telefone { get; private set; } = string.Empty;

    public string Cidade { get; private set; } = string.Empty;

    public string Estado { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    public IReadOnlyCollection<FornecedorProduto> Produtos => _produtos;

    public void AtualizarDados(
        string razaoSocial,
        string nomeFantasia,
        string email,
        string telefone,
        string cidade,
        string estado)
    {
        RazaoSocial = razaoSocial;
        NomeFantasia = nomeFantasia;
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone;
        Cidade = cidade;
        Estado = estado.ToUpperInvariant();
    }

    public void Inativar() => Ativo = false;
}
