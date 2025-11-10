// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Constants;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Models.Dashboard;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Define operações de análise e consolidação de métricas de vendas.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Obtém indicadores agregados (receita, pedidos e clientes ativos) para o período informado.
    /// </summary>
    /// <param name="dataInicio">Data inicial do filtro (inclusiva). Quando nula, é calculada automaticamente.</param>
    /// <param name="dataFim">Data final do filtro (inclusiva). Quando nula, é calculada automaticamente.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Resumo consolidado de pedidos e produtos.</returns>
    Task<DashboardSummaryDto> ObterResumoAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
    
    /// <summary>
    /// Retorna a evolução de vendas ao longo do período escolhido.
    /// </summary>
    /// <param name="periodo">Intervalo pré-definido utilizado para agrupar os resultados.</param>
    /// <param name="dataInicio">Data inicial personalizada (apenas quando <paramref name="periodo"/> é Personalizado).</param>
    /// <param name="dataFim">Data final personalizada (apenas quando <paramref name="periodo"/> é Personalizado).</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Lista ordenada com receita, pedidos e itens vendidos por período.</returns>
    Task<IReadOnlyList<SalesByPeriodDto>> ObterVendasPorPeriodoAsync(PeriodFilter periodo, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
    
    /// <summary>
    /// Lista os produtos mais vendidos dentro do período informado.
    /// </summary>
    /// <param name="top">Quantidade máxima de produtos retornados.</param>
    /// <param name="dataInicio">Data inicial do filtro (opcional).</param>
    /// <param name="dataFim">Data final do filtro (opcional).</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Lista com produtos mais vendidos ordenados por quantidade.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="top"/> é menor ou igual a zero.</exception>
    Task<IReadOnlyList<ProductSalesDto>> ObterProdutosMaisVendidosAsync(int top = ApplicationConstants.DefaultTopItems, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
    
    /// <summary>
    /// Retorna os produtos com menor estoque considerando a quantidade solicitada.
    /// </summary>
    /// <param name="top">Quantidade máxima de produtos retornados.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Lista de produtos ordenada pelo estoque crescente.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="top"/> é menor ou igual a zero.</exception>
    Task<IReadOnlyList<Product>> ObterProdutosMenorEstoqueAsync(int top = ApplicationConstants.DefaultTopItems, CancellationToken ct = default);
    
    /// <summary>
    /// Retorna os produtos com melhor avaliação média.
    /// </summary>
    /// <param name="top">Quantidade máxima de produtos retornados.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Lista de produtos ordenada pela média de avaliações decrescente.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="top"/> é menor ou igual a zero.</exception>
    Task<IReadOnlyList<ProductRatingDto>> ObterProdutosMelhoresAvaliadosAsync(int top = ApplicationConstants.DefaultTopItems, CancellationToken ct = default);
    
    /// <summary>
    /// Retorna os produtos com pior avaliação média.
    /// </summary>
    /// <param name="top">Quantidade máxima de produtos retornados.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Lista de produtos ordenada pela média de avaliações crescente.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="top"/> é menor ou igual a zero.</exception>
    Task<IReadOnlyList<ProductRatingDto>> ObterProdutosPioresAvaliadosAsync(int top = ApplicationConstants.DefaultTopItems, CancellationToken ct = default);
    
    /// <summary>
    /// Agrupa pedidos do período por status e calcula quantidade e receita de cada categoria.
    /// </summary>
    /// <param name="dataInicio">Data inicial do filtro (opcional).</param>
    /// <param name="dataFim">Data final do filtro (opcional).</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Lista com contagem de pedidos por status.</returns>
    Task<IReadOnlyList<OrderStatusCountDto>> ObterPedidosPorStatusAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
    
    /// <summary>
    /// Obtém o conjunto completo de informações do dashboard para o período solicitado.
    /// </summary>
    /// <param name="periodo">Intervalo pré-definido utilizado para agrupar os resultados.</param>
    /// <param name="topProdutos">Quantidade máxima de produtos retornados nos rankings.</param>
    /// <param name="dataInicio">Data inicial personalizada (apenas quando <paramref name="periodo"/> é Personalizado).</param>
    /// <param name="dataFim">Data final personalizada (apenas quando <paramref name="periodo"/> é Personalizado).</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Estrutura consolidada com resumo, vendas, pedidos e rankings de produtos.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="topProdutos"/> é menor ou igual a zero.</exception>
    Task<DashboardAnalyticsDto> ObterAnalyticsCompletoAsync(PeriodFilter periodo, int topProdutos = ApplicationConstants.DefaultTopItems, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
}

