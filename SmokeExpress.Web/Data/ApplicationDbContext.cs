// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Data;

/// <summary>
/// Contexto principal do Entity Framework Core, responsável por mapear as entidades de domínio.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    public DbSet<Referral> Referrals => Set<Referral>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.NomeCompleto)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(u => u.Endereco)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(u => u.DataNascimento)
                .HasColumnType("date");
        });

        builder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Nome)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(c => c.Descricao)
                .HasMaxLength(500);

            entity.HasMany(c => c.Produtos)
                .WithOne(p => p.Categoria)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Nome)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(p => p.Descricao)
                .HasMaxLength(2000);

            entity.Property(p => p.Preco)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.ImagemUrl)
                .HasMaxLength(500);
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(o => o.EnderecoEntrega)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(o => o.TotalPedido)
                .HasColumnType("decimal(18,2)");

            entity.HasOne(o => o.Cliente)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(o => o.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.PrecoUnitario)
                .HasColumnType("decimal(18,2)");

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.Itens)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                .WithMany(p => p.ItensPedido)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BlogPost>(entity =>
        {
            entity.Property(bp => bp.Titulo)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(bp => bp.Slug)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(bp => bp.Slug)
                .IsUnique();

            entity.HasOne(bp => bp.Autor)
                .WithMany(u => u.Posts)
                .HasForeignKey(bp => bp.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Referral>(entity =>
        {
            entity.HasIndex(r => new { r.ReferrerUserId, r.ReferredUserId })
                .IsUnique();

            entity.HasOne(r => r.ReferrerUser)
                .WithMany(u => u.IndicacoesRealizadas)
                .HasForeignKey(r => r.ReferrerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReferredUser)
                .WithMany(u => u.IndicacoesRecebidas)
                .HasForeignKey(r => r.ReferredUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}


