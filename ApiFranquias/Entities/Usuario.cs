namespace ConectaFranquias.Api.Entities;

public class Usuario
{
    protected Usuario()
    {
    }

    public Usuario(string nome, string email, string senhaHash, int perfilId, int? unidadeId)
    {
        Nome = nome;
        Email = email.Trim().ToLowerInvariant();
        SenhaHash = senhaHash;
        PerfilId = perfilId;
        UnidadeId = unidadeId;
        Ativo = true;
        DataCadastro = DateTime.UtcNow;
    }

    public int UsuarioId { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string SenhaHash { get; private set; } = string.Empty;

    public int PerfilId { get; private set; }

    public Perfil Perfil { get; private set; } = null!;

    public int? UnidadeId { get; private set; }

    public UnidadeFranqueada? Unidade { get; private set; }

    public bool Ativo { get; private set; }

    public DateTime DataCadastro { get; private set; }

    public void AtualizarDados(string nome, string email, int perfilId, int? unidadeId)
    {
        Nome = nome;
        Email = email.Trim().ToLowerInvariant();
        PerfilId = perfilId;
        UnidadeId = unidadeId;
    }

    public void Ativar() => Ativo = true;

    public void Inativar() => Ativo = false;
}
