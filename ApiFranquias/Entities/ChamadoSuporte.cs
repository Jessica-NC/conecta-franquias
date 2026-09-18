using ConectaFranquias.Api.Excecoes;

namespace ConectaFranquias.Api.Entities;

public class ChamadoSuporte
{
    private readonly List<InteracaoChamado> _interacoes = [];

    protected ChamadoSuporte()
    {
    }

    public ChamadoSuporte(
        int unidadeId,
        int usuarioAberturaId,
        string protocolo,
        string titulo,
        string descricao,
        CategoriaChamado categoria,
        PrioridadeChamado prioridade)
    {
        UnidadeId = unidadeId;
        UsuarioAberturaId = usuarioAberturaId;
        Protocolo = protocolo;
        Titulo = titulo;
        Descricao = descricao;
        Categoria = categoria;
        Prioridade = prioridade;
        Situacao = SituacaoChamado.Aberto;
        DataAbertura = DateTime.UtcNow;
    }

    public int ChamadoSuporteId { get; private set; }

    public int UnidadeId { get; private set; }

    public UnidadeFranqueada Unidade { get; private set; } = null!;

    public int UsuarioAberturaId { get; private set; }

    public Usuario UsuarioAbertura { get; private set; } = null!;

    public string Protocolo { get; private set; } = string.Empty;

    public string Titulo { get; private set; } = string.Empty;

    public string Descricao { get; private set; } = string.Empty;

    public CategoriaChamado Categoria { get; private set; }

    public PrioridadeChamado Prioridade { get; private set; }

    public SituacaoChamado Situacao { get; private set; }

    public DateTime DataAbertura { get; private set; }

    public DateTime? DataEncerramento { get; private set; }

    public string? SolucaoAplicada { get; private set; }

    public IReadOnlyCollection<InteracaoChamado> Interacoes => _interacoes;

    public bool EstaEmAberto => Situacao is SituacaoChamado.Aberto or SituacaoChamado.EmAndamento;

    public void AtualizarClassificacao(
        string titulo,
        string descricao,
        CategoriaChamado categoria,
        PrioridadeChamado prioridade)
    {
        GarantirQueEstaEmAberto();
        Titulo = titulo;
        Descricao = descricao;
        Categoria = categoria;
        Prioridade = prioridade;
    }

    public InteracaoChamado RegistrarInteracao(int usuarioId, string mensagem, SituacaoChamado? novaSituacao)
    {
        var situacaoAnterior = Situacao;

        if (novaSituacao.HasValue && novaSituacao.Value != Situacao)
        {
            GarantirQueEstaEmAberto();
            Situacao = novaSituacao.Value;

            if (Situacao is SituacaoChamado.Encerrado or SituacaoChamado.Cancelado)
            {
                DataEncerramento = DateTime.UtcNow;
            }
        }

        var interacao = new InteracaoChamado(ChamadoSuporteId, usuarioId, mensagem, situacaoAnterior, Situacao);
        _interacoes.Add(interacao);
        return interacao;
    }

    public InteracaoChamado Encerrar(int usuarioId, string solucaoAplicada)
    {
        GarantirQueEstaEmAberto();

        var situacaoAnterior = Situacao;
        SolucaoAplicada = solucaoAplicada;
        Situacao = SituacaoChamado.Encerrado;
        DataEncerramento = DateTime.UtcNow;

        var interacao = new InteracaoChamado(ChamadoSuporteId, usuarioId, solucaoAplicada, situacaoAnterior, Situacao);
        _interacoes.Add(interacao);
        return interacao;
    }

    private void GarantirQueEstaEmAberto()
    {
        if (!EstaEmAberto)
        {
            throw new ExcecaoNegocio($"O chamado {Protocolo} está {Situacao} e não aceita novas alterações.");
        }
    }
}
