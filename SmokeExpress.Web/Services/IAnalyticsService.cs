// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Models.Dashboard;

namespace SmokeExpress.Web.Services;

public interface IAnalyticsService
{
    Task<DashboardSummaryDto> ObterResumoAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
    
    Task<IReadOnlyList<SalesByPeriodDto>> ObterVendasPorPeriodoAsync(PeriodFilter periodo, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
    
    Task<IReadOnlyList<ProductSalesDto>> ObterProdutosMaisVendidosAsync(int top = 10, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
    
    Task<IReadOnlyList<Product>> ObterProdutosMenorEstoqueAsync(int top = 10, CancellationToken ct = default);
    
    Task<IReadOnlyList<ProductRatingDto>> ObterProdutosMelhoresAvaliadosAsync(int top = 10, CancellationToken ct = default);
    
    Task<IReadOnlyList<ProductRatingDto>> ObterProdutosPioresAvaliadosAsync(int top = 10, CancellationToken ct = default);
    
    Task<IReadOnlyList<OrderStatusCountDto>> ObterPedidosPorStatusAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
}

