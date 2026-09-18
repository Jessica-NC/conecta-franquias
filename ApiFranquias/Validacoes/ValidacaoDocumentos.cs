using System.ComponentModel.DataAnnotations;

namespace ConectaFranquias.Api.Validacoes;

public static class DocumentoValidador
{
    private static readonly int[] PesosCnpjPrimeiro = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] PesosCnpjSegundo = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    public static string ApenasDigitos(string? documento) =>
        string.IsNullOrWhiteSpace(documento)
            ? string.Empty
            : new string(documento.Where(char.IsDigit).ToArray());

    public static bool CpfEhValido(string? cpf)
    {
        var digitos = ApenasDigitos(cpf);

        if (digitos.Length != 11 || digitos.Distinct().Count() == 1)
        {
            return false;
        }

        var primeiro = CalcularDigitoSequencial(digitos[..9], 10);
        var segundo = CalcularDigitoSequencial(digitos[..9] + primeiro, 11);

        return digitos.EndsWith($"{primeiro}{segundo}", StringComparison.Ordinal);
    }

    public static bool CnpjEhValido(string? cnpj)
    {
        var digitos = ApenasDigitos(cnpj);

        if (digitos.Length != 14 || digitos.Distinct().Count() == 1)
        {
            return false;
        }

        var primeiro = CalcularDigitoComPesos(digitos[..12], PesosCnpjPrimeiro);
        var segundo = CalcularDigitoComPesos(digitos[..12] + primeiro, PesosCnpjSegundo);

        return digitos.EndsWith($"{primeiro}{segundo}", StringComparison.Ordinal);
    }

    public static string FormatarCnpj(string cnpj)
    {
        var d = ApenasDigitos(cnpj);
        return d.Length != 14 ? cnpj : $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}";
    }

    public static string FormatarCpf(string cpf)
    {
        var d = ApenasDigitos(cpf);
        return d.Length != 11 ? cpf : $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}";
    }

    public static int CalcularDigitoComPesos(string baseCalculo, int[] pesos)
    {
        var soma = baseCalculo.Select((caractere, indice) => (caractere - '0') * pesos[indice]).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public static int CalcularDigitoSequencial(string baseCalculo, int pesoInicial)
    {
        var soma = baseCalculo.Select((caractere, indice) => (caractere - '0') * (pesoInicial - indice)).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class CnpjAttribute : ValidationAttribute
{
    public CnpjAttribute() : base("O campo {0} não contém um CNPJ válido.")
    {
    }

    // A obrigatoriedade fica a cargo de [Required]; aqui só validamos o formato.
    public override bool IsValid(object? value) => value is null || DocumentoValidador.CnpjEhValido(value.ToString());
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class CpfAttribute : ValidationAttribute
{
    public CpfAttribute() : base("O campo {0} não contém um CPF válido.")
    {
    }

    public override bool IsValid(object? value) => value is null || DocumentoValidador.CpfEhValido(value.ToString());
}
