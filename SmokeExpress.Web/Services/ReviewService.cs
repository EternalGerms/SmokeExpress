// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

public class ReviewService(ApplicationDbContext dbContext, ILogger<ReviewService> logger) : IReviewService
{
    public async Task<ProductReview> CriarAvaliacaoAsync(string userId, int productId, int rating, string? comment, int? orderId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId não pode ser vazio.", nameof(userId));
        }

        if (rating < 0 || rating > 5)
        {
            throw new ArgumentException("Rating deve estar entre 0 e 5.", nameof(rating));
        }

        // Verificar se o produto existe
        var produtoExiste = await dbContext.Products.AnyAsync(p => p.Id == productId, ct);
        if (!produtoExiste)
        {
            throw new KeyNotFoundException($"Produto com ID {productId} não encontrado.");
        }

        // Se OrderId foi fornecido, verificar se o pedido existe
        if (orderId.HasValue)
        {
            var pedidoExiste = await dbContext.Orders.AnyAsync(o => o.Id == orderId.Value && o.ApplicationUserId == userId, ct);
            if (!pedidoExiste)
            {
                throw new KeyNotFoundException($"Pedido com ID {orderId.Value} não encontrado ou não pertence ao usuário.");
            }
        }

        // Criar nova avaliação (permite múltiplas avaliações do mesmo produto pelo mesmo usuário em pedidos diferentes)
        var avaliacao = new ProductReview
        {
            ProductId = productId,
            ApplicationUserId = userId,
            OrderId = orderId,
            Rating = rating,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment,
            DataAvaliacao = DateTime.UtcNow
        };

        dbContext.Reviews.Add(avaliacao);
        await dbContext.SaveChangesAsync(ct);
        
        logger.LogInformation("Nova avaliação criada para produto {ProductId} pelo usuário {UserId} no pedido {OrderId} com rating {Rating}", productId, userId, orderId, rating);
        return avaliacao;
    }

    public async Task<IReadOnlyList<ProductReview>> ObterAvaliacoesPorProdutoAsync(int productId, bool apenasComComentario = false, CancellationToken ct = default)
    {
        var query = dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.ApplicationUser)
            .Where(r => r.ProductId == productId);

        if (apenasComComentario)
        {
            query = query.Where(r => !string.IsNullOrWhiteSpace(r.Comment));
        }

        return await query
            .OrderByDescending(r => r.DataAvaliacao)
            .ToListAsync(ct);
    }

    public async Task<decimal?> ObterMediaAvaliacoesAsync(int productId, CancellationToken ct = default)
    {
        var media = await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .AverageAsync(r => (decimal?)r.Rating, ct);

        return media;
    }

    public async Task<int> ObterTotalAvaliacoesAsync(int productId, CancellationToken ct = default)
    {
        return await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .CountAsync(ct);
    }

    public async Task<(decimal? Media, int Total, IReadOnlyList<ProductReview> AvaliacoesComComentario)> ObterResumoAvaliacoesAsync(int productId, CancellationToken ct = default)
    {
        // Buscar todas as avaliações do produto uma única vez
        var todasAvaliacoes = await dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.ApplicationUser)
            .Where(r => r.ProductId == productId)
            .ToListAsync(ct);

        // Calcular média
        var media = todasAvaliacoes.Any() 
            ? (decimal?)todasAvaliacoes.Average(r => (decimal)r.Rating)
            : null;

        // Total de avaliações
        var total = todasAvaliacoes.Count;

        // Avaliações com comentário
        var avaliacoesComComentario = todasAvaliacoes
            .Where(r => !string.IsNullOrWhiteSpace(r.Comment))
            .OrderByDescending(r => r.DataAvaliacao)
            .ToList();

        return (media, total, avaliacoesComComentario);
    }

    public async Task<Dictionary<int, decimal?>> ObterMediasAvaliacoesPorProdutosAsync(IEnumerable<int> productIds, CancellationToken ct = default)
    {
        var idsList = productIds.ToList();
        if (idsList.Count == 0)
        {
            return new Dictionary<int, decimal?>();
        }

        var medias = await dbContext.Reviews
            .AsNoTracking()
            .Where(r => idsList.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Media = g.Average(r => (decimal?)r.Rating)
            })
            .ToListAsync(ct);

        var resultado = idsList.ToDictionary(id => id, id => (decimal?)null);
        foreach (var item in medias)
        {
            resultado[item.ProductId] = item.Media;
        }

        return resultado;
    }

    public async Task<bool> UsuarioJaAvaliouAsync(string userId, int productId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        return await dbContext.Reviews
            .AsNoTracking()
            .AnyAsync(r => r.ProductId == productId && r.ApplicationUserId == userId, ct);
    }

    public async Task<bool> UsuarioJaAvaliouNoPedidoAsync(string userId, int productId, int orderId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        return await dbContext.Reviews
            .AsNoTracking()
            .AnyAsync(r => r.ProductId == productId 
                        && r.ApplicationUserId == userId 
                        && r.OrderId == orderId, ct);
    }

    public async Task<ProductReview?> ObterAvaliacaoPorUsuarioAsync(string userId, int productId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await dbContext.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.ApplicationUserId == userId, ct);
    }
}

