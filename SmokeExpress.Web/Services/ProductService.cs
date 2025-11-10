// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmokeExpress.Web.Common;
using SmokeExpress.Web.Constants;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Helpers;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação padrão do serviço de produtos para as operações administrativas.
/// </summary>
// Convenção de logging: mensagens com propriedades nomeadas {Prop}; usar BeginScope para contexto (ex.: {ProductId}, {CategoriaId}).
public class ProductService(ApplicationDbContext context, ILogger<ProductService> logger) : IProductService
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Product>> ListarAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Listando produtos.");
        return await _context.Products
            .Include(p => p.Categoria)
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedResult<Product>> ListarPaginadoAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Validar parâmetros
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = ApplicationConstants.AdminPageSize;

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

    /// <inheritdoc />
    public async Task<PagedResult<Product>> BuscarPaginadoAsync(string? termoBusca, int? categoriaId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Validar parâmetros
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = ApplicationConstants.DefaultPageSize; // 12 itens padrão para e-commerce (3x4 grid)

        var query = _context.Products
            .Include(p => p.Categoria)
            .AsNoTracking();

        // Aplicar filtro de busca por termo (Nome ou Descrição)
        query = AplicarFiltroBusca(query, termoBusca);

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

    /// <inheritdoc />
    public async Task<PagedResult<Product>> BuscarPaginadoAsync(ProductSearchFilters filters, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Validar parâmetros
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = ApplicationConstants.DefaultPageSize; // 12 itens padrão para e-commerce (3x4 grid)

        var query = _context.Products
            .Include(p => p.Categoria)
            .AsNoTracking();

        query = AplicarFiltros(query, filters);

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

    private IQueryable<Product> AplicarFiltros(IQueryable<Product> query, ProductSearchFilters filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.TermoBusca))
        {
            query = AplicarFiltroBusca(query, filters.TermoBusca);
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

        return query;
    }

    private static IQueryable<Product> AplicarFiltroBusca(IQueryable<Product> query, string? termoBusca)
    {
        if (string.IsNullOrWhiteSpace(termoBusca))
        {
            return query;
        }

        var termos = termoBusca.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.ToLower())
            .ToArray();

        if (termos.Length == 0)
        {
            return query;
        }

        return query.Where(p =>
            termos.All(termo =>
                p.Nome.ToLower().Contains(termo) ||
                (p.Descricao != null && p.Descricao.ToLower().Contains(termo))
            )
        );
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

    /// <inheritdoc />
    public async Task<Product?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Obtendo produto. ProductId: {ProductId}", id);
        return await _context.Products
            .Include(p => p.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Product> CriarAsync(Product product, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(product, nameof(product));
        using var _ = logger.BeginScope(new { ProductId = product.Id, CategoriaId = product.CategoriaId });
        var validacao = await ValidarProdutoAsync(product, cancellationToken);
        if (!validacao.IsSuccess)
        {
            throw new ValidationException(validacao.ErrorMessage!);
        }

        _context.Products.Add(product);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao salvar produto no banco de dados");
            throw new BusinessException("Erro ao criar produto. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar produto");
            throw;
        }

        logger.LogInformation("Produto criado. ProductId: {ProductId}, CategoriaId: {CategoriaId}, Nome: {Nome}, Preco: {Preco}",
            product.Id, product.CategoriaId, product.Nome, FormatHelper.FormatCurrency(product.Preco));
        return product;
    }

    /// <inheritdoc />
    public async Task AtualizarAsync(Product product, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(product, nameof(product));
        using var _ = logger.BeginScope(new { ProductId = product.Id, CategoriaId = product.CategoriaId });
        var validacao = await ValidarProdutoAsync(product, cancellationToken);
        if (!validacao.IsSuccess)
        {
            throw new ValidationException(validacao.ErrorMessage!);
        }

        var existente = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);
        
        if (existente == null)
        {
            throw new NotFoundException("Produto", product.Id);
        }

        existente.Nome = product.Nome;
        existente.Descricao = product.Descricao;
        existente.Preco = product.Preco;
        existente.Estoque = product.Estoque;
        existente.ImagemUrl = product.ImagemUrl;
        existente.CategoriaId = product.CategoriaId;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao atualizar produto {ProductId} no banco de dados", product.Id);
            throw new BusinessException("Erro ao atualizar produto. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar produto {ProductId}", product.Id);
            throw;
        }

        logger.LogInformation("Produto atualizado. ProductId: {ProductId}, CategoriaId: {CategoriaId}", product.Id, product.CategoriaId);
    }

    /// <inheritdoc />
    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        using var _ = logger.BeginScope(new { ProductId = id });
        var validacao = await ValidarRemocaoAsync(id, cancellationToken);
        if (!validacao.IsSuccess)
        {
            if (validacao.ErrorMessage!.Contains("não encontrado"))
            {
                throw new NotFoundException("Produto", id);
            }
            throw new ValidationException(validacao.ErrorMessage);
        }

        var existente = await _context.Products
            .Include(p => p.ItensPedido)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (existente == null)
        {
            // Não deveria acontecer pois já validamos, mas garantimos segurança
            throw new NotFoundException("Produto", id);
        }

        _context.Products.Remove(existente);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao remover produto {ProductId} do banco de dados", id);
            throw new BusinessException("Erro ao remover produto. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover produto {ProductId}", id);
            throw;
        }

        logger.LogInformation("Produto removido. ProductId: {ProductId}", id);
    }

    private async Task<Result> ValidarProdutoAsync(Product? product, CancellationToken cancellationToken)
    {
        if (product == null)
        {
            return Result.Failure("Produto não pode ser nulo.");
        }

        if (string.IsNullOrWhiteSpace(product.Nome))
        {
            return Result.Failure("O nome do produto é obrigatório.");
        }

        if (product.Preco < 0)
        {
            return Result.Failure("O preço do produto não pode ser negativo.");
        }

        if (product.Estoque < 0)
        {
            return Result.Failure("O estoque do produto não pode ser negativo.");
        }

        // Validar se a categoria existe
        if (product.CategoriaId > 0)
        {
            var categoriaExiste = await _context.Categories
                .AnyAsync(c => c.Id == product.CategoriaId, cancellationToken);
            
            if (!categoriaExiste)
            {
                return Result.Failure($"Categoria com ID {product.CategoriaId} não encontrada.");
            }
        }

        return Result.Success();
    }

    private async Task<Result> ValidarRemocaoAsync(int id, CancellationToken cancellationToken)
    {
        var existente = await _context.Products
            .Include(p => p.ItensPedido)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (existente == null)
        {
            return Result.Failure($"Produto com ID {id} não encontrado.");
        }

        if (existente.ItensPedido.Any())
        {
            return Result.Failure($"Não é possível excluir o produto '{existente.Nome}' pois ele possui pedidos associados.");
        }

        return Result.Success();
    }
}



