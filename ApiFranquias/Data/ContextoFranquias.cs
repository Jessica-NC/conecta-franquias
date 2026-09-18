using ConectaFranquias.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConectaFranquias.Api.Data;

public class ContextoFranquias(DbContextOptions<ContextoFranquias> opcoes) : DbContext(opcoes)
{
    public DbSet<Perfil> Perfis => Set<Perfil>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Franqueadora> Franqueadoras => Set<Franqueadora>();

    public DbSet<Franqueado> Franqueados => Set<Franqueado>();

    public DbSet<UnidadeFranqueada> Unidades => Set<UnidadeFranqueada>();

    public DbSet<Responsavel> Responsaveis => Set<Responsavel>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<ProdutoServico> ProdutosServicos => Set<ProdutoServico>();

    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();

    public DbSet<FornecedorProduto> FornecedoresProdutos => Set<FornecedorProduto>();

    public DbSet<Estoque> Estoques => Set<Estoque>();

    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();

    public DbSet<Venda> Vendas => Set<Venda>();

    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

    public DbSet<Royalty> Royalties => Set<Royalty>();

    public DbSet<ChamadoSuporte> Chamados => Set<ChamadoSuporte>();

    public DbSet<InteracaoChamado> InteracoesChamado => Set<InteracaoChamado>();

    protected override void OnModelCreating(ModelBuilder construtor)
    {
        base.OnModelCreating(construtor);

        construtor.ApplyConfigurationsFromAssembly(typeof(ContextoFranquias).Assembly);

        // O SQLite não possui tipo decimal nativo. Fixar a precisão evita que
        // valores financeiros sejam gravados como ponto flutuante e percam centavos.
        foreach (var propriedade in construtor.Model
                     .GetEntityTypes()
                     .SelectMany(tipo => tipo.GetProperties())
                     .Where(propriedade => propriedade.ClrType == typeof(decimal) ||
                                           propriedade.ClrType == typeof(decimal?)))
        {
            propriedade.SetColumnType("decimal(18,2)");
        }
    }
}
