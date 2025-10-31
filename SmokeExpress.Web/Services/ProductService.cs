// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação padrão do serviço de produtos para as operações administrativas.
/// </summary>
public class ProductService(ApplicationDbContext context) : IProductService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IReadOnlyCollection<Product>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Categoria)
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product> CriarAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task AtualizarAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        var existente = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Produto com Id {product.Id} não encontrado.");

        existente.Nome = product.Nome;
        existente.Descricao = product.Descricao;
        existente.Preco = product.Preco;
        existente.Estoque = product.Estoque;
        existente.ImagemUrl = product.ImagemUrl;
        existente.CategoriaId = product.CategoriaId;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var existente = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Produto com Id {id} não encontrado.");

        _context.Products.Remove(existente);
        await _context.SaveChangesAsync(cancellationToken);
    }
}


