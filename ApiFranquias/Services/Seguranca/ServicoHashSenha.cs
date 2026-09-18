using System.Security.Cryptography;

namespace ConectaFranquias.Api.Services.Seguranca;

public interface IServicoHashSenha
{
    string GerarHash(string senha);

    bool SenhaConfere(string senha, string hashArmazenado);
}

public class ServicoHashSenha : IServicoHashSenha
{
    private const int TamanhoSalBytes = 16;
    private const int TamanhoHashBytes = 32;
    private const int IteracoesPadrao = 100_000;

    private static readonly HashAlgorithmName Algoritmo = HashAlgorithmName.SHA256;

    public string GerarHash(string senha)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanhoSalBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, IteracoesPadrao, Algoritmo, TamanhoHashBytes);

        return $"{IteracoesPadrao}.{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
    }

    public bool SenhaConfere(string senha, string hashArmazenado)
    {
        var partes = hashArmazenado.Split('.', 3);

        if (partes.Length != 3 || !int.TryParse(partes[0], out var iteracoes))
        {
            return false;
        }

        byte[] sal;
        byte[] hashEsperado;

        try
        {
            sal = Convert.FromBase64String(partes[1]);
            hashEsperado = Convert.FromBase64String(partes[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var hashInformado = Rfc2898DeriveBytes.Pbkdf2(senha, sal, iteracoes, Algoritmo, hashEsperado.Length);

        // Comparação em tempo fixo evita vazar informação por tempo de resposta.
        return CryptographicOperations.FixedTimeEquals(hashInformado, hashEsperado);
    }
}
