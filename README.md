# Conecta Franquias — API REST

## Objetivo do projeto

API REST para gestão de uma rede de franquias, centralizando usuários e perfis
de acesso, franqueadora e unidades, catálogo de produtos e serviços, estoque,
vendas, taxas e royalties, fornecedores, chamados de suporte e relatórios
gerenciais.

## Tecnologias utilizadas

- .NET 10 / C#
- ASP.NET Core Web API
- Entity Framework Core 10
- SQLite
- JWT Bearer (autenticação)
- Swashbuckle (Swagger UI)

## Requisitos de execução

- Apenas o [.NET SDK 10.0](https://dotnet.microsoft.com/download) ou superior instalado
- Não é necessário instalar nem configurar nenhum servidor de banco de dados
- Não é necessário executar nenhum script de criação de banco: as migrations
  do Entity Framework Core rodam automaticamente no primeiro start e criam o
  arquivo SQLite (`App_Data/FranquiasDb.db`) já com todas as tabelas
- Não é necessário executar nenhum script de carga de dados: a própria
  aplicação popula o banco com uma carga de exemplo (unidades, produtos,
  vendas, royalties etc.) automaticamente na primeira execução

## Como executar

```bash
cd ApiFranquias
dotnet restore
dotnet run
```

Ao subir, a API fica disponível em `http://localhost:5142` e a documentação
interativa (Swagger) em:

```
http://localhost:5142/swagger
```

## Principais regras de negócio

- Uma unidade inativa não pode registrar novas vendas
- Uma venda deve ter ao menos um item e não pode repetir o mesmo produto/serviço
- O valor total da venda é sempre calculado a partir dos itens, nunca informado pelo cliente
- O estoque nunca pode ficar negativo; a baixa é feita automaticamente ao confirmar uma venda
- Não é permitido cadastrar duas unidades com o mesmo CNPJ, nem dois usuários com o mesmo e-mail
- O royalty de cada competência é calculado sobre o faturamento confirmado da unidade no período, somado à taxa fixa de franquia
- Cada perfil de acesso enxerga apenas o que lhe é permitido: Administrador vê a rede inteira; Gestor e Operador veem somente a própria unidade
