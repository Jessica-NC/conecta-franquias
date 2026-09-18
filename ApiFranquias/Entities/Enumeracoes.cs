namespace ConectaFranquias.Api.Entities;

public enum TipoPerfil
{
    Administrador = 1,

    GestorUnidade = 2,

    Operador = 3
}

public enum SituacaoUnidade
{
    Ativa = 1,
    Suspensa = 2,
    Inativa = 3
}

public enum TipoProdutoServico
{
    Produto = 1,
    Servico = 2
}

public enum TipoMovimentacaoEstoque
{
    Entrada = 1,
    Saida = 2
}

public enum SituacaoVenda
{
    Pendente = 1,
    Confirmada = 2,
    Cancelada = 3
}

public enum FormaPagamento
{
    Dinheiro = 1,
    CartaoDebito = 2,
    CartaoCredito = 3,
    Pix = 4,
    Boleto = 5
}

public enum SituacaoRoyalty
{
    Pendente = 1,
    Pago = 2,
    Atrasado = 3
}

public enum CategoriaChamado
{
    Financeiro = 1,
    Operacional = 2,
    Sistema = 3,
    Suprimentos = 4,
    Outros = 5
}

public enum PrioridadeChamado
{
    Baixa = 1,
    Media = 2,
    Alta = 3,
    Critica = 4
}

public enum SituacaoChamado
{
    Aberto = 1,
    EmAndamento = 2,
    Encerrado = 3,
    Cancelado = 4
}
