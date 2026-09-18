using ConectaFranquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConectaFranquias.Api.Configurations;

public class EstoqueConfiguracao : IEntityTypeConfiguration<Estoque>
{
    public void Configure(EntityTypeBuilder<Estoque> construtor)
    {
        construtor.ToTable("Estoques", tabela =>
            // Reforça no banco a regra de saldo não negativo garantida pela entidade.
            tabela.HasCheckConstraint("CK_Estoques_Quantidade_NaoNegativa", "Quantidade >= 0"));

        construtor.HasKey(estoque => estoque.EstoqueId);

        construtor.HasIndex(estoque => new { estoque.UnidadeId, estoque.ProdutoServicoId })
            .IsUnique()
            .HasDatabaseName("IX_Estoques_Unidade_Produto_Unico");

        construtor.HasOne(estoque => estoque.Unidade)
            .WithMany(unidade => unidade.Estoques)
            .HasForeignKey(estoque => estoque.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne(estoque => estoque.ProdutoServico)
            .WithMany(produto => produto.Estoques)
            .HasForeignKey(estoque => estoque.ProdutoServicoId)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.Metadata
            .FindNavigation(nameof(Estoque.Movimentacoes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class MovimentacaoEstoqueConfiguracao : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> construtor)
    {
        construtor.ToTable("MovimentacoesEstoque");
        construtor.HasKey(movimentacao => movimentacao.MovimentacaoEstoqueId);

        construtor.Property(movimentacao => movimentacao.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        construtor.Property(movimentacao => movimentacao.Motivo).HasMaxLength(200).IsRequired();

        construtor.HasOne(movimentacao => movimentacao.Estoque)
            .WithMany(estoque => estoque.Movimentacoes)
            .HasForeignKey(movimentacao => movimentacao.EstoqueId)
            .OnDelete(DeleteBehavior.Cascade);

        construtor.HasOne(movimentacao => movimentacao.Usuario)
            .WithMany()
            .HasForeignKey(movimentacao => movimentacao.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne(movimentacao => movimentacao.Venda)
            .WithMany()
            .HasForeignKey(movimentacao => movimentacao.VendaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VendaConfiguracao : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> construtor)
    {
        construtor.ToTable("Vendas");
        construtor.HasKey(venda => venda.VendaId);

        construtor.Property(venda => venda.NumeroVenda).HasMaxLength(30).IsRequired();
        construtor.Property(venda => venda.FormaPagamento).HasConversion<string>().HasMaxLength(20).IsRequired();
        construtor.Property(venda => venda.Situacao).HasConversion<string>().HasMaxLength(20).IsRequired();

        construtor.HasIndex(venda => new { venda.UnidadeId, venda.NumeroVenda })
            .IsUnique()
            .HasDatabaseName("IX_Vendas_Unidade_Numero_Unico");

        construtor.HasIndex(venda => new { venda.UnidadeId, venda.DataVenda });

        construtor.HasOne(venda => venda.Unidade)
            .WithMany(unidade => unidade.Vendas)
            .HasForeignKey(venda => venda.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne(venda => venda.Usuario)
            .WithMany()
            .HasForeignKey(venda => venda.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.Metadata
            .FindNavigation(nameof(Venda.Itens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ItemVendaConfiguracao : IEntityTypeConfiguration<ItemVenda>
{
    public void Configure(EntityTypeBuilder<ItemVenda> construtor)
    {
        construtor.ToTable("ItensVenda", tabela =>
            tabela.HasCheckConstraint("CK_ItensVenda_Quantidade_Positiva", "Quantidade > 0"));

        construtor.HasKey(item => item.ItemVendaId);

        construtor.HasOne(item => item.Venda)
            .WithMany(venda => venda.Itens)
            .HasForeignKey(item => item.VendaId)
            .OnDelete(DeleteBehavior.Cascade);

        construtor.HasOne(item => item.ProdutoServico)
            .WithMany()
            .HasForeignKey(item => item.ProdutoServicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RoyaltyConfiguracao : IEntityTypeConfiguration<Royalty>
{
    public void Configure(EntityTypeBuilder<Royalty> construtor)
    {
        construtor.ToTable("Royalties", tabela =>
            tabela.HasCheckConstraint(
                "CK_Royalties_Percentual_Valido",
                "PercentualAplicado >= 0 AND PercentualAplicado <= 100"));

        construtor.HasKey(royalty => royalty.RoyaltyId);

        construtor.Property(royalty => royalty.Situacao).HasConversion<string>().HasMaxLength(20).IsRequired();

        construtor.Ignore(royalty => royalty.Competencia);

        construtor.HasIndex(royalty => new { royalty.UnidadeId, royalty.CompetenciaAno, royalty.CompetenciaMes })
            .IsUnique()
            .HasDatabaseName("IX_Royalties_Unidade_Competencia_Unico");

        construtor.HasOne(royalty => royalty.Unidade)
            .WithMany(unidade => unidade.Royalties)
            .HasForeignKey(royalty => royalty.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChamadoSuporteConfiguracao : IEntityTypeConfiguration<ChamadoSuporte>
{
    public void Configure(EntityTypeBuilder<ChamadoSuporte> construtor)
    {
        construtor.ToTable("Chamados");
        construtor.HasKey(chamado => chamado.ChamadoSuporteId);

        construtor.Property(chamado => chamado.Protocolo).HasMaxLength(30).IsRequired();
        construtor.Property(chamado => chamado.Titulo).HasMaxLength(150).IsRequired();
        construtor.Property(chamado => chamado.Descricao).HasMaxLength(2000).IsRequired();
        construtor.Property(chamado => chamado.Categoria).HasConversion<string>().HasMaxLength(20).IsRequired();
        construtor.Property(chamado => chamado.Prioridade).HasConversion<string>().HasMaxLength(20).IsRequired();
        construtor.Property(chamado => chamado.Situacao).HasConversion<string>().HasMaxLength(20).IsRequired();
        construtor.Property(chamado => chamado.SolucaoAplicada).HasMaxLength(2000);

        construtor.Ignore(chamado => chamado.EstaEmAberto);

        construtor.HasIndex(chamado => chamado.Protocolo)
            .IsUnique()
            .HasDatabaseName("IX_Chamados_Protocolo_Unico");

        construtor.HasIndex(chamado => new { chamado.Situacao, chamado.Prioridade });

        construtor.HasOne(chamado => chamado.Unidade)
            .WithMany(unidade => unidade.Chamados)
            .HasForeignKey(chamado => chamado.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne(chamado => chamado.UsuarioAbertura)
            .WithMany()
            .HasForeignKey(chamado => chamado.UsuarioAberturaId)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.Metadata
            .FindNavigation(nameof(ChamadoSuporte.Interacoes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class InteracaoChamadoConfiguracao : IEntityTypeConfiguration<InteracaoChamado>
{
    public void Configure(EntityTypeBuilder<InteracaoChamado> construtor)
    {
        construtor.ToTable("InteracoesChamado");
        construtor.HasKey(interacao => interacao.InteracaoChamadoId);

        construtor.Property(interacao => interacao.Mensagem).HasMaxLength(2000).IsRequired();
        construtor.Property(interacao => interacao.SituacaoAnterior).HasConversion<string>().HasMaxLength(20).IsRequired();
        construtor.Property(interacao => interacao.SituacaoAtual).HasConversion<string>().HasMaxLength(20).IsRequired();

        construtor.HasOne(interacao => interacao.Chamado)
            .WithMany(chamado => chamado.Interacoes)
            .HasForeignKey(interacao => interacao.ChamadoSuporteId)
            .OnDelete(DeleteBehavior.Cascade);

        construtor.HasOne(interacao => interacao.Usuario)
            .WithMany()
            .HasForeignKey(interacao => interacao.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
