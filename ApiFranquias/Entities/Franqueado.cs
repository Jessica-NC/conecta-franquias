namespace ConectaFranquias.Api.Entities;

public class Franqueado
{
    private readonly List<UnidadeFranqueada> _unidades = [];

    protected Franqueado()
    {
    }

    public Franqueado(string nome, string cpf, string email, string telefone, string cidade, string estado)
    {
        Nome = nome;
        Cpf = cpf;
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone;
        Cidade = cidade;
        Estado = estado;
        DataCadastro = DateTime.UtcNow;
    }

    public int FranqueadoId { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Cpf { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Telefone { get; private set; } = string.Empty;

    public string Cidade { get; private set; } = string.Empty;

    public string Estado { get; private set; } = string.Empty;

    public DateTime DataCadastro { get; private set; }

    public IReadOnlyCollection<UnidadeFranqueada> Unidades => _unidades;

    public void AtualizarDados(string nome, string email, string telefone, string cidade, string estado)
    {
        Nome = nome;
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone;
        Cidade = cidade;
        Estado = estado;
    }
}
