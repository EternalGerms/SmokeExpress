// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmokeExpress.Web.Common;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Resources;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação de <see cref="IReviewService"/> que persiste avaliações de produtos no banco de dados.
/// </summary>
public class ReviewService(ApplicationDbContext dbContext, ILogger<ReviewService> logger) : IReviewService
{
    /// <inheritdoc />
    public async Task<ProductReview> CriarAvaliacaoAsync(string userId, int productId, int rating, string? comment, int? orderId = null, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(userId, nameof(userId));
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
        if (rating < 0 || rating > 5) throw new ArgumentOutOfRangeException(nameof(rating));
        var validacao = await ValidarAvaliacaoAsync(userId, productId, rating, orderId, ct);
        if (!validacao.IsSuccess)
        {
            if (validacao.ErrorMessage!.Contains("não encontrado") || validacao.ErrorMessage.Contains("não encontrada"))
            {
                if (validacao.ErrorMessage.Contains("Produto"))
                {
                    throw new NotFoundException("Produto", productId);
                }
                throw new NotFoundException(validacao.ErrorMessage);
            }
            throw new ValidationException(validacao.ErrorMessage);
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
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao salvar avaliação no banco de dados");
            throw new BusinessException(ErrorMessages.ErrorCreatingReview);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar avaliação");
            throw;
        }
        
        logger.LogInformation("Nova avaliação criada para produto {ProductId} pelo usuário {UserId} no pedido {OrderId} com rating {Rating}", productId, userId, orderId, rating);
        return avaliacao;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductReview>> ObterAvaliacoesPorProdutoAsync(int productId, bool apenasComComentario = false, CancellationToken ct = default)
    {
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
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

    /// <inheritdoc />
    public async Task<decimal?> ObterMediaAvaliacoesAsync(int productId, CancellationToken ct = default)
    {
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
        var media = await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .AverageAsync(r => (decimal?)r.Rating, ct);

        return media;
    }

    /// <inheritdoc />
    public async Task<int> ObterTotalAvaliacoesAsync(int productId, CancellationToken ct = default)
    {
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
        return await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .CountAsync(ct);
    }

    /// <inheritdoc />
    public async Task<(decimal? Media, int Total, IReadOnlyList<ProductReview> AvaliacoesComComentario)> ObterResumoAvaliacoesAsync(int productId, CancellationToken ct = default)
    {
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
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

    /// <inheritdoc />
    public async Task<Dictionary<int, decimal?>> ObterMediasAvaliacoesPorProdutosAsync(IEnumerable<int> productIds, CancellationToken ct = default)
    {
        Guard.AgainstNull(productIds, nameof(productIds));
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    private async Task<Result> ValidarAvaliacaoAsync(string userId, int productId, int rating, int? orderId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure(ErrorMessages.UserIdCannotBeEmpty);
        }

        if (rating < 0 || rating > 5)
        {
            return Result.Failure(ErrorMessages.InvalidRating);
        }

        // Verificar se o produto existe
        var produtoExiste = await dbContext.Products.AnyAsync(p => p.Id == productId, ct);
        if (!produtoExiste)
        {
            return Result.Failure(string.Format(ErrorMessages.ProductWithIdNotFoundForReview, productId));
        }

        // Se OrderId foi fornecido, verificar se o pedido existe
        if (orderId.HasValue)
        {
            var pedidoExiste = await dbContext.Orders.AnyAsync(o => o.Id == orderId.Value && o.ApplicationUserId == userId, ct);
            if (!pedidoExiste)
            {
                return Result.Failure(string.Format(ErrorMessages.OrderNotFoundOrNotOwned, orderId.Value));
            }
        }

        return Result.Success();
    }
}

