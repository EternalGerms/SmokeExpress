// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

public interface IReviewService
{
    Task<ProductReview> CriarAvaliacaoAsync(string userId, int productId, int rating, string? comment, int? orderId = null, CancellationToken ct = default);
    
    Task<IReadOnlyList<ProductReview>> ObterAvaliacoesPorProdutoAsync(int productId, bool apenasComComentario = false, CancellationToken ct = default);
    
    Task<decimal?> ObterMediaAvaliacoesAsync(int productId, CancellationToken ct = default);
    
    Task<int> ObterTotalAvaliacoesAsync(int productId, CancellationToken ct = default);
    
    Task<(decimal? Media, int Total, IReadOnlyList<ProductReview> AvaliacoesComComentario)> ObterResumoAvaliacoesAsync(int productId, CancellationToken ct = default);
    
    Task<Dictionary<int, decimal?>> ObterMediasAvaliacoesPorProdutosAsync(IEnumerable<int> productIds, CancellationToken ct = default);
    
    Task<bool> UsuarioJaAvaliouAsync(string userId, int productId, CancellationToken ct = default);
    
    Task<bool> UsuarioJaAvaliouNoPedidoAsync(string userId, int productId, int orderId, CancellationToken ct = default);
    
    Task<ProductReview?> ObterAvaliacaoPorUsuarioAsync(string userId, int productId, CancellationToken ct = default);
}

