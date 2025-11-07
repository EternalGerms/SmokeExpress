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

    public async Task<PagedResult<Product>> BuscarPaginadoAsync(ProductSearchFilters filters, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Validar parâmetros
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 12; // 12 itens padrão para e-commerce (3x4 grid)

        var query = _context.Products
            .Include(p => p.Categoria)
            .AsNoTracking();

        // Aplicar filtro de busca por termo (Nome ou Descrição) - busca por múltiplos termos (AND)
        if (!string.IsNullOrWhiteSpace(filters.TermoBusca))
        {
            var termos = filters.TermoBusca.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ToLower())
                .ToArray();

            if (termos.Length > 0)
            {
                // Busca que exige que TODOS os termos estejam presentes (AND)
                query = query.Where(p =>
                    termos.All(termo =>
                        p.Nome.ToLower().Contains(termo) ||
                        (p.Descricao != null && p.Descricao.ToLower().Contains(termo))
                    )
                );
            }
        }

        // Aplicar filtro por categoria
        if (filters.CategoriaId.HasValue && filters.CategoriaId.Value > 0)
        {
            query = query.Where(p => p.CategoriaId == filters.CategoriaId.Value);
        }

        // Aplicar filtro de preço mínimo
        if (filters.PrecoMin.HasValue)
        {
            query = query.Where(p => p.Preco >= filters.PrecoMin.Value);
        }

        // Aplicar filtro de preço máximo
        if (filters.PrecoMax.HasValue)
        {
            query = query.Where(p => p.Preco <= filters.PrecoMax.Value);
        }

        // Aplicar filtro de estoque (apenas produtos com estoque disponível)
        if (filters.ApenasEmEstoque == true)
        {
            query = query.Where(p => p.Estoque > 0);
        }

        // Aplicar ordenação
        query = filters.Ordenacao switch
        {
            ProductSortOrder.PrecoAsc => query.OrderBy(p => p.Preco),
            ProductSortOrder.PrecoDesc => query.OrderByDescending(p => p.Preco),
            ProductSortOrder.Relevancia => AplicarOrdenacaoRelevancia(query, filters.TermoBusca),
            ProductSortOrder.MelhoresAvaliados => AplicarOrdenacaoPorAvaliacao(query),
            _ => query.OrderBy(p => p.Nome) // ProductSortOrder.Nome ou padrão
        };

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

    /// <summary>
    /// Aplica ordenação por relevância baseada na correspondência com o termo de busca.
    /// Produtos que mais correspondem ao termo aparecem primeiro.
    /// </summary>
    private static IQueryable<Product> AplicarOrdenacaoRelevancia(IQueryable<Product> query, string? termoBusca)
    {
        if (string.IsNullOrWhiteSpace(termoBusca))
        {
            // Se não há termo de busca, ordena por nome
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

        // Ordena por relevância: produtos com mais correspondências no nome primeiro,
        // depois por correspondências na descrição, e finalmente por nome
        return query.OrderByDescending(p =>
            termos.Count(termo => p.Nome.ToLower().Contains(termo)) * 2 + // Nome tem peso 2
            termos.Count(termo => p.Descricao != null && p.Descricao.ToLower().Contains(termo)) // Descrição tem peso 1
        ).ThenBy(p => p.Nome);
    }

    /// <summary>
    /// Aplica ordenação por melhor avaliação (maior média de avaliações primeiro).
    /// Produtos sem avaliações aparecem por último.
    /// </summary>
    private IQueryable<Product> AplicarOrdenacaoPorAvaliacao(IQueryable<Product> query)
    {
        var mediasQuery = _context.Reviews
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Media = g.Average(r => (decimal?)r.Rating)
            });

        return query
            .GroupJoin(
                mediasQuery,
                produto => produto.Id,
                media => media.ProductId,
                (produto, medias) => new
                {
                    Produto = produto,
                    MediaAvaliacao = medias.Select(m => m.Media).FirstOrDefault()
                })
            .OrderByDescending(x => x.MediaAvaliacao ?? -1)
            .ThenBy(x => x.Produto.Nome)
            .Select(x => x.Produto);
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
            .Include(p => p.ItensPedido)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Produto com Id {id} não encontrado.");

        // Verificar se o produto tem pedidos associados
        if (existente.ItensPedido.Any())
        {
            throw new InvalidOperationException($"Não é possível excluir o produto '{existente.Nome}' pois ele possui pedidos associados.");
        }

        _context.Products.Remove(existente);
        await _context.SaveChangesAsync(cancellationToken);
    }
}



