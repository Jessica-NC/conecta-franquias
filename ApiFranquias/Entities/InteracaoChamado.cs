namespace ConectaFranquias.Api.Entities;

public class InteracaoChamado
{
    protected InteracaoChamado()
    {
    }

    internal InteracaoChamado(
        int chamadoSuporteId,
        int usuarioId,
        string mensagem,
        SituacaoChamado situacaoAnterior,
        SituacaoChamado situacaoAtual)
    {
        ChamadoSuporteId = chamadoSuporteId;
        UsuarioId = usuarioId;
        Mensagem = mensagem;
        SituacaoAnterior = situacaoAnterior;
        SituacaoAtual = situacaoAtual;
        DataRegistro = DateTime.UtcNow;
    }

    public int InteracaoChamadoId { get; private set; }

    public int ChamadoSuporteId { get; private set; }

    public ChamadoSuporte Chamado { get; private set; } = null!;

    public int UsuarioId { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    public string Mensagem { get; private set; } = string.Empty;

    public SituacaoChamado SituacaoAnterior { get; private set; }

    public SituacaoChamado SituacaoAtual { get; private set; }

    public DateTime DataRegistro { get; private set; }
}
