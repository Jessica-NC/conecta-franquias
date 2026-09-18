namespace ConectaFranquias.Api.Entities;

public class Responsavel
{
    protected Responsavel()
    {
    }

    public Responsavel(int unidadeId, string nome, string cpf, string cargo, string email, string telefone)
    {
        UnidadeId = unidadeId;
        Nome = nome;
        Cpf = cpf;
        Cargo = cargo;
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone;
        Ativo = true;
    }

    public int ResponsavelId { get; private set; }

    public int UnidadeId { get; private set; }

    public UnidadeFranqueada Unidade { get; private set; } = null!;

    public string Nome { get; private set; } = string.Empty;

    public string Cpf { get; private set; } = string.Empty;

    public string Cargo { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Telefone { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    public void AtualizarDados(string nome, string cargo, string email, string telefone)
    {
        Nome = nome;
        Cargo = cargo;
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone;
    }

    public void Inativar() => Ativo = false;
}
