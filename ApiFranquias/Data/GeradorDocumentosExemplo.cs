using ConectaFranquias.Api.Validacoes;

namespace ConectaFranquias.Api.Data;

internal static class GeradorDocumentosExemplo
{
    private static readonly int[] PesosPrimeiro = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] PesosSegundo = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    public static string CriarCnpj(int raiz)
    {
        var baseCnpj = $"{raiz:D8}0001";

        var primeiro = DocumentoValidador.CalcularDigitoComPesos(baseCnpj, PesosPrimeiro);
        var segundo = DocumentoValidador.CalcularDigitoComPesos(baseCnpj + primeiro, PesosSegundo);

        return DocumentoValidador.FormatarCnpj($"{baseCnpj}{primeiro}{segundo}");
    }

    public static string CriarCpf(int baseNumerica)
    {
        var baseCpf = $"{baseNumerica:D9}";

        var primeiro = DocumentoValidador.CalcularDigitoSequencial(baseCpf, 10);
        var segundo = DocumentoValidador.CalcularDigitoSequencial(baseCpf + primeiro, 11);

        return DocumentoValidador.FormatarCpf($"{baseCpf}{primeiro}{segundo}");
    }
}
