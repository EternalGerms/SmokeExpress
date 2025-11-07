// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Models.Dashboard;

/// <summary>
/// DTO consolidado com todas as estatísticas relacionadas a pedidos e vendas.
/// </summary>
public class DashboardAnalyticsDto
{
    public DashboardSummaryDto Resumo { get; set; } = null!;
    public IReadOnlyList<OrderStatusCountDto> PedidosPorStatus { get; set; } = new List<OrderStatusCountDto>();
    public IReadOnlyList<SalesByPeriodDto> VendasPorPeriodo { get; set; } = new List<SalesByPeriodDto>();
    public IReadOnlyList<ProductSalesDto> ProdutosMaisVendidos { get; set; } = new List<ProductSalesDto>();
}

