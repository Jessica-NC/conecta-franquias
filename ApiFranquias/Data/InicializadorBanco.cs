using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Services.Seguranca;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ConectaFranquias.Api.Data;

public static class InicializadorBanco
{
    public const string SenhaPadraoExemplo = "axc9457@";

    private const int SementeAleatoria = 20250815;

    public static async Task InicializarAsync(IServiceProvider provedor, CancellationToken cancelamento = default)
    {
        using var escopo = provedor.CreateScope();

        var contexto = escopo.ServiceProvider.GetRequiredService<ContextoFranquias>();
        var servicoHash = escopo.ServiceProvider.GetRequiredService<IServicoHashSenha>();
        var log = escopo.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(InicializadorBanco));

        GarantirPastaDoBanco(contexto);
        await contexto.Database.MigrateAsync(cancelamento);

        if (await contexto.Usuarios.AnyAsync(cancelamento))
        {
            log.LogInformation("Banco de dados já inicializado; carga de exemplo ignorada.");
            return;
        }

        log.LogInformation("Banco vazio detectado. Gerando carga de exemplo...");

        var sorteio = new Random(SementeAleatoria);
        var hoje = DateTime.UtcNow.Date;

        var perfis = await SemearPerfisAsync(contexto, cancelamento);
        var franqueadora = await SemearFranqueadoraAsync(contexto, cancelamento);
        var franqueados = await SemearFranqueadosAsync(contexto, cancelamento);
        var unidades = await SemearUnidadesAsync(contexto, franqueadora, franqueados, hoje, cancelamento);
        var usuarios = await SemearUsuariosAsync(contexto, perfis, unidades, servicoHash, cancelamento);

        var produtos = await SemearCatalogoAsync(contexto, cancelamento);
        await SemearFornecedoresAsync(contexto, produtos, cancelamento);

        var estoques = await SemearEstoquesAsync(contexto, unidades, produtos, usuarios[0], cancelamento);
        var vendas = await SemearVendasAsync(contexto, unidades, produtos, usuarios, sorteio, hoje, cancelamento);
        await SemearBaixasDeEstoqueAsync(contexto, vendas, estoques, produtos, cancelamento);

        await SemearRoyaltiesAsync(contexto, unidades, vendas, hoje, cancelamento);
        await SemearChamadosAsync(contexto, unidades, usuarios, sorteio, hoje, cancelamento);

        log.LogInformation(
            "Carga concluída: {Unidades} unidades, {Produtos} itens de catálogo, {Vendas} vendas.",
            unidades.Count,
            produtos.Count,
            vendas.Count);
    }

    private static void GarantirPastaDoBanco(ContextoFranquias contexto)
    {
        var conexao = contexto.Database.GetConnectionString();

        if (string.IsNullOrWhiteSpace(conexao))
        {
            return;
        }

        var caminho = Path.GetDirectoryName(
            Path.GetFullPath(new SqliteConnectionStringBuilder(conexao).DataSource));

        if (!string.IsNullOrEmpty(caminho))
        {
            Directory.CreateDirectory(caminho);
        }
    }

    private static async Task<Dictionary<TipoPerfil, Perfil>> SemearPerfisAsync(
        ContextoFranquias contexto,
        CancellationToken cancelamento)
    {
        var perfis = new List<Perfil>
        {
            new(TipoPerfil.Administrador, "Administrador da franqueadora",
                "Acesso irrestrito à rede: cadastros, apurações e relatórios consolidados."),
            new(TipoPerfil.GestorUnidade, "Gestor de unidade",
                "Administra os dados, o estoque e as vendas da própria unidade."),
            new(TipoPerfil.Operador, "Operador de unidade",
                "Registra vendas e movimenta o estoque da própria unidade.")
        };

        await contexto.Perfis.AddRangeAsync(perfis, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        return perfis.ToDictionary(perfil => perfil.Tipo);
    }

    private static async Task<Franqueadora> SemearFranqueadoraAsync(
        ContextoFranquias contexto,
        CancellationToken cancelamento)
    {
        var franqueadora = new Franqueadora(
            "Sabor & Cia Franchising Ltda.",
            "Sabor & Cia",
            GeradorDocumentosExemplo.CriarCnpj(41250700),
            "contato@saborecia.com.br",
            "(11) 3255-8800",
            new DateTime(2014, 3, 12));

        await contexto.Franqueadoras.AddAsync(franqueadora, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        return franqueadora;
    }

    private static async Task<List<Franqueado>> SemearFranqueadosAsync(
        ContextoFranquias contexto,
        CancellationToken cancelamento)
    {
        var dados = new[]
        {
            ("Mariana Alves Ribeiro", 318442907, "mariana.ribeiro@saborecia.com.br", "(11) 98812-4471", "São Paulo", "SP"),
            ("Rodrigo Teixeira Lima", 274905163, "rodrigo.lima@saborecia.com.br", "(19) 99604-2218", "Campinas", "SP"),
            ("Patrícia Nogueira Sá", 190663428, "patricia.sa@saborecia.com.br", "(21) 98455-3390", "Rio de Janeiro", "RJ"),
            ("Eduardo Farias Mendes", 452118076, "eduardo.mendes@saborecia.com.br", "(31) 98120-7745", "Belo Horizonte", "MG")
        };

        var franqueados = dados
            .Select(item => new Franqueado(
                item.Item1,
                GeradorDocumentosExemplo.CriarCpf(item.Item2),
                item.Item3,
                item.Item4,
                item.Item5,
                item.Item6))
            .ToList();

        await contexto.Franqueados.AddRangeAsync(franqueados, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        return franqueados;
    }

    private static async Task<List<UnidadeFranqueada>> SemearUnidadesAsync(
        ContextoFranquias contexto,
        Franqueadora franqueadora,
        IReadOnlyList<Franqueado> franqueados,
        DateTime hoje,
        CancellationToken cancelamento)
    {
        var dados = new[]
        {
            ("SP-001", "Sabor & Cia Paulista", "Paulista Alimentos Ltda.", 47118203,
                "paulista@saborecia.com.br", "(11) 3255-1001",
                "Avenida Paulista", "1578", "Bela Vista", "São Paulo", "SP", "01310-200",
                6.5m, 1800.00m, SituacaoUnidade.Ativa),

            ("SP-002", "Sabor & Cia Campinas", "Campinas Sabor Comércio Ltda.", 52390174,
                "campinas@saborecia.com.br", "(19) 3251-2002",
                "Avenida Norte-Sul", "820", "Cambuí", "Campinas", "SP", "13025-320",
                6.5m, 1600.00m, SituacaoUnidade.Ativa),

            ("RJ-001", "Sabor & Cia Botafogo", "Botafogo Gastronomia Ltda.", 60741285,
                "botafogo@saborecia.com.br", "(21) 2537-3003",
                "Rua Voluntários da Pátria", "445", "Botafogo", "Rio de Janeiro", "RJ", "22270-000",
                7.0m, 1900.00m, SituacaoUnidade.Ativa),

            ("MG-001", "Sabor & Cia Savassi", "Savassi Alimentação Ltda.", 73805916,
                "savassi@saborecia.com.br", "(31) 3284-4004",
                "Rua Pernambuco", "1230", "Savassi", "Belo Horizonte", "MG", "30130-151",
                6.0m, 1500.00m, SituacaoUnidade.Inativa)
        };

        var unidades = new List<UnidadeFranqueada>();

        for (var indice = 0; indice < dados.Length; indice++)
        {
            var item = dados[indice];

            var unidade = new UnidadeFranqueada(
                franqueadora.FranqueadoraId,
                franqueados[indice].FranqueadoId,
                item.Item1,
                item.Item2,
                item.Item3,
                GeradorDocumentosExemplo.CriarCnpj(item.Item4),
                item.Item5,
                item.Item6,
                new Endereco(item.Item7, item.Item8, item.Item9, item.Item10, item.Item11, item.Item12),
                hoje.AddDays(-900 + (indice * 120)),
                item.Item13,
                item.Item14);

            unidade.AlterarSituacao(item.Item15);
            unidades.Add(unidade);
        }

        await contexto.Unidades.AddRangeAsync(unidades, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        var responsaveis = new[]
        {
            (0, "Juliana Prado Martins", 205883714, "Gerente geral", "juliana.martins@saborecia.com.br", "(11) 98771-3320"),
            (1, "Renata Coelho Barros", 428915037, "Gerente geral", "renata.barros@saborecia.com.br", "(19) 99118-4407"),
            (2, "Thiago Menezes Cruz", 539026148, "Gerente geral", "thiago.cruz@saborecia.com.br", "(21) 98330-5529"),
            (3, "Bruno Carvalho Nunes", 751248360, "Gerente geral", "bruno.nunes@saborecia.com.br", "(31) 98866-2274")
        }
            .Select(item => new Responsavel(
                unidades[item.Item1].UnidadeId,
                item.Item2,
                GeradorDocumentosExemplo.CriarCpf(item.Item3),
                item.Item4,
                item.Item5,
                item.Item6))
            .ToList();

        await contexto.Responsaveis.AddRangeAsync(responsaveis, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        return unidades;
    }

    private static async Task<List<Usuario>> SemearUsuariosAsync(
        ContextoFranquias contexto,
        IReadOnlyDictionary<TipoPerfil, Perfil> perfis,
        IReadOnlyList<UnidadeFranqueada> unidades,
        IServicoHashSenha servicoHash,
        CancellationToken cancelamento)
    {
        var senhaHash = servicoHash.GerarHash(SenhaPadraoExemplo);

        var usuarios = new List<Usuario>
        {
            new("Administrador da Rede", "admin@saborecia.com.br", senhaHash,
                perfis[TipoPerfil.Administrador].PerfilId, null)
        };

        foreach (var unidade in unidades)
        {
            var apelido = unidade.Codigo.ToLowerInvariant().Replace("-", string.Empty);

            usuarios.Add(new Usuario(
                $"Gestor {unidade.NomeFantasia}",
                $"gestor.{apelido}@saborecia.com.br",
                senhaHash,
                perfis[TipoPerfil.GestorUnidade].PerfilId,
                unidade.UnidadeId));

            if (unidade.Situacao == SituacaoUnidade.Ativa)
            {
                usuarios.Add(new Usuario(
                    $"Operador {unidade.NomeFantasia}",
                    $"operador.{apelido}@saborecia.com.br",
                    senhaHash,
                    perfis[TipoPerfil.Operador].PerfilId,
                    unidade.UnidadeId));
            }
        }

        await contexto.Usuarios.AddRangeAsync(usuarios, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        usuarios.Last().Inativar();
        await contexto.SaveChangesAsync(cancelamento);

        return usuarios;
    }

    private static async Task<List<ProdutoServico>> SemearCatalogoAsync(
        ContextoFranquias contexto,
        CancellationToken cancelamento)
    {
        var categorias = new List<Categoria>
        {
            new("Pratos prontos", "Refeições completas servidas no balcão e no delivery."),
            new("Bebidas", "Bebidas quentes, geladas e sucos naturais."),
            new("Sobremesas", "Doces, tortas e sobremesas geladas."),
            new("Serviços", "Serviços prestados pela unidade, como eventos e assinaturas.")
        };

        await contexto.Categorias.AddRangeAsync(categorias, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        var dados = new[]
        {
            (0, "PRT-001", "Prato executivo da casa", "Arroz, feijão, proteína do dia e guarnição.", TipoProdutoServico.Produto, 34.90m),
            (0, "PRT-002", "Massa artesanal ao sugo", "Massa fresca com molho de tomate italiano.", TipoProdutoServico.Produto, 42.50m),
            (0, "PRT-003", "Salada premium", "Mix de folhas, grãos e proteína grelhada.", TipoProdutoServico.Produto, 29.90m),
            (0, "PRT-004", "Sanduíche artesanal", "Pão de fermentação natural com recheio da casa.", TipoProdutoServico.Produto, 31.00m),

            (1, "BEB-001", "Café espresso", "Café espresso extraído na hora.", TipoProdutoServico.Produto, 7.50m),
            (1, "BEB-002", "Suco natural 500 ml", "Suco de frutas frescas sem adição de açúcar.", TipoProdutoServico.Produto, 12.90m),
            (1, "BEB-003", "Refrigerante lata", "Refrigerante gelado 350 ml.", TipoProdutoServico.Produto, 8.00m),
            (1, "BEB-004", "Chá gelado artesanal", "Chá gelado da casa com especiarias.", TipoProdutoServico.Produto, 10.50m),

            (2, "SOB-001", "Pudim da casa", "Pudim de leite condensado tradicional.", TipoProdutoServico.Produto, 14.90m),
            (2, "SOB-002", "Torta de limão", "Fatia de torta de limão com merengue.", TipoProdutoServico.Produto, 18.50m),
            (2, "SOB-003", "Brownie com sorvete", "Brownie quente com sorvete de creme.", TipoProdutoServico.Produto, 22.00m),

            (3, "SRV-001", "Buffet para eventos", "Serviço de buffet fechado para eventos corporativos.", TipoProdutoServico.Servico, 2500.00m),
            (3, "SRV-002", "Assinatura marmitas mensal", "Plano mensal de 20 refeições entregues.", TipoProdutoServico.Servico, 649.00m)
        };

        var produtos = dados
            .Select(item => new ProdutoServico(
                categorias[item.Item1].CategoriaId,
                item.Item2,
                item.Item3,
                item.Item4,
                item.Item5,
                item.Item6))
            .ToList();

        await contexto.ProdutosServicos.AddRangeAsync(produtos, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        produtos.Single(produto => produto.Codigo == "BEB-004").Inativar();
        await contexto.SaveChangesAsync(cancelamento);

        return produtos;
    }

    private static async Task SemearFornecedoresAsync(
        ContextoFranquias contexto,
        IReadOnlyList<ProdutoServico> produtos,
        CancellationToken cancelamento)
    {
        var fornecedores = new[]
        {
            ("Distribuidora Grão Nobre Ltda.", "Grão Nobre", 33472085, "comercial@graonobre.com.br", "(11) 3644-2100", "São Paulo", "SP"),
            ("Bebidas Vale Claro S.A.", "Vale Claro", 44583196, "vendas@valeclaro.com.br", "(19) 3721-8890", "Campinas", "SP"),
            ("Laticínios Serra Azul Ltda.", "Serra Azul", 66705318, "contato@serraazul.com.br", "(31) 3455-9021", "Belo Horizonte", "MG")
        }
            .Select(item => new Fornecedor(
                item.Item1,
                item.Item2,
                GeradorDocumentosExemplo.CriarCnpj(item.Item3),
                item.Item4,
                item.Item5,
                item.Item6,
                item.Item7))
            .ToList();

        await contexto.Fornecedores.AddRangeAsync(fornecedores, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        var vinculos = new[]
        {
            (0, "PRT-001", 15.90m),
            (0, "PRT-003", 13.60m),
            (1, "BEB-001", 3.20m),
            (1, "BEB-002", 6.10m),
            (1, "BEB-003", 3.90m),
            (2, "SOB-001", 7.30m),
            (2, "SOB-002", 9.80m),
            (2, "SOB-003", 11.20m)
        }
            .Select(item => new FornecedorProduto(
                fornecedores[item.Item1].FornecedorId,
                produtos.Single(produto => produto.Codigo == item.Item2).ProdutoServicoId,
                item.Item3))
            .ToList();

        await contexto.FornecedoresProdutos.AddRangeAsync(vinculos, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);
    }

    private static async Task<List<Estoque>> SemearEstoquesAsync(
        ContextoFranquias contexto,
        IReadOnlyList<UnidadeFranqueada> unidades,
        IReadOnlyList<ProdutoServico> produtos,
        Usuario administrador,
        CancellationToken cancelamento)
    {
        var produtosComEstoque = produtos.Where(produto => produto.ControlaEstoque).ToList();
        var unidadesOperantes = unidades.Where(unidade => unidade.PodeRegistrarVendas).ToList();

        var estoques = new List<Estoque>();

        foreach (var unidade in unidadesOperantes)
        {
            foreach (var produto in produtosComEstoque)
            {
                var estoque = new Estoque(unidade.UnidadeId, produto.ProdutoServicoId, quantidadeMinima: 60);

                estoque.Movimentar(
                    TipoMovimentacaoEstoque.Entrada,
                    600,
                    "Carga inicial de implantação da unidade",
                    administrador.UsuarioId);

                estoques.Add(estoque);
            }
        }

        await contexto.Estoques.AddRangeAsync(estoques, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        foreach (var estoque in estoques.Take(5))
        {
            estoque.Movimentar(
                TipoMovimentacaoEstoque.Saida,
                estoque.Quantidade - 12,
                "Baixa por inventário: perdas e vencimento de lote",
                administrador.UsuarioId);
        }

        await contexto.SaveChangesAsync(cancelamento);

        return estoques;
    }

    private static async Task<List<Venda>> SemearVendasAsync(
        ContextoFranquias contexto,
        IReadOnlyList<UnidadeFranqueada> unidades,
        IReadOnlyList<ProdutoServico> produtos,
        IReadOnlyList<Usuario> usuarios,
        Random sorteio,
        DateTime hoje,
        CancellationToken cancelamento)
    {
        var formasPagamento = Enum.GetValues<FormaPagamento>();
        var vendaveis = produtos.Where(produto => produto.Ativo).ToList();
        var unidadesVendedoras = unidades.Where(unidade => unidade.PodeRegistrarVendas).ToList();

        var vendas = new List<Venda>();

        foreach (var unidade in unidadesVendedoras)
        {
            var totalVendas = sorteio.Next(22, 30);

            for (var indice = 1; indice <= totalVendas; indice++)
            {
                var dataVenda = hoje.AddDays(-sorteio.Next(0, 90)).AddHours(sorteio.Next(9, 22));

                var usuarioRegistro = usuarios
                    .Where(usuario => usuario.UnidadeId == unidade.UnidadeId)
                    .OrderBy(_ => sorteio.Next())
                    .First();

                var venda = new Venda(
                    unidade.UnidadeId,
                    usuarioRegistro.UsuarioId,
                    $"V{unidade.UnidadeId:D3}-{indice:D6}",
                    formasPagamento[sorteio.Next(formasPagamento.Length)],
                    dataVenda);

                var itensDaVenda = vendaveis
                    .OrderBy(_ => sorteio.Next())
                    .Take(sorteio.Next(1, 4))
                    .ToList();

                foreach (var produto in itensDaVenda)
                {
                    var quantidade = produto.ControlaEstoque ? sorteio.Next(1, 4) : 1;
                    venda.AdicionarItem(produto.ProdutoServicoId, quantidade, produto.PrecoBase);
                }

                venda.Confirmar();

                if (sorteio.Next(0, 100) < 8)
                {
                    venda.Cancelar();
                }

                vendas.Add(venda);
            }
        }

        await contexto.Vendas.AddRangeAsync(vendas, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        return vendas;
    }

    private static async Task SemearBaixasDeEstoqueAsync(
        ContextoFranquias contexto,
        IReadOnlyList<Venda> vendas,
        IReadOnlyList<Estoque> estoques,
        IReadOnlyList<ProdutoServico> produtos,
        CancellationToken cancelamento)
    {
        var controlaEstoque = produtos.ToDictionary(
            produto => produto.ProdutoServicoId,
            produto => produto.ControlaEstoque);

        var indiceEstoques = estoques.ToDictionary(estoque => (estoque.UnidadeId, estoque.ProdutoServicoId));

        foreach (var venda in vendas.Where(item => item.Situacao == SituacaoVenda.Confirmada))
        {
            foreach (var item in venda.Itens.Where(item => controlaEstoque[item.ProdutoServicoId]))
            {
                if (!indiceEstoques.TryGetValue((venda.UnidadeId, item.ProdutoServicoId), out var estoque) ||
                    estoque.Quantidade < item.Quantidade)
                {
                    continue;
                }

                estoque.Movimentar(
                    TipoMovimentacaoEstoque.Saida,
                    item.Quantidade,
                    $"Baixa automática da venda {venda.NumeroVenda}",
                    venda.UsuarioId,
                    venda.VendaId);
            }
        }

        await contexto.SaveChangesAsync(cancelamento);
    }

    private static async Task SemearRoyaltiesAsync(
        ContextoFranquias contexto,
        IReadOnlyList<UnidadeFranqueada> unidades,
        IReadOnlyList<Venda> vendas,
        DateTime hoje,
        CancellationToken cancelamento)
    {
        var royalties = new List<Royalty>();
        var unidadesApuradas = unidades.Where(unidade => unidade.PodeRegistrarVendas).ToList();

        for (var deslocamento = 2; deslocamento >= 1; deslocamento--)
        {
            var dataInicio = new DateTime(hoje.Year, hoje.Month, 1).AddMonths(-deslocamento);
            var dataFim = dataInicio.AddMonths(1).AddDays(-1);

            foreach (var unidade in unidadesApuradas)
            {
                var faturamento = vendas
                    .Where(venda => venda.UnidadeId == unidade.UnidadeId &&
                                    venda.Situacao == SituacaoVenda.Confirmada &&
                                    venda.DataVenda >= dataInicio &&
                                    venda.DataVenda <= dataFim.AddDays(1).AddTicks(-1))
                    .Sum(venda => venda.ValorTotal);

                var royalty = new Royalty(
                    unidade.UnidadeId,
                    dataInicio.Year,
                    dataInicio.Month,
                    dataInicio,
                    dataFim,
                    faturamento,
                    unidade.PercentualRoyalty,
                    unidade.TaxaFranquiaMensal,
                    dataVencimento: dataInicio.AddMonths(1).AddDays(9));

                if (deslocamento == 2)
                {
                    royalty.RegistrarPagamento(royalty.ValorTotal, royalty.DataVencimento.AddDays(-2));
                }
                else
                {
                    royalty.MarcarComoAtrasado();
                }

                royalties.Add(royalty);
            }
        }

        await contexto.Royalties.AddRangeAsync(royalties, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);
    }

    private static async Task SemearChamadosAsync(
        ContextoFranquias contexto,
        IReadOnlyList<UnidadeFranqueada> unidades,
        IReadOnlyList<Usuario> usuarios,
        Random sorteio,
        DateTime hoje,
        CancellationToken cancelamento)
    {
        var modelos = new[]
        {
            ("Divergência no repasse de royalties", "O valor apurado na competência não confere com o relatório enviado pela franqueadora.", CategoriaChamado.Financeiro, PrioridadeChamado.Alta),
            ("Sistema de PDV apresentando lentidão", "O terminal de vendas trava no fechamento de comandas em horário de pico.", CategoriaChamado.Sistema, PrioridadeChamado.Critica),
            ("Atraso na entrega do fornecedor de bebidas", "O pedido semanal não foi entregue no prazo acordado.", CategoriaChamado.Suprimentos, PrioridadeChamado.Alta),
            ("Treinamento para novos colaboradores", "Solicitamos agenda de treinamento operacional para a equipe recém-contratada.", CategoriaChamado.Operacional, PrioridadeChamado.Media),
            ("Falha na integração do delivery", "Pedidos do aplicativo não estão entrando automaticamente no sistema.", CategoriaChamado.Sistema, PrioridadeChamado.Critica),
            ("Reposição de embalagens padronizadas", "O estoque de embalagens está abaixo do mínimo e o pedido não foi processado.", CategoriaChamado.Suprimentos, PrioridadeChamado.Media),
            ("Erro no cálculo da taxa de franquia", "A taxa cobrada diverge do valor previsto em contrato.", CategoriaChamado.Financeiro, PrioridadeChamado.Alta),
            ("Revisão do cardápio regional", "Gostaríamos de avaliar a inclusão de pratos típicos da região.", CategoriaChamado.Operacional, PrioridadeChamado.Baixa)
        };

        var administrador = usuarios[0];
        var unidadesComChamados = unidades.Where(unidade => unidade.PodeRegistrarVendas).ToList();

        var chamados = new List<ChamadoSuporte>();

        for (var indice = 0; indice < modelos.Length; indice++)
        {
            var modelo = modelos[indice];
            var unidade = unidadesComChamados[sorteio.Next(unidadesComChamados.Count)];

            var autor = usuarios
                .Where(usuario => usuario.UnidadeId == unidade.UnidadeId)
                .OrderBy(_ => sorteio.Next())
                .FirstOrDefault() ?? administrador;

            chamados.Add(new ChamadoSuporte(
                unidade.UnidadeId,
                autor.UsuarioId,
                $"CH-{hoje.Year}-{indice + 1:D6}",
                modelo.Item1,
                modelo.Item2,
                modelo.Item3,
                modelo.Item4));
        }

        await contexto.Chamados.AddRangeAsync(chamados, cancelamento);
        await contexto.SaveChangesAsync(cancelamento);

        for (var indice = 0; indice < chamados.Count; indice++)
        {
            var chamado = chamados[indice];

            switch (indice % 4)
            {
                case 0:
                    break;

                case 1:
                    chamado.RegistrarInteracao(
                        administrador.UsuarioId,
                        "Chamado recebido pela central. Estamos apurando com a área responsável.",
                        SituacaoChamado.EmAndamento);
                    break;

                case 2:
                    chamado.RegistrarInteracao(
                        administrador.UsuarioId,
                        "Análise concluída pela equipe técnica.",
                        SituacaoChamado.EmAndamento);
                    chamado.Encerrar(
                        administrador.UsuarioId,
                        "Ajuste aplicado e validado junto à unidade. Chamado encerrado.");
                    break;

                default:
                    chamado.RegistrarInteracao(
                        chamado.UsuarioAberturaId,
                        "Solicitação resolvida internamente pela unidade.",
                        SituacaoChamado.Cancelado);
                    break;
            }
        }

        await contexto.SaveChangesAsync(cancelamento);
    }
}
