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

    public async Task<PagedResult<Product>> ListarPaginadoAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Validar parâmetros
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.Products
            .Include(p => p.Categoria)
            .AsNoTracking()
            .OrderBy(p => p.Nome);

        // Obter total de registros
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginação
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Product>> BuscarPaginadoAsync(string? termoBusca, int? categoriaId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Validar parâmetros
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 12; // 12 itens padrão para e-commerce (3x4 grid)

        var query = _context.Products
            .Include(p => p.Categoria)
            .AsNoTracking();

        // Aplicar filtro de busca por termo (Nome ou Descrição)
        if (!string.IsNullOrWhiteSpace(termoBusca))
        {
            var termo = termoBusca.Trim().ToLower();
            query = query.Where(p =>
                p.Nome.ToLower().Contains(termo) ||
                (p.Descricao != null && p.Descricao.ToLower().Contains(termo)));
        }

        // Aplicar filtro por categoria
        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        // Ordenar por nome
        query = query.OrderBy(p => p.Nome);

        // Obter total de registros
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginação
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
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


