using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação padrão do serviço de produtos para as operações administrativas.
/// </summary>
public class ProductService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IProductService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory = dbContextFactory;

    public async Task<IReadOnlyCollection<Product>> ListarAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Products
            .Include(p => p.Categoria)
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Product>> ListarPaginadoAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Products
            .Include(p => p.Categoria)
            .AsNoTracking()
            .OrderBy(p => p.Nome);

        var totalCount = await query.CountAsync(cancellationToken);

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
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 12;

        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Products
            .Include(p => p.Categoria)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(termoBusca))
        {
            var termo = termoBusca.Trim().ToLower();
            query = query.Where(p =>
                p.Nome.ToLower().Contains(termo) ||
                (p.Descricao != null && p.Descricao.ToLower().Contains(termo)));
        }

        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        query = query.OrderBy(p => p.Nome);

        var totalCount = await query.CountAsync(cancellationToken);

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

    public async Task<PagedResult<Product>> BuscarPaginadoAsync(ProductSearchFilters filters, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 12;

        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Products
            .Include(p => p.Categoria)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filters.TermoBusca))
        {
            var termos = filters.TermoBusca.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ToLower())
                .ToArray();

            if (termos.Length > 0)
            {
                query = query.Where(p =>
                    termos.All(termo =>
                        p.Nome.ToLower().Contains(termo) ||
                        (p.Descricao != null && p.Descricao.ToLower().Contains(termo))
                    ));
            }
        }

        if (filters.CategoriaId.HasValue && filters.CategoriaId.Value > 0)
        {
            query = query.Where(p => p.CategoriaId == filters.CategoriaId.Value);
        }

        if (filters.PrecoMin.HasValue)
        {
            query = query.Where(p => p.Preco >= filters.PrecoMin.Value);
        }

        if (filters.PrecoMax.HasValue)
        {
            query = query.Where(p => p.Preco <= filters.PrecoMax.Value);
        }

        if (filters.ApenasEmEstoque == true)
        {
            query = query.Where(p => p.Estoque > 0);
        }

        query = filters.Ordenacao switch
        {
            ProductSortOrder.PrecoAsc => query.OrderBy(p => p.Preco),
            ProductSortOrder.PrecoDesc => query.OrderByDescending(p => p.Preco),
            ProductSortOrder.Relevancia => AplicarOrdenacaoRelevancia(query, filters.TermoBusca),
            ProductSortOrder.MelhoresAvaliados => AplicarOrdenacaoPorAvaliacao(query, context),
            _ => query.OrderBy(p => p.Nome)
        };

        var totalCount = await query.CountAsync(cancellationToken);

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

    private static IQueryable<Product> AplicarOrdenacaoRelevancia(IQueryable<Product> query, string? termoBusca)
    {
        if (string.IsNullOrWhiteSpace(termoBusca))
        {
            return query.OrderBy(p => p.Nome);
        }

        var termos = termoBusca.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.ToLower())
            .ToArray();

        if (termos.Length == 0)
        {
            return query.OrderBy(p => p.Nome);
        }

        return query.OrderByDescending(p =>
            termos.Count(termo => p.Nome.ToLower().Contains(termo)) * 2 +
            termos.Count(termo => p.Descricao != null && p.Descricao.ToLower().Contains(termo))
        ).ThenBy(p => p.Nome);
    }

    private static IQueryable<Product> AplicarOrdenacaoPorAvaliacao(IQueryable<Product> query, ApplicationDbContext context)
    {
        var mediasQuery = context.Reviews
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Media = g.Average(r => r.Rating),
                TotalAvaliacoes = g.Count()
            });

        return query
            .GroupJoin(
                mediasQuery,
                produto => produto.Id,
                media => media.ProductId,
                (produto, medias) => new { produto, medias }
            )
            .SelectMany(
                x => x.medias.DefaultIfEmpty(),
                (x, media) => new
                {
                    Produto = x.produto,
                    Media = media != null ? media.Media : (double?)null,
                    TotalAvaliacoes = media != null ? media.TotalAvaliacoes : 0
                }
            )
            .OrderByDescending(x => x.Media.HasValue)
            .ThenByDescending(x => x.Media)
            .ThenByDescending(x => x.TotalAvaliacoes)
            .ThenBy(x => x.Produto.Nome)
            .Select(x => x.Produto);
    }

    public async Task<Product?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Products
            .Include(p => p.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product> CriarAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task AtualizarAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existente = await context.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Produto com Id {product.Id} não encontrado.");

        existente.Nome = product.Nome;
        existente.Descricao = product.Descricao;
        existente.Preco = product.Preco;
        existente.Estoque = product.Estoque;
        existente.CategoriaId = product.CategoriaId;
        existente.ImagemUrl = product.ImagemUrl;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existente = await context.Products
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Produto com Id {id} não encontrado.");

        context.Products.Remove(existente);
        await context.SaveChangesAsync(cancellationToken);
    }
}

