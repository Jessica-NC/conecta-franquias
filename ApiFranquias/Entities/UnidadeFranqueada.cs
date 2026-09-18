namespace ConectaFranquias.Api.Entities;

public class UnidadeFranqueada
{
    private readonly List<Responsavel> _responsaveis = [];
    private readonly List<Estoque> _estoques = [];
    private readonly List<Venda> _vendas = [];
    private readonly List<Royalty> _royalties = [];
    private readonly List<ChamadoSuporte> _chamados = [];

    protected UnidadeFranqueada()
    {
    }

    public UnidadeFranqueada(
        int franqueadoraId,
        int franqueadoId,
        string codigo,
        string nomeFantasia,
        string razaoSocial,
        string cnpj,
        string email,
        string telefone,
        Endereco endereco,
        DateTime dataInicioContrato,
        decimal percentualRoyalty,
        decimal taxaFranquiaMensal)
    {
        FranqueadoraId = franqueadoraId;
        FranqueadoId = franqueadoId;
        Codigo = codigo;
        NomeFantasia = nomeFantasia;
        RazaoSocial = razaoSocial;
        Cnpj = cnpj;
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone;
        Endereco = endereco;
        DataInicioContrato = dataInicioContrato;
        PercentualRoyalty = percentualRoyalty;
        TaxaFranquiaMensal = taxaFranquiaMensal;
        Situacao = SituacaoUnidade.Ativa;
        DataCadastro = DateTime.UtcNow;
    }

    public int UnidadeId { get; private set; }

    public int FranqueadoraId { get; private set; }

    public Franqueadora Franqueadora { get; private set; } = null!;

    public int FranqueadoId { get; private set; }

    public Franqueado Franqueado { get; private set; } = null!;

    public string Codigo { get; private set; } = string.Empty;

    public string NomeFantasia { get; private set; } = string.Empty;

    public string RazaoSocial { get; private set; } = string.Empty;

    public string Cnpj { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Telefone { get; private set; } = string.Empty;

    public Endereco Endereco { get; private set; } = null!;

    public DateTime DataInicioContrato { get; private set; }

    public decimal PercentualRoyalty { get; private set; }

    public decimal TaxaFranquiaMensal { get; private set; }

    public SituacaoUnidade Situacao { get; private set; }

    public DateTime DataCadastro { get; private set; }

    public IReadOnlyCollection<Responsavel> Responsaveis => _responsaveis;

    public IReadOnlyCollection<Estoque> Estoques => _estoques;

    public IReadOnlyCollection<Venda> Vendas => _vendas;

    public IReadOnlyCollection<Royalty> Royalties => _royalties;

    public IReadOnlyCollection<ChamadoSuporte> Chamados => _chamados;

    public bool PodeRegistrarVendas => Situacao == SituacaoUnidade.Ativa;

    public void AtualizarDados(
        int franqueadoId,
        string nomeFantasia,
        string razaoSocial,
        string email,
        string telefone,
        Endereco endereco,
        decimal percentualRoyalty,
        decimal taxaFranquiaMensal)
    {
        FranqueadoId = franqueadoId;
        NomeFantasia = nomeFantasia;
        RazaoSocial = razaoSocial;
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone;
        Endereco = endereco;
        PercentualRoyalty = percentualRoyalty;
        TaxaFranquiaMensal = taxaFranquiaMensal;
    }

    public void AlterarSituacao(SituacaoUnidade situacao) => Situacao = situacao;
}
