namespace ConectaFranquias.Api.Entities;

public class Perfil
{
    private readonly List<Usuario> _usuarios = [];

    protected Perfil()
    {
    }

    public Perfil(TipoPerfil tipo, string nome, string descricao)
    {
        Tipo = tipo;
        Nome = nome;
        Descricao = descricao;
    }

    public int PerfilId { get; private set; }

    public TipoPerfil Tipo { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Descricao { get; private set; } = string.Empty;

    public IReadOnlyCollection<Usuario> Usuarios => _usuarios;
}
