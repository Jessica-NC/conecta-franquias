using ConectaFranquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConectaFranquias.Api.Configurations;

public class PerfilConfiguracao : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> construtor)
    {
        construtor.ToTable("Perfis");
        construtor.HasKey(perfil => perfil.PerfilId);

        construtor.Property(perfil => perfil.Tipo).HasConversion<string>().HasMaxLength(30).IsRequired();
        construtor.Property(perfil => perfil.Nome).HasMaxLength(60).IsRequired();
        construtor.Property(perfil => perfil.Descricao).HasMaxLength(200).IsRequired();

        construtor.HasIndex(perfil => perfil.Tipo).IsUnique();

        construtor.Metadata
            .FindNavigation(nameof(Perfil.Usuarios))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class UsuarioConfiguracao : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> construtor)
    {
        construtor.ToTable("Usuarios");
        construtor.HasKey(usuario => usuario.UsuarioId);

        construtor.Property(usuario => usuario.Nome).HasMaxLength(120).IsRequired();
        construtor.Property(usuario => usuario.Email).HasMaxLength(150).IsRequired();
        construtor.Property(usuario => usuario.SenhaHash).HasMaxLength(300).IsRequired();

        construtor.HasIndex(usuario => usuario.Email)
            .IsUnique()
            .HasDatabaseName("IX_Usuarios_Email_Unico");

        construtor.HasOne(usuario => usuario.Perfil)
            .WithMany(perfil => perfil.Usuarios)
            .HasForeignKey(usuario => usuario.PerfilId)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne(usuario => usuario.Unidade)
            .WithMany()
            .HasForeignKey(usuario => usuario.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
