using ConectaFranquias.Api.Excecoes;

namespace ConectaFranquias.Api.Entities;

public class Royalty
{
    protected Royalty()
    {
    }

    public Royalty(
        int unidadeId,
        int competenciaAno,
        int competenciaMes,
        DateTime dataInicio,
        DateTime dataFim,
        decimal faturamentoBase,
        decimal percentualAplicado,
        decimal taxaFranquia,
        DateTime dataVencimento)
    {
        if (percentualAplicado < 0 || percentualAplicado > 100)
        {
            throw new ExcecaoNegocio("O percentual de royalty deve estar entre 0 e 100.");
        }

        UnidadeId = unidadeId;
        CompetenciaAno = competenciaAno;
        CompetenciaMes = competenciaMes;
        DataInicio = dataInicio;
        DataFim = dataFim;
        FaturamentoBase = faturamentoBase;
        PercentualAplicado = percentualAplicado;
        TaxaFranquia = taxaFranquia;
        DataVencimento = dataVencimento;
        Situacao = SituacaoRoyalty.Pendente;
        DataApuracao = DateTime.UtcNow;
        Recalcular();
    }

    public int RoyaltyId { get; private set; }

    public int UnidadeId { get; private set; }

    public UnidadeFranqueada Unidade { get; private set; } = null!;

    public int CompetenciaAno { get; private set; }

    public int CompetenciaMes { get; private set; }

    public DateTime DataInicio { get; private set; }

    public DateTime DataFim { get; private set; }

    public decimal FaturamentoBase { get; private set; }

    public decimal PercentualAplicado { get; private set; }

    public decimal ValorRoyalty { get; private set; }

    public decimal TaxaFranquia { get; private set; }

    public decimal ValorTotal { get; private set; }

    public SituacaoRoyalty Situacao { get; private set; }

    public DateTime DataApuracao { get; private set; }

    public DateTime DataVencimento { get; private set; }

    public DateTime? DataPagamento { get; private set; }

    public decimal? ValorPago { get; private set; }

    public string Competencia => $"{CompetenciaAno:D4}-{CompetenciaMes:D2}";

    public void AtualizarApuracao(decimal faturamentoBase, decimal percentualAplicado, decimal taxaFranquia)
    {
        if (Situacao == SituacaoRoyalty.Pago)
        {
            throw new ExcecaoNegocio("Um royalty já pago não pode ser reapurado.");
        }

        FaturamentoBase = faturamentoBase;
        PercentualAplicado = percentualAplicado;
        TaxaFranquia = taxaFranquia;
        DataApuracao = DateTime.UtcNow;
        Recalcular();
    }

    public void RegistrarPagamento(decimal valorPago, DateTime dataPagamento)
    {
        if (Situacao == SituacaoRoyalty.Pago)
        {
            throw new ExcecaoNegocio("Este royalty já está quitado.");
        }

        if (valorPago <= 0)
        {
            throw new ExcecaoNegocio("O valor pago deve ser maior que zero.");
        }

        ValorPago = valorPago;
        DataPagamento = dataPagamento;
        Situacao = SituacaoRoyalty.Pago;
    }

    public void MarcarComoAtrasado()
    {
        if (Situacao == SituacaoRoyalty.Pendente && DateTime.UtcNow.Date > DataVencimento.Date)
        {
            Situacao = SituacaoRoyalty.Atrasado;
        }
    }

    private void Recalcular()
    {
        ValorRoyalty = Math.Round(FaturamentoBase * (PercentualAplicado / 100m), 2, MidpointRounding.AwayFromZero);
        ValorTotal = ValorRoyalty + TaxaFranquia;
    }
}
