using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Entities;

namespace ConectaFranquias.Api.Mapeamentos;

public static class MapeamentoDominio
{
    public static UsuarioResposta ParaResposta(this Usuario usuario) => new()
    {
        UsuarioId = usuario.UsuarioId,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Perfil = usuario.Perfil?.Tipo ?? default,
        UnidadeId = usuario.UnidadeId,
        UnidadeNome = usuario.Unidade?.NomeFantasia,
        Ativo = usuario.Ativo,
        DataCadastro = usuario.DataCadastro
    };

    public static FranqueadoraResposta ParaResposta(this Franqueadora franqueadora, int totalUnidades) => new()
    {
        FranqueadoraId = franqueadora.FranqueadoraId,
        RazaoSocial = franqueadora.RazaoSocial,
        NomeFantasia = franqueadora.NomeFantasia,
        Cnpj = franqueadora.Cnpj,
        Email = franqueadora.Email,
        Telefone = franqueadora.Telefone,
        DataFundacao = franqueadora.DataFundacao,
        TotalUnidades = totalUnidades
    };

    public static FranqueadoResposta ParaResposta(this Franqueado franqueado, int totalUnidades) => new()
    {
        FranqueadoId = franqueado.FranqueadoId,
        Nome = franqueado.Nome,
        Cpf = franqueado.Cpf,
        Email = franqueado.Email,
        Telefone = franqueado.Telefone,
        Cidade = franqueado.Cidade,
        Estado = franqueado.Estado,
        TotalUnidades = totalUnidades
    };

    public static EnderecoDto ParaDto(this Endereco endereco) => new()
    {
        Logradouro = endereco.Logradouro,
        Numero = endereco.Numero,
        Bairro = endereco.Bairro,
        Cidade = endereco.Cidade,
        Estado = endereco.Estado,
        Cep = endereco.Cep
    };

    public static Endereco ParaEntidade(this EnderecoDto dto) => new(
        dto.Logradouro.Trim(),
        dto.Numero.Trim(),
        dto.Bairro.Trim(),
        dto.Cidade.Trim(),
        dto.Estado.Trim(),
        dto.Cep.Trim());

    public static ResponsavelResposta ParaResposta(this Responsavel responsavel) => new()
    {
        ResponsavelId = responsavel.ResponsavelId,
        UnidadeId = responsavel.UnidadeId,
        Nome = responsavel.Nome,
        Cpf = responsavel.Cpf,
        Cargo = responsavel.Cargo,
        Email = responsavel.Email,
        Telefone = responsavel.Telefone,
        Ativo = responsavel.Ativo
    };

    public static UnidadeResposta ParaResposta(this UnidadeFranqueada unidade) => new()
    {
        UnidadeId = unidade.UnidadeId,
        Codigo = unidade.Codigo,
        NomeFantasia = unidade.NomeFantasia,
        RazaoSocial = unidade.RazaoSocial,
        Cnpj = unidade.Cnpj,
        Email = unidade.Email,
        Telefone = unidade.Telefone,
        Endereco = unidade.Endereco.ParaDto(),
        DataInicioContrato = unidade.DataInicioContrato,
        PercentualRoyalty = unidade.PercentualRoyalty,
        TaxaFranquiaMensal = unidade.TaxaFranquiaMensal,
        Situacao = unidade.Situacao,
        PodeRegistrarVendas = unidade.PodeRegistrarVendas,
        FranqueadoraId = unidade.FranqueadoraId,
        FranqueadoId = unidade.FranqueadoId,
        FranqueadoNome = unidade.Franqueado?.Nome ?? string.Empty,
        Responsaveis = unidade.Responsaveis.Select(ParaResposta).ToList()
    };

    public static UnidadeResumoResposta ParaResumo(this UnidadeFranqueada unidade) => new()
    {
        UnidadeId = unidade.UnidadeId,
        Codigo = unidade.Codigo,
        NomeFantasia = unidade.NomeFantasia,
        Cnpj = unidade.Cnpj,
        Cidade = unidade.Endereco.Cidade,
        Estado = unidade.Endereco.Estado,
        Situacao = unidade.Situacao,
        PercentualRoyalty = unidade.PercentualRoyalty,
        FranqueadoNome = unidade.Franqueado?.Nome ?? string.Empty
    };

    public static CategoriaResposta ParaResposta(this Categoria categoria, int totalProdutos) => new()
    {
        CategoriaId = categoria.CategoriaId,
        Nome = categoria.Nome,
        Descricao = categoria.Descricao,
        TotalProdutos = totalProdutos
    };

    public static ProdutoServicoResposta ParaResposta(this ProdutoServico produto) => new()
    {
        ProdutoServicoId = produto.ProdutoServicoId,
        Codigo = produto.Codigo,
        Nome = produto.Nome,
        Descricao = produto.Descricao,
        Tipo = produto.Tipo,
        PrecoBase = produto.PrecoBase,
        Ativo = produto.Ativo,
        ControlaEstoque = produto.ControlaEstoque,
        CategoriaId = produto.CategoriaId,
        CategoriaNome = produto.Categoria?.Nome ?? string.Empty
    };

    public static FornecedorResposta ParaResposta(this Fornecedor fornecedor) => new()
    {
        FornecedorId = fornecedor.FornecedorId,
        RazaoSocial = fornecedor.RazaoSocial,
        NomeFantasia = fornecedor.NomeFantasia,
        Cnpj = fornecedor.Cnpj,
        Email = fornecedor.Email,
        Telefone = fornecedor.Telefone,
        Cidade = fornecedor.Cidade,
        Estado = fornecedor.Estado,
        Ativo = fornecedor.Ativo,
        Produtos = fornecedor.Produtos
            .Select(vinculo => new FornecedorProdutoResposta
            {
                ProdutoServicoId = vinculo.ProdutoServicoId,
                ProdutoCodigo = vinculo.ProdutoServico?.Codigo ?? string.Empty,
                ProdutoNome = vinculo.ProdutoServico?.Nome ?? string.Empty,
                PrecoCusto = vinculo.PrecoCusto
            })
            .ToList()
    };

    public static EstoqueResposta ParaResposta(this Estoque estoque) => new()
    {
        EstoqueId = estoque.EstoqueId,
        UnidadeId = estoque.UnidadeId,
        UnidadeNome = estoque.Unidade?.NomeFantasia ?? string.Empty,
        ProdutoServicoId = estoque.ProdutoServicoId,
        ProdutoCodigo = estoque.ProdutoServico?.Codigo ?? string.Empty,
        ProdutoNome = estoque.ProdutoServico?.Nome ?? string.Empty,
        Quantidade = estoque.Quantidade,
        QuantidadeMinima = estoque.QuantidadeMinima,
        AbaixoDoMinimo = estoque.AbaixoDoMinimo,
        DataAtualizacao = estoque.DataAtualizacao
    };

    public static MovimentacaoEstoqueResposta ParaResposta(this MovimentacaoEstoque movimentacao) => new()
    {
        MovimentacaoEstoqueId = movimentacao.MovimentacaoEstoqueId,
        EstoqueId = movimentacao.EstoqueId,
        ProdutoNome = movimentacao.Estoque?.ProdutoServico?.Nome ?? string.Empty,
        Tipo = movimentacao.Tipo,
        Quantidade = movimentacao.Quantidade,
        QuantidadeAnterior = movimentacao.QuantidadeAnterior,
        QuantidadeResultante = movimentacao.QuantidadeResultante,
        Motivo = movimentacao.Motivo,
        VendaId = movimentacao.VendaId,
        DataMovimentacao = movimentacao.DataMovimentacao
    };

    public static VendaResposta ParaResposta(this Venda venda) => new()
    {
        VendaId = venda.VendaId,
        NumeroVenda = venda.NumeroVenda,
        UnidadeId = venda.UnidadeId,
        UnidadeNome = venda.Unidade?.NomeFantasia ?? string.Empty,
        DataVenda = venda.DataVenda,
        FormaPagamento = venda.FormaPagamento,
        Situacao = venda.Situacao,
        ValorTotal = venda.ValorTotal,
        Itens = venda.Itens
            .Select(item => new ItemVendaResposta
            {
                ProdutoServicoId = item.ProdutoServicoId,
                ProdutoCodigo = item.ProdutoServico?.Codigo ?? string.Empty,
                ProdutoNome = item.ProdutoServico?.Nome ?? string.Empty,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario,
                ValorTotal = item.ValorTotal
            })
            .ToList()
    };

    public static VendaResumoResposta ParaResumo(this Venda venda) => new()
    {
        VendaId = venda.VendaId,
        NumeroVenda = venda.NumeroVenda,
        UnidadeId = venda.UnidadeId,
        UnidadeNome = venda.Unidade?.NomeFantasia ?? string.Empty,
        DataVenda = venda.DataVenda,
        Situacao = venda.Situacao,
        ValorTotal = venda.ValorTotal,
        TotalItens = venda.Itens.Count
    };

    public static RoyaltyResposta ParaResposta(this Royalty royalty) => new()
    {
        RoyaltyId = royalty.RoyaltyId,
        UnidadeId = royalty.UnidadeId,
        UnidadeNome = royalty.Unidade?.NomeFantasia ?? string.Empty,
        Competencia = royalty.Competencia,
        DataInicio = royalty.DataInicio,
        DataFim = royalty.DataFim,
        FaturamentoBase = royalty.FaturamentoBase,
        PercentualAplicado = royalty.PercentualAplicado,
        ValorRoyalty = royalty.ValorRoyalty,
        TaxaFranquia = royalty.TaxaFranquia,
        ValorTotal = royalty.ValorTotal,
        Situacao = royalty.Situacao,
        DataVencimento = royalty.DataVencimento,
        DataPagamento = royalty.DataPagamento,
        ValorPago = royalty.ValorPago
    };

    public static ChamadoResposta ParaResposta(this ChamadoSuporte chamado) => new()
    {
        ChamadoSuporteId = chamado.ChamadoSuporteId,
        Protocolo = chamado.Protocolo,
        UnidadeId = chamado.UnidadeId,
        UnidadeNome = chamado.Unidade?.NomeFantasia ?? string.Empty,
        Titulo = chamado.Titulo,
        Descricao = chamado.Descricao,
        Categoria = chamado.Categoria,
        Prioridade = chamado.Prioridade,
        Situacao = chamado.Situacao,
        DataAbertura = chamado.DataAbertura,
        DataEncerramento = chamado.DataEncerramento,
        SolucaoAplicada = chamado.SolucaoAplicada,
        Interacoes = chamado.Interacoes
            .OrderBy(interacao => interacao.DataRegistro)
            .Select(interacao => new InteracaoChamadoResposta
            {
                UsuarioId = interacao.UsuarioId,
                UsuarioNome = interacao.Usuario?.Nome ?? string.Empty,
                Mensagem = interacao.Mensagem,
                SituacaoAnterior = interacao.SituacaoAnterior,
                SituacaoAtual = interacao.SituacaoAtual,
                DataRegistro = interacao.DataRegistro
            })
            .ToList()
    };

    public static ChamadoResumoResposta ParaResumo(this ChamadoSuporte chamado) => new()
    {
        ChamadoSuporteId = chamado.ChamadoSuporteId,
        Protocolo = chamado.Protocolo,
        UnidadeId = chamado.UnidadeId,
        UnidadeNome = chamado.Unidade?.NomeFantasia ?? string.Empty,
        Titulo = chamado.Titulo,
        Categoria = chamado.Categoria,
        Prioridade = chamado.Prioridade,
        Situacao = chamado.Situacao,
        DataAbertura = chamado.DataAbertura
    };
}
