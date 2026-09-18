using ConectaFranquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConectaFranquias.Api.Configurations;

public class CategoriaConfiguracao : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> construtor)
    {
        construtor.ToTable("Categorias");
        construtor.HasKey(categoria => categoria.CategoriaId);

        construtor.Property(categoria => categoria.Nome).HasMaxLength(80).IsRequired();
        construtor.Property(categoria => categoria.Descricao).HasMaxLength(200).IsRequired();

        construtor.HasIndex(categoria => categoria.Nome)
            .IsUnique()
            .HasDatabaseName("IX_Categorias_Nome_Unico");

        construtor.Metadata
            .FindNavigation(nameof(Categoria.Produtos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ProdutoServicoConfiguracao : IEntityTypeConfiguration<ProdutoServico>
{
    public void Configure(EntityTypeBuilder<ProdutoServico> construtor)
    {
        construtor.ToTable("ProdutosServicos");
        construtor.HasKey(produto => produto.ProdutoServicoId);

        construtor.Property(produto => produto.Codigo).HasMaxLength(20).IsRequired();
        construtor.Property(produto => produto.Nome).HasMaxLength(120).IsRequired();
        construtor.Property(produto => produto.Descricao).HasMaxLength(400).IsRequired();
        construtor.Property(produto => produto.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();

        construtor.HasIndex(produto => produto.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_ProdutosServicos_Codigo_Unico");

        construtor.HasIndex(produto => produto.Nome);

        construtor.HasOne(produto => produto.Categoria)
            .WithMany(categoria => categoria.Produtos)
            .HasForeignKey(produto => produto.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.Metadata
            .FindNavigation(nameof(ProdutoServico.Fornecedores))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        construtor.Metadata
            .FindNavigation(nameof(ProdutoServico.Estoques))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class FornecedorConfiguracao : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> construtor)
    {
        construtor.ToTable("Fornecedores");
        construtor.HasKey(fornecedor => fornecedor.FornecedorId);

        construtor.Property(fornecedor => fornecedor.RazaoSocial).HasMaxLength(150).IsRequired();
        construtor.Property(fornecedor => fornecedor.NomeFantasia).HasMaxLength(120).IsRequired();
        construtor.Property(fornecedor => fornecedor.Cnpj).HasMaxLength(18).IsRequired();
        construtor.Property(fornecedor => fornecedor.Email).HasMaxLength(150).IsRequired();
        construtor.Property(fornecedor => fornecedor.Telefone).HasMaxLength(20).IsRequired();
        construtor.Property(fornecedor => fornecedor.Cidade).HasMaxLength(80).IsRequired();
        construtor.Property(fornecedor => fornecedor.Estado).HasMaxLength(2).IsRequired();

        construtor.HasIndex(fornecedor => fornecedor.Cnpj)
            .IsUnique()
            .HasDatabaseName("IX_Fornecedores_Cnpj_Unico");

        construtor.HasIndex(fornecedor => fornecedor.NomeFantasia);

        construtor.Metadata
            .FindNavigation(nameof(Fornecedor.Produtos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class FornecedorProdutoConfiguracao : IEntityTypeConfiguration<FornecedorProduto>
{
    public void Configure(EntityTypeBuilder<FornecedorProduto> construtor)
    {
        construtor.ToTable("FornecedoresProdutos");
        construtor.HasKey(vinculo => vinculo.FornecedorProdutoId);

        construtor.HasIndex(vinculo => new { vinculo.FornecedorId, vinculo.ProdutoServicoId })
            .IsUnique()
            .HasDatabaseName("IX_FornecedoresProdutos_Vinculo_Unico");

        construtor.HasOne(vinculo => vinculo.Fornecedor)
            .WithMany(fornecedor => fornecedor.Produtos)
            .HasForeignKey(vinculo => vinculo.FornecedorId)
            .OnDelete(DeleteBehavior.Cascade);

        construtor.HasOne(vinculo => vinculo.ProdutoServico)
            .WithMany(produto => produto.Fornecedores)
            .HasForeignKey(vinculo => vinculo.ProdutoServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
