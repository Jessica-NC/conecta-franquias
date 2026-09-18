namespace ConectaFranquias.Api.Entities;

public class MovimentacaoEstoque
{
    protected MovimentacaoEstoque()
    {
    }

    internal MovimentacaoEstoque(
        int estoqueId,
        TipoMovimentacaoEstoque tipo,
        int quantidade,
        int quantidadeAnterior,
        int quantidadeResultante,
        string motivo,
        int? usuarioId,
        int? vendaId)
    {
        EstoqueId = estoqueId;
        Tipo = tipo;
        Quantidade = quantidade;
        QuantidadeAnterior = quantidadeAnterior;
        QuantidadeResultante = quantidadeResultante;
        Motivo = motivo;
        UsuarioId = usuarioId;
        VendaId = vendaId;
        DataMovimentacao = DateTime.UtcNow;
    }

    public int MovimentacaoEstoqueId { get; private set; }

    public int EstoqueId { get; private set; }

    public Estoque Estoque { get; private set; } = null!;

    public TipoMovimentacaoEstoque Tipo { get; private set; }

    public int Quantidade { get; private set; }

    public int QuantidadeAnterior { get; private set; }

    public int QuantidadeResultante { get; private set; }

    public string Motivo { get; private set; } = string.Empty;

    public int? UsuarioId { get; private set; }

    public Usuario? Usuario { get; private set; }

    public int? VendaId { get; private set; }

    public Venda? Venda { get; private set; }

    public DateTime DataMovimentacao { get; private set; }
}
