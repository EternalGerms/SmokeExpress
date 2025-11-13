// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Identity;
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

    public DbSet<Referral> Referrals => Set<Referral>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<ProductReview> Reviews => Set<ProductReview>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.NomeCompleto)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(u => u.Rua)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(u => u.Numero)
                .HasMaxLength(20);

            entity.Property(u => u.Cidade)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.Bairro)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.Complemento)
                .HasMaxLength(200);

            entity.Property(u => u.DataNascimento)
                .HasColumnType("date");

            entity.Property(u => u.DocumentoFiscal)
                .HasMaxLength(14)
                .IsRequired();

            entity.Property(u => u.TipoCliente)
                .HasMaxLength(2)
                .IsRequired();

            entity.Property(u => u.Genero)
                .HasMaxLength(30);

            entity.Property(u => u.ConsentiuMarketing)
                .HasDefaultValue(false);

            entity.Property(u => u.TermosAceitosEm)
                .HasColumnType("datetime2");

            entity.HasIndex(u => u.DocumentoFiscal)
                .IsUnique();
        });

        builder.Entity<Address>(entity =>
        {
            entity.Property(a => a.Rua).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Numero).HasMaxLength(20);
            entity.Property(a => a.Cidade).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Bairro).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Complemento).HasMaxLength(200);

            entity.HasOne(a => a.User)
                .WithMany(u => u.Enderecos)
                .HasForeignKey(a => a.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.ApplicationUserId, a.IsDefault });
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
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(o => o.Rua)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(o => o.Numero)
                .HasMaxLength(20);

            entity.Property(o => o.Cidade)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.Bairro)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.Complemento)
                .HasMaxLength(200);

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

        builder.Entity<ProductReview>(entity =>
        {
            entity.Property(r => r.Comment)
                .HasMaxLength(1000);

            entity.HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ApplicationUser)
                .WithMany()
                .HasForeignKey(r => r.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Permite múltiplas avaliações do mesmo produto pelo mesmo usuário (em pedidos diferentes)
        });

        const string adminRoleId = "c1c4c1c5-d9c1-46d0-a242-6d4832f758fd";
        const string userRoleId = "5f0f43f2-6ae6-4b8e-93d8-acff54f9b89a";
        const string adminUserId = "a8c0d5f5-5c6b-4fba-8825-8dc28c166a51";

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "a79a2da7-61fc-402f-9c5a-7b2af970eada"
            },
            new IdentityRole
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "be3fda52-8c88-4f0a-9120-a317b9e9e3bb"
            });

        builder.Entity<ApplicationUser>().HasData(new ApplicationUser
        {
            Id = adminUserId,
            UserName = "admin@smokeexpress.com",
            NormalizedUserName = "ADMIN@SMOKEEXPRESS.COM",
            Email = "admin@smokeexpress.com",
            NormalizedEmail = "ADMIN@SMOKEEXPRESS.COM",
            EmailConfirmed = true,
            PasswordHash = "$2a$11$Xauk1xFDGHsoG1B3vBcsHOzklVT1Ahn8ULHMaJIerLR6j5KhBT.lG",
            SecurityStamp = "2c80a39d-7a62-4c1f-95a5-3f8a61d557f6",
            ConcurrencyStamp = "6ba93fc8-6631-4e0d-a37f-d3095fe3f81d",
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            NomeCompleto = "Administrador Smoke Express",
            Rua = "Avenida Central",
            Numero = "1000",
            Cidade = "São Paulo",
            Bairro = "Centro",
            DocumentoFiscal = "12345678901",
            TipoCliente = "PF",
            DataNascimento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TermosAceitosEm = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string> { RoleId = adminRoleId, UserId = adminUserId },
            new IdentityUserRole<string> { RoleId = userRoleId, UserId = adminUserId });
    }
}



