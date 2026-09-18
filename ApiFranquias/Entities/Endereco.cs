namespace ConectaFranquias.Api.Entities;

public class Endereco
{
    protected Endereco()
    {
    }

    public Endereco(string logradouro, string numero, string bairro, string cidade, string estado, string cep)
    {
        Logradouro = logradouro;
        Numero = numero;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado.ToUpperInvariant();
        Cep = cep;
    }

    public string Logradouro { get; private set; } = string.Empty;

    public string Numero { get; private set; } = string.Empty;

    public string Bairro { get; private set; } = string.Empty;

    public string Cidade { get; private set; } = string.Empty;

    public string Estado { get; private set; } = string.Empty;

    public string Cep { get; private set; } = string.Empty;
}
