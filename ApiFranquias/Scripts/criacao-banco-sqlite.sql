CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Categorias" (
    "CategoriaId" INTEGER NOT NULL CONSTRAINT "PK_Categorias" PRIMARY KEY AUTOINCREMENT,
    "Nome" TEXT NOT NULL,
    "Descricao" TEXT NOT NULL
);

CREATE TABLE "Fornecedores" (
    "FornecedorId" INTEGER NOT NULL CONSTRAINT "PK_Fornecedores" PRIMARY KEY AUTOINCREMENT,
    "RazaoSocial" TEXT NOT NULL,
    "NomeFantasia" TEXT NOT NULL,
    "Cnpj" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Telefone" TEXT NOT NULL,
    "Cidade" TEXT NOT NULL,
    "Estado" TEXT NOT NULL,
    "Ativo" INTEGER NOT NULL
);

CREATE TABLE "Franqueadoras" (
    "FranqueadoraId" INTEGER NOT NULL CONSTRAINT "PK_Franqueadoras" PRIMARY KEY AUTOINCREMENT,
    "RazaoSocial" TEXT NOT NULL,
    "NomeFantasia" TEXT NOT NULL,
    "Cnpj" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Telefone" TEXT NOT NULL,
    "DataFundacao" TEXT NOT NULL
);

CREATE TABLE "Franqueados" (
    "FranqueadoId" INTEGER NOT NULL CONSTRAINT "PK_Franqueados" PRIMARY KEY AUTOINCREMENT,
    "Nome" TEXT NOT NULL,
    "Cpf" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Telefone" TEXT NOT NULL,
    "Cidade" TEXT NOT NULL,
    "Estado" TEXT NOT NULL,
    "DataCadastro" TEXT NOT NULL
);

CREATE TABLE "Perfis" (
    "PerfilId" INTEGER NOT NULL CONSTRAINT "PK_Perfis" PRIMARY KEY AUTOINCREMENT,
    "Tipo" TEXT NOT NULL,
    "Nome" TEXT NOT NULL,
    "Descricao" TEXT NOT NULL
);

CREATE TABLE "ProdutosServicos" (
    "ProdutoServicoId" INTEGER NOT NULL CONSTRAINT "PK_ProdutosServicos" PRIMARY KEY AUTOINCREMENT,
    "CategoriaId" INTEGER NOT NULL,
    "Codigo" TEXT NOT NULL,
    "Nome" TEXT NOT NULL,
    "Descricao" TEXT NOT NULL,
    "Tipo" TEXT NOT NULL,
    "PrecoBase" decimal(18,2) NOT NULL,
    "Ativo" INTEGER NOT NULL,
    CONSTRAINT "FK_ProdutosServicos_Categorias_CategoriaId" FOREIGN KEY ("CategoriaId") REFERENCES "Categorias" ("CategoriaId") ON DELETE RESTRICT
);

CREATE TABLE "Unidades" (
    "UnidadeId" INTEGER NOT NULL CONSTRAINT "PK_Unidades" PRIMARY KEY AUTOINCREMENT,
    "FranqueadoraId" INTEGER NOT NULL,
    "FranqueadoId" INTEGER NOT NULL,
    "Codigo" TEXT NOT NULL,
    "NomeFantasia" TEXT NOT NULL,
    "RazaoSocial" TEXT NOT NULL,
    "Cnpj" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Telefone" TEXT NOT NULL,
    "Logradouro" TEXT NOT NULL,
    "Numero" TEXT NOT NULL,
    "Bairro" TEXT NOT NULL,
    "Cidade" TEXT NOT NULL,
    "Estado" TEXT NOT NULL,
    "Cep" TEXT NOT NULL,
    "DataInicioContrato" TEXT NOT NULL,
    "PercentualRoyalty" decimal(18,2) NOT NULL,
    "TaxaFranquiaMensal" decimal(18,2) NOT NULL,
    "Situacao" TEXT NOT NULL,
    "DataCadastro" TEXT NOT NULL,
    CONSTRAINT "FK_Unidades_Franqueadoras_FranqueadoraId" FOREIGN KEY ("FranqueadoraId") REFERENCES "Franqueadoras" ("FranqueadoraId") ON DELETE RESTRICT,
    CONSTRAINT "FK_Unidades_Franqueados_FranqueadoId" FOREIGN KEY ("FranqueadoId") REFERENCES "Franqueados" ("FranqueadoId") ON DELETE RESTRICT
);

CREATE TABLE "FornecedoresProdutos" (
    "FornecedorProdutoId" INTEGER NOT NULL CONSTRAINT "PK_FornecedoresProdutos" PRIMARY KEY AUTOINCREMENT,
    "FornecedorId" INTEGER NOT NULL,
    "ProdutoServicoId" INTEGER NOT NULL,
    "PrecoCusto" decimal(18,2) NOT NULL,
    CONSTRAINT "FK_FornecedoresProdutos_Fornecedores_FornecedorId" FOREIGN KEY ("FornecedorId") REFERENCES "Fornecedores" ("FornecedorId") ON DELETE CASCADE,
    CONSTRAINT "FK_FornecedoresProdutos_ProdutosServicos_ProdutoServicoId" FOREIGN KEY ("ProdutoServicoId") REFERENCES "ProdutosServicos" ("ProdutoServicoId") ON DELETE CASCADE
);

CREATE TABLE "Estoques" (
    "EstoqueId" INTEGER NOT NULL CONSTRAINT "PK_Estoques" PRIMARY KEY AUTOINCREMENT,
    "UnidadeId" INTEGER NOT NULL,
    "ProdutoServicoId" INTEGER NOT NULL,
    "Quantidade" INTEGER NOT NULL,
    "QuantidadeMinima" INTEGER NOT NULL,
    "DataAtualizacao" TEXT NOT NULL,
    CONSTRAINT "CK_Estoques_Quantidade_NaoNegativa" CHECK (Quantidade >= 0),
    CONSTRAINT "FK_Estoques_ProdutosServicos_ProdutoServicoId" FOREIGN KEY ("ProdutoServicoId") REFERENCES "ProdutosServicos" ("ProdutoServicoId") ON DELETE RESTRICT,
    CONSTRAINT "FK_Estoques_Unidades_UnidadeId" FOREIGN KEY ("UnidadeId") REFERENCES "Unidades" ("UnidadeId") ON DELETE RESTRICT
);

CREATE TABLE "Responsaveis" (
    "ResponsavelId" INTEGER NOT NULL CONSTRAINT "PK_Responsaveis" PRIMARY KEY AUTOINCREMENT,
    "UnidadeId" INTEGER NOT NULL,
    "Nome" TEXT NOT NULL,
    "Cpf" TEXT NOT NULL,
    "Cargo" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Telefone" TEXT NOT NULL,
    "Ativo" INTEGER NOT NULL,
    CONSTRAINT "FK_Responsaveis_Unidades_UnidadeId" FOREIGN KEY ("UnidadeId") REFERENCES "Unidades" ("UnidadeId") ON DELETE CASCADE
);

CREATE TABLE "Royalties" (
    "RoyaltyId" INTEGER NOT NULL CONSTRAINT "PK_Royalties" PRIMARY KEY AUTOINCREMENT,
    "UnidadeId" INTEGER NOT NULL,
    "CompetenciaAno" INTEGER NOT NULL,
    "CompetenciaMes" INTEGER NOT NULL,
    "DataInicio" TEXT NOT NULL,
    "DataFim" TEXT NOT NULL,
    "FaturamentoBase" decimal(18,2) NOT NULL,
    "PercentualAplicado" decimal(18,2) NOT NULL,
    "ValorRoyalty" decimal(18,2) NOT NULL,
    "TaxaFranquia" decimal(18,2) NOT NULL,
    "ValorTotal" decimal(18,2) NOT NULL,
    "Situacao" TEXT NOT NULL,
    "DataApuracao" TEXT NOT NULL,
    "DataVencimento" TEXT NOT NULL,
    "DataPagamento" TEXT NULL,
    "ValorPago" decimal(18,2) NULL,
    CONSTRAINT "CK_Royalties_Percentual_Valido" CHECK (PercentualAplicado >= 0 AND PercentualAplicado <= 100),
    CONSTRAINT "FK_Royalties_Unidades_UnidadeId" FOREIGN KEY ("UnidadeId") REFERENCES "Unidades" ("UnidadeId") ON DELETE RESTRICT
);

CREATE TABLE "Usuarios" (
    "UsuarioId" INTEGER NOT NULL CONSTRAINT "PK_Usuarios" PRIMARY KEY AUTOINCREMENT,
    "Nome" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "SenhaHash" TEXT NOT NULL,
    "PerfilId" INTEGER NOT NULL,
    "UnidadeId" INTEGER NULL,
    "Ativo" INTEGER NOT NULL,
    "DataCadastro" TEXT NOT NULL,
    CONSTRAINT "FK_Usuarios_Perfis_PerfilId" FOREIGN KEY ("PerfilId") REFERENCES "Perfis" ("PerfilId") ON DELETE RESTRICT,
    CONSTRAINT "FK_Usuarios_Unidades_UnidadeId" FOREIGN KEY ("UnidadeId") REFERENCES "Unidades" ("UnidadeId") ON DELETE RESTRICT
);

CREATE TABLE "Chamados" (
    "ChamadoSuporteId" INTEGER NOT NULL CONSTRAINT "PK_Chamados" PRIMARY KEY AUTOINCREMENT,
    "UnidadeId" INTEGER NOT NULL,
    "UsuarioAberturaId" INTEGER NOT NULL,
    "Protocolo" TEXT NOT NULL,
    "Titulo" TEXT NOT NULL,
    "Descricao" TEXT NOT NULL,
    "Categoria" TEXT NOT NULL,
    "Prioridade" TEXT NOT NULL,
    "Situacao" TEXT NOT NULL,
    "DataAbertura" TEXT NOT NULL,
    "DataEncerramento" TEXT NULL,
    "SolucaoAplicada" TEXT NULL,
    CONSTRAINT "FK_Chamados_Unidades_UnidadeId" FOREIGN KEY ("UnidadeId") REFERENCES "Unidades" ("UnidadeId") ON DELETE RESTRICT,
    CONSTRAINT "FK_Chamados_Usuarios_UsuarioAberturaId" FOREIGN KEY ("UsuarioAberturaId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE RESTRICT
);

CREATE TABLE "Vendas" (
    "VendaId" INTEGER NOT NULL CONSTRAINT "PK_Vendas" PRIMARY KEY AUTOINCREMENT,
    "UnidadeId" INTEGER NOT NULL,
    "UsuarioId" INTEGER NOT NULL,
    "NumeroVenda" TEXT NOT NULL,
    "DataVenda" TEXT NOT NULL,
    "FormaPagamento" TEXT NOT NULL,
    "Situacao" TEXT NOT NULL,
    "ValorTotal" decimal(18,2) NOT NULL,
    CONSTRAINT "FK_Vendas_Unidades_UnidadeId" FOREIGN KEY ("UnidadeId") REFERENCES "Unidades" ("UnidadeId") ON DELETE RESTRICT,
    CONSTRAINT "FK_Vendas_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE RESTRICT
);

CREATE TABLE "InteracoesChamado" (
    "InteracaoChamadoId" INTEGER NOT NULL CONSTRAINT "PK_InteracoesChamado" PRIMARY KEY AUTOINCREMENT,
    "ChamadoSuporteId" INTEGER NOT NULL,
    "UsuarioId" INTEGER NOT NULL,
    "Mensagem" TEXT NOT NULL,
    "SituacaoAnterior" TEXT NOT NULL,
    "SituacaoAtual" TEXT NOT NULL,
    "DataRegistro" TEXT NOT NULL,
    CONSTRAINT "FK_InteracoesChamado_Chamados_ChamadoSuporteId" FOREIGN KEY ("ChamadoSuporteId") REFERENCES "Chamados" ("ChamadoSuporteId") ON DELETE CASCADE,
    CONSTRAINT "FK_InteracoesChamado_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE RESTRICT
);

CREATE TABLE "ItensVenda" (
    "ItemVendaId" INTEGER NOT NULL CONSTRAINT "PK_ItensVenda" PRIMARY KEY AUTOINCREMENT,
    "VendaId" INTEGER NOT NULL,
    "ProdutoServicoId" INTEGER NOT NULL,
    "Quantidade" INTEGER NOT NULL,
    "PrecoUnitario" decimal(18,2) NOT NULL,
    "ValorTotal" decimal(18,2) NOT NULL,
    CONSTRAINT "CK_ItensVenda_Quantidade_Positiva" CHECK (Quantidade > 0),
    CONSTRAINT "FK_ItensVenda_ProdutosServicos_ProdutoServicoId" FOREIGN KEY ("ProdutoServicoId") REFERENCES "ProdutosServicos" ("ProdutoServicoId") ON DELETE RESTRICT,
    CONSTRAINT "FK_ItensVenda_Vendas_VendaId" FOREIGN KEY ("VendaId") REFERENCES "Vendas" ("VendaId") ON DELETE CASCADE
);

CREATE TABLE "MovimentacoesEstoque" (
    "MovimentacaoEstoqueId" INTEGER NOT NULL CONSTRAINT "PK_MovimentacoesEstoque" PRIMARY KEY AUTOINCREMENT,
    "EstoqueId" INTEGER NOT NULL,
    "Tipo" TEXT NOT NULL,
    "Quantidade" INTEGER NOT NULL,
    "QuantidadeAnterior" INTEGER NOT NULL,
    "QuantidadeResultante" INTEGER NOT NULL,
    "Motivo" TEXT NOT NULL,
    "UsuarioId" INTEGER NULL,
    "VendaId" INTEGER NULL,
    "DataMovimentacao" TEXT NOT NULL,
    CONSTRAINT "FK_MovimentacoesEstoque_Estoques_EstoqueId" FOREIGN KEY ("EstoqueId") REFERENCES "Estoques" ("EstoqueId") ON DELETE CASCADE,
    CONSTRAINT "FK_MovimentacoesEstoque_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE RESTRICT,
    CONSTRAINT "FK_MovimentacoesEstoque_Vendas_VendaId" FOREIGN KEY ("VendaId") REFERENCES "Vendas" ("VendaId") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_Categorias_Nome_Unico" ON "Categorias" ("Nome");

CREATE UNIQUE INDEX "IX_Chamados_Protocolo_Unico" ON "Chamados" ("Protocolo");

CREATE INDEX "IX_Chamados_Situacao_Prioridade" ON "Chamados" ("Situacao", "Prioridade");

CREATE INDEX "IX_Chamados_UnidadeId" ON "Chamados" ("UnidadeId");

CREATE INDEX "IX_Chamados_UsuarioAberturaId" ON "Chamados" ("UsuarioAberturaId");

CREATE INDEX "IX_Estoques_ProdutoServicoId" ON "Estoques" ("ProdutoServicoId");

CREATE UNIQUE INDEX "IX_Estoques_Unidade_Produto_Unico" ON "Estoques" ("UnidadeId", "ProdutoServicoId");

CREATE UNIQUE INDEX "IX_Fornecedores_Cnpj_Unico" ON "Fornecedores" ("Cnpj");

CREATE INDEX "IX_Fornecedores_NomeFantasia" ON "Fornecedores" ("NomeFantasia");

CREATE INDEX "IX_FornecedoresProdutos_ProdutoServicoId" ON "FornecedoresProdutos" ("ProdutoServicoId");

CREATE UNIQUE INDEX "IX_FornecedoresProdutos_Vinculo_Unico" ON "FornecedoresProdutos" ("FornecedorId", "ProdutoServicoId");

CREATE UNIQUE INDEX "IX_Franqueadoras_Cnpj_Unico" ON "Franqueadoras" ("Cnpj");

CREATE UNIQUE INDEX "IX_Franqueados_Cpf_Unico" ON "Franqueados" ("Cpf");

CREATE INDEX "IX_InteracoesChamado_ChamadoSuporteId" ON "InteracoesChamado" ("ChamadoSuporteId");

CREATE INDEX "IX_InteracoesChamado_UsuarioId" ON "InteracoesChamado" ("UsuarioId");

CREATE INDEX "IX_ItensVenda_ProdutoServicoId" ON "ItensVenda" ("ProdutoServicoId");

CREATE INDEX "IX_ItensVenda_VendaId" ON "ItensVenda" ("VendaId");

CREATE INDEX "IX_MovimentacoesEstoque_EstoqueId" ON "MovimentacoesEstoque" ("EstoqueId");

CREATE INDEX "IX_MovimentacoesEstoque_UsuarioId" ON "MovimentacoesEstoque" ("UsuarioId");

CREATE INDEX "IX_MovimentacoesEstoque_VendaId" ON "MovimentacoesEstoque" ("VendaId");

CREATE UNIQUE INDEX "IX_Perfis_Tipo" ON "Perfis" ("Tipo");

CREATE INDEX "IX_ProdutosServicos_CategoriaId" ON "ProdutosServicos" ("CategoriaId");

CREATE UNIQUE INDEX "IX_ProdutosServicos_Codigo_Unico" ON "ProdutosServicos" ("Codigo");

CREATE INDEX "IX_ProdutosServicos_Nome" ON "ProdutosServicos" ("Nome");

CREATE UNIQUE INDEX "IX_Responsaveis_Unidade_Cpf_Unico" ON "Responsaveis" ("UnidadeId", "Cpf");

CREATE UNIQUE INDEX "IX_Royalties_Unidade_Competencia_Unico" ON "Royalties" ("UnidadeId", "CompetenciaAno", "CompetenciaMes");

CREATE INDEX "IX_Unidades_Cidade" ON "Unidades" ("Cidade");

CREATE UNIQUE INDEX "IX_Unidades_Cnpj_Unico" ON "Unidades" ("Cnpj");

CREATE UNIQUE INDEX "IX_Unidades_Codigo_Unico" ON "Unidades" ("Codigo");

CREATE INDEX "IX_Unidades_FranqueadoId" ON "Unidades" ("FranqueadoId");

CREATE INDEX "IX_Unidades_FranqueadoraId" ON "Unidades" ("FranqueadoraId");

CREATE UNIQUE INDEX "IX_Usuarios_Email_Unico" ON "Usuarios" ("Email");

CREATE INDEX "IX_Usuarios_PerfilId" ON "Usuarios" ("PerfilId");

CREATE INDEX "IX_Usuarios_UnidadeId" ON "Usuarios" ("UnidadeId");

CREATE UNIQUE INDEX "IX_Vendas_Unidade_Numero_Unico" ON "Vendas" ("UnidadeId", "NumeroVenda");

CREATE INDEX "IX_Vendas_UnidadeId_DataVenda" ON "Vendas" ("UnidadeId", "DataVenda");

CREATE INDEX "IX_Vendas_UsuarioId" ON "Vendas" ("UsuarioId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260909160201_CriacaoInicial', '10.0.11');

COMMIT;

