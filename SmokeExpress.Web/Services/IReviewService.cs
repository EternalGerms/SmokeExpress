// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Define operações de criação e consulta de avaliações de produtos realizadas pelos usuários.
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Cria uma nova avaliação para o produto informado.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="productId">Identificador do produto avaliado.</param>
    /// <param name="rating">Nota atribuída ao produto (0 a 5).</param>
    /// <param name="comment">Comentário opcional descrevendo a experiência.</param>
    /// <param name="orderId">Pedido relacionado à avaliação (opcional).</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Avaliação criada e persistida.</returns>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="userId"/> é vazio.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="productId"/> ou <paramref name="rating"/> são inválidos.</exception>
    /// <exception cref="ValidationException">Lançada quando as regras de negócio impedem a avaliação.</exception>
    /// <exception cref="NotFoundException">Lançada quando o produto ou pedido associado não é encontrado.</exception>
    /// <exception cref="BusinessException">Lançada quando ocorre erro ao salvar a avaliação.</exception>
    Task<ProductReview> CriarAvaliacaoAsync(string userId, int productId, int rating, string? comment, int? orderId = null, CancellationToken ct = default);
    
    /// <summary>
    /// Obtém todas as avaliações de um produto.
    /// </summary>
    /// <param name="productId">Identificador do produto.</param>
    /// <param name="apenasComComentario">
    /// Quando <c>true</c>, retorna apenas avaliações com comentário associado.
    /// </param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Lista de avaliações ordenadas por data mais recente.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="productId"/> é menor ou igual a zero.</exception>
    Task<IReadOnlyList<ProductReview>> ObterAvaliacoesPorProdutoAsync(int productId, bool apenasComComentario = false, CancellationToken ct = default);
    
    /// <summary>
    /// Calcula a média de avaliações do produto.
    /// </summary>
    /// <param name="productId">Identificador do produto.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Média das avaliações ou <c>null</c> quando não há registros.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="productId"/> é menor ou igual a zero.</exception>
    Task<decimal?> ObterMediaAvaliacoesAsync(int productId, CancellationToken ct = default);
    
    /// <summary>
    /// Obtém o total de avaliações do produto.
    /// </summary>
    /// <param name="productId">Identificador do produto.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Total de avaliações encontradas.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="productId"/> é menor ou igual a zero.</exception>
    Task<int> ObterTotalAvaliacoesAsync(int productId, CancellationToken ct = default);
    
    /// <summary>
    /// Obtém resumo de avaliações do produto, incluindo média e comentários.
    /// </summary>
    /// <param name="productId">Identificador do produto.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Tupla contendo média, total e avaliações com comentário.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="productId"/> é menor ou igual a zero.</exception>
    Task<(decimal? Media, int Total, IReadOnlyList<ProductReview> AvaliacoesComComentario)> ObterResumoAvaliacoesAsync(int productId, CancellationToken ct = default);
    
    /// <summary>
    /// Obtém médias de avaliações para uma coleção de produtos.
    /// </summary>
    /// <param name="productIds">Conjunto de identificadores de produtos.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Dicionário mapeando o ID do produto para a média calculada.</returns>
    /// <exception cref="ArgumentNullException">Lançada quando <paramref name="productIds"/> é nulo.</exception>
    Task<Dictionary<int, decimal?>> ObterMediasAvaliacoesPorProdutosAsync(IEnumerable<int> productIds, CancellationToken ct = default);
    
    /// <summary>
    /// Verifica se o usuário já avaliou o produto em algum momento.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="productId">Identificador do produto.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns><c>true</c> quando o usuário já avaliou o produto.</returns>
    Task<bool> UsuarioJaAvaliouAsync(string userId, int productId, CancellationToken ct = default);
    
    /// <summary>
    /// Verifica se o usuário já avaliou o produto no pedido informado.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="productId">Identificador do produto.</param>
    /// <param name="orderId">Identificador do pedido.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns><c>true</c> quando já existe avaliação para a combinação usuário/produto/pedido.</returns>
    Task<bool> UsuarioJaAvaliouNoPedidoAsync(string userId, int productId, int orderId, CancellationToken ct = default);
    
    /// <summary>
    /// Obtém a avaliação de um produto realizada pelo usuário, quando existente.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="productId">Identificador do produto.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Avaliação encontrada ou <c>null</c> quando o usuário ainda não avaliou o produto.</returns>
    Task<ProductReview?> ObterAvaliacaoPorUsuarioAsync(string userId, int productId, CancellationToken ct = default);
}

