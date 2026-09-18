using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConectaFranquias.Api.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.CategoriaId);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    FornecedorId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RazaoSocial = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    NomeFantasia = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Cnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.FornecedorId);
                });

            migrationBuilder.CreateTable(
                name: "Franqueadoras",
                columns: table => new
                {
                    FranqueadoraId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RazaoSocial = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    NomeFantasia = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Cnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataFundacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Franqueadoras", x => x.FranqueadoraId);
                });

            migrationBuilder.CreateTable(
                name: "Franqueados",
                columns: table => new
                {
                    FranqueadoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Franqueados", x => x.FranqueadoId);
                });

            migrationBuilder.CreateTable(
                name: "Perfis",
                columns: table => new
                {
                    PerfilId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfis", x => x.PerfilId);
                });

            migrationBuilder.CreateTable(
                name: "ProdutosServicos",
                columns: table => new
                {
                    ProdutoServicoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PrecoBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutosServicos", x => x.ProdutoServicoId);
                    table.ForeignKey(
                        name: "FK_ProdutosServicos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "CategoriaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Unidades",
                columns: table => new
                {
                    UnidadeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FranqueadoraId = table.Column<int>(type: "INTEGER", nullable: false),
                    FranqueadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NomeFantasia = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    RazaoSocial = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Cnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Logradouro = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Cep = table.Column<string>(type: "TEXT", maxLength: 9, nullable: false),
                    DataInicioContrato = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PercentualRoyalty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxaFranquiaMensal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Situacao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unidades", x => x.UnidadeId);
                    table.ForeignKey(
                        name: "FK_Unidades_Franqueadoras_FranqueadoraId",
                        column: x => x.FranqueadoraId,
                        principalTable: "Franqueadoras",
                        principalColumn: "FranqueadoraId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Unidades_Franqueados_FranqueadoId",
                        column: x => x.FranqueadoId,
                        principalTable: "Franqueados",
                        principalColumn: "FranqueadoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FornecedoresProdutos",
                columns: table => new
                {
                    FornecedorProdutoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FornecedorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoServicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecoCusto = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FornecedoresProdutos", x => x.FornecedorProdutoId);
                    table.ForeignKey(
                        name: "FK_FornecedoresProdutos_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "FornecedorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FornecedoresProdutos_ProdutosServicos_ProdutoServicoId",
                        column: x => x.ProdutoServicoId,
                        principalTable: "ProdutosServicos",
                        principalColumn: "ProdutoServicoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Estoques",
                columns: table => new
                {
                    EstoqueId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UnidadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoServicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeMinima = table.Column<int>(type: "INTEGER", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estoques", x => x.EstoqueId);
                    table.CheckConstraint("CK_Estoques_Quantidade_NaoNegativa", "Quantidade >= 0");
                    table.ForeignKey(
                        name: "FK_Estoques_ProdutosServicos_ProdutoServicoId",
                        column: x => x.ProdutoServicoId,
                        principalTable: "ProdutosServicos",
                        principalColumn: "ProdutoServicoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Estoques_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "UnidadeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Responsaveis",
                columns: table => new
                {
                    ResponsavelId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UnidadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    Cargo = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsaveis", x => x.ResponsavelId);
                    table.ForeignKey(
                        name: "FK_Responsaveis_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "UnidadeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Royalties",
                columns: table => new
                {
                    RoyaltyId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UnidadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CompetenciaAno = table.Column<int>(type: "INTEGER", nullable: false),
                    CompetenciaMes = table.Column<int>(type: "INTEGER", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FaturamentoBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PercentualAplicado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorRoyalty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxaFranquia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Situacao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataApuracao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValorPago = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Royalties", x => x.RoyaltyId);
                    table.CheckConstraint("CK_Royalties_Percentual_Valido", "PercentualAplicado >= 0 AND PercentualAplicado <= 100");
                    table.ForeignKey(
                        name: "FK_Royalties_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "UnidadeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    PerfilId = table.Column<int>(type: "INTEGER", nullable: false),
                    UnidadeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Usuarios_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "PerfilId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Usuarios_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "UnidadeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Chamados",
                columns: table => new
                {
                    ChamadoSuporteId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UnidadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioAberturaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Protocolo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Prioridade = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Situacao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataAbertura = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataEncerramento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SolucaoAplicada = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chamados", x => x.ChamadoSuporteId);
                    table.ForeignKey(
                        name: "FK_Chamados_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "UnidadeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chamados_Usuarios_UsuarioAberturaId",
                        column: x => x.UsuarioAberturaId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    VendaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UnidadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroVenda = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DataVenda = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FormaPagamento = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Situacao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.VendaId);
                    table.ForeignKey(
                        name: "FK_Vendas_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "UnidadeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InteracoesChamado",
                columns: table => new
                {
                    InteracaoChamadoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChamadoSuporteId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Mensagem = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SituacaoAnterior = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SituacaoAtual = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataRegistro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteracoesChamado", x => x.InteracaoChamadoId);
                    table.ForeignKey(
                        name: "FK_InteracoesChamado_Chamados_ChamadoSuporteId",
                        column: x => x.ChamadoSuporteId,
                        principalTable: "Chamados",
                        principalColumn: "ChamadoSuporteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InteracoesChamado_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItensVenda",
                columns: table => new
                {
                    ItemVendaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VendaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoServicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensVenda", x => x.ItemVendaId);
                    table.CheckConstraint("CK_ItensVenda_Quantidade_Positiva", "Quantidade > 0");
                    table.ForeignKey(
                        name: "FK_ItensVenda_ProdutosServicos_ProdutoServicoId",
                        column: x => x.ProdutoServicoId,
                        principalTable: "ProdutosServicos",
                        principalColumn: "ProdutoServicoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItensVenda_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "VendaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    MovimentacaoEstoqueId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EstoqueId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeAnterior = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeResultante = table.Column<int>(type: "INTEGER", nullable: false),
                    Motivo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    VendaId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataMovimentacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.MovimentacaoEstoqueId);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Estoques_EstoqueId",
                        column: x => x.EstoqueId,
                        principalTable: "Estoques",
                        principalColumn: "EstoqueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "VendaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nome_Unico",
                table: "Categorias",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chamados_Protocolo_Unico",
                table: "Chamados",
                column: "Protocolo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chamados_Situacao_Prioridade",
                table: "Chamados",
                columns: new[] { "Situacao", "Prioridade" });

            migrationBuilder.CreateIndex(
                name: "IX_Chamados_UnidadeId",
                table: "Chamados",
                column: "UnidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Chamados_UsuarioAberturaId",
                table: "Chamados",
                column: "UsuarioAberturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoques_ProdutoServicoId",
                table: "Estoques",
                column: "ProdutoServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoques_Unidade_Produto_Unico",
                table: "Estoques",
                columns: new[] { "UnidadeId", "ProdutoServicoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedores_Cnpj_Unico",
                table: "Fornecedores",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedores_NomeFantasia",
                table: "Fornecedores",
                column: "NomeFantasia");

            migrationBuilder.CreateIndex(
                name: "IX_FornecedoresProdutos_ProdutoServicoId",
                table: "FornecedoresProdutos",
                column: "ProdutoServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_FornecedoresProdutos_Vinculo_Unico",
                table: "FornecedoresProdutos",
                columns: new[] { "FornecedorId", "ProdutoServicoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Franqueadoras_Cnpj_Unico",
                table: "Franqueadoras",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Franqueados_Cpf_Unico",
                table: "Franqueados",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InteracoesChamado_ChamadoSuporteId",
                table: "InteracoesChamado",
                column: "ChamadoSuporteId");

            migrationBuilder.CreateIndex(
                name: "IX_InteracoesChamado_UsuarioId",
                table: "InteracoesChamado",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_ProdutoServicoId",
                table: "ItensVenda",
                column: "ProdutoServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_VendaId",
                table: "ItensVenda",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_EstoqueId",
                table: "MovimentacoesEstoque",
                column: "EstoqueId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_UsuarioId",
                table: "MovimentacoesEstoque",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_VendaId",
                table: "MovimentacoesEstoque",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Perfis_Tipo",
                table: "Perfis",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProdutosServicos_CategoriaId",
                table: "ProdutosServicos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutosServicos_Codigo_Unico",
                table: "ProdutosServicos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProdutosServicos_Nome",
                table: "ProdutosServicos",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_Responsaveis_Unidade_Cpf_Unico",
                table: "Responsaveis",
                columns: new[] { "UnidadeId", "Cpf" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Royalties_Unidade_Competencia_Unico",
                table: "Royalties",
                columns: new[] { "UnidadeId", "CompetenciaAno", "CompetenciaMes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_Cidade",
                table: "Unidades",
                column: "Cidade");

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_Cnpj_Unico",
                table: "Unidades",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_Codigo_Unico",
                table: "Unidades",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_FranqueadoId",
                table: "Unidades",
                column: "FranqueadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_FranqueadoraId",
                table: "Unidades",
                column: "FranqueadoraId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email_Unico",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PerfilId",
                table: "Usuarios",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UnidadeId",
                table: "Usuarios",
                column: "UnidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_Unidade_Numero_Unico",
                table: "Vendas",
                columns: new[] { "UnidadeId", "NumeroVenda" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_UnidadeId_DataVenda",
                table: "Vendas",
                columns: new[] { "UnidadeId", "DataVenda" });

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_UsuarioId",
                table: "Vendas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FornecedoresProdutos");

            migrationBuilder.DropTable(
                name: "InteracoesChamado");

            migrationBuilder.DropTable(
                name: "ItensVenda");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "Responsaveis");

            migrationBuilder.DropTable(
                name: "Royalties");

            migrationBuilder.DropTable(
                name: "Fornecedores");

            migrationBuilder.DropTable(
                name: "Chamados");

            migrationBuilder.DropTable(
                name: "Estoques");

            migrationBuilder.DropTable(
                name: "Vendas");

            migrationBuilder.DropTable(
                name: "ProdutosServicos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Perfis");

            migrationBuilder.DropTable(
                name: "Unidades");

            migrationBuilder.DropTable(
                name: "Franqueadoras");

            migrationBuilder.DropTable(
                name: "Franqueados");
        }
    }
}
