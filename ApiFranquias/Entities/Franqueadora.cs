namespace ConectaFranquias.Api.Entities;

public class Franqueadora
{
    private readonly List<UnidadeFranqueada> _unidades = [];

    protected Franqueadora()
    {
    }

    public Franqueadora(
        string razaoSocial,
        string nomeFantasia,
        string cnpj,
        string email,
        string telefone,
        DateTime dataFundacao)
    {
        RazaoSocial = razaoSocial;
        NomeFantasia = nomeFantasia;
        Cnpj = cnpj;
        Email = email;
        Telefone = telefone;
        DataFundacao = dataFundacao;
    }

    public int FranqueadoraId { get; private set; }

    public string RazaoSocial { get; private set; } = string.Empty;

    public string NomeFantasia { get; private set; } = string.Empty;

    public string Cnpj { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Telefone { get; private set; } = string.Empty;

    public DateTime DataFundacao { get; private set; }

    public IReadOnlyCollection<UnidadeFranqueada> Unidades => _unidades;

    public void AtualizarDados(string razaoSocial, string nomeFantasia, string email, string telefone)
    {
        RazaoSocial = razaoSocial;
        NomeFantasia = nomeFantasia;
        Email = email;
        Telefone = telefone;
    }
}
