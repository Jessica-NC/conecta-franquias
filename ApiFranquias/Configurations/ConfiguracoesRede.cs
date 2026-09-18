using ConectaFranquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConectaFranquias.Api.Configurations;

public class FranqueadoraConfiguracao : IEntityTypeConfiguration<Franqueadora>
{
    public void Configure(EntityTypeBuilder<Franqueadora> construtor)
    {
        construtor.ToTable("Franqueadoras");
        construtor.HasKey(franqueadora => franqueadora.FranqueadoraId);

        construtor.Property(franqueadora => franqueadora.RazaoSocial).HasMaxLength(150).IsRequired();
        construtor.Property(franqueadora => franqueadora.NomeFantasia).HasMaxLength(120).IsRequired();
        construtor.Property(franqueadora => franqueadora.Cnpj).HasMaxLength(18).IsRequired();
        construtor.Property(franqueadora => franqueadora.Email).HasMaxLength(150).IsRequired();
        construtor.Property(franqueadora => franqueadora.Telefone).HasMaxLength(20).IsRequired();

        construtor.HasIndex(franqueadora => franqueadora.Cnpj)
            .IsUnique()
            .HasDatabaseName("IX_Franqueadoras_Cnpj_Unico");

        construtor.Metadata
            .FindNavigation(nameof(Franqueadora.Unidades))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class FranqueadoConfiguracao : IEntityTypeConfiguration<Franqueado>
{
    public void Configure(EntityTypeBuilder<Franqueado> construtor)
    {
        construtor.ToTable("Franqueados");
        construtor.HasKey(franqueado => franqueado.FranqueadoId);

        construtor.Property(franqueado => franqueado.Nome).HasMaxLength(120).IsRequired();
        construtor.Property(franqueado => franqueado.Cpf).HasMaxLength(14).IsRequired();
        construtor.Property(franqueado => franqueado.Email).HasMaxLength(150).IsRequired();
        construtor.Property(franqueado => franqueado.Telefone).HasMaxLength(20).IsRequired();
        construtor.Property(franqueado => franqueado.Cidade).HasMaxLength(80).IsRequired();
        construtor.Property(franqueado => franqueado.Estado).HasMaxLength(2).IsRequired();

        construtor.HasIndex(franqueado => franqueado.Cpf)
            .IsUnique()
            .HasDatabaseName("IX_Franqueados_Cpf_Unico");

        construtor.Metadata
            .FindNavigation(nameof(Franqueado.Unidades))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class UnidadeFranqueadaConfiguracao : IEntityTypeConfiguration<UnidadeFranqueada>
{
    public void Configure(EntityTypeBuilder<UnidadeFranqueada> construtor)
    {
        construtor.ToTable("Unidades");
        construtor.HasKey(unidade => unidade.UnidadeId);

        construtor.Property(unidade => unidade.Codigo).HasMaxLength(20).IsRequired();
        construtor.Property(unidade => unidade.NomeFantasia).HasMaxLength(120).IsRequired();
        construtor.Property(unidade => unidade.RazaoSocial).HasMaxLength(150).IsRequired();
        construtor.Property(unidade => unidade.Cnpj).HasMaxLength(18).IsRequired();
        construtor.Property(unidade => unidade.Email).HasMaxLength(150).IsRequired();
        construtor.Property(unidade => unidade.Telefone).HasMaxLength(20).IsRequired();
        construtor.Property(unidade => unidade.Situacao).HasConversion<string>().HasMaxLength(20).IsRequired();

        construtor.HasIndex(unidade => unidade.Cnpj)
            .IsUnique()
            .HasDatabaseName("IX_Unidades_Cnpj_Unico");

        construtor.HasIndex(unidade => unidade.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_Unidades_Codigo_Unico");

        construtor.OwnsOne(unidade => unidade.Endereco, endereco =>
        {
            endereco.Property(item => item.Logradouro).HasColumnName("Logradouro").HasMaxLength(150).IsRequired();
            endereco.Property(item => item.Numero).HasColumnName("Numero").HasMaxLength(15).IsRequired();
            endereco.Property(item => item.Bairro).HasColumnName("Bairro").HasMaxLength(80).IsRequired();
            endereco.Property(item => item.Cidade).HasColumnName("Cidade").HasMaxLength(80).IsRequired();
            endereco.Property(item => item.Estado).HasColumnName("Estado").HasMaxLength(2).IsRequired();
            endereco.Property(item => item.Cep).HasColumnName("Cep").HasMaxLength(9).IsRequired();

            endereco.HasIndex(item => item.Cidade);
        });

        construtor.Navigation(unidade => unidade.Endereco).IsRequired();

        construtor.HasOne(unidade => unidade.Franqueadora)
            .WithMany(franqueadora => franqueadora.Unidades)
            .HasForeignKey(unidade => unidade.FranqueadoraId)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne(unidade => unidade.Franqueado)
            .WithMany(franqueado => franqueado.Unidades)
            .HasForeignKey(unidade => unidade.FranqueadoId)
            .OnDelete(DeleteBehavior.Restrict);

        foreach (var navegacao in new[]
                 {
                     nameof(UnidadeFranqueada.Responsaveis),
                     nameof(UnidadeFranqueada.Estoques),
                     nameof(UnidadeFranqueada.Vendas),
                     nameof(UnidadeFranqueada.Royalties),
                     nameof(UnidadeFranqueada.Chamados)
                 })
        {
            construtor.Metadata.FindNavigation(navegacao)!.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}

public class ResponsavelConfiguracao : IEntityTypeConfiguration<Responsavel>
{
    public void Configure(EntityTypeBuilder<Responsavel> construtor)
    {
        construtor.ToTable("Responsaveis");
        construtor.HasKey(responsavel => responsavel.ResponsavelId);

        construtor.Property(responsavel => responsavel.Nome).HasMaxLength(120).IsRequired();
        construtor.Property(responsavel => responsavel.Cpf).HasMaxLength(14).IsRequired();
        construtor.Property(responsavel => responsavel.Cargo).HasMaxLength(60).IsRequired();
        construtor.Property(responsavel => responsavel.Email).HasMaxLength(150).IsRequired();
        construtor.Property(responsavel => responsavel.Telefone).HasMaxLength(20).IsRequired();

        construtor.HasIndex(responsavel => new { responsavel.UnidadeId, responsavel.Cpf })
            .IsUnique()
            .HasDatabaseName("IX_Responsaveis_Unidade_Cpf_Unico");

        construtor.HasOne(responsavel => responsavel.Unidade)
            .WithMany(unidade => unidade.Responsaveis)
            .HasForeignKey(responsavel => responsavel.UnidadeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
