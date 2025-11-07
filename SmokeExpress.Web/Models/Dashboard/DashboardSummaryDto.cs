// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Models.Dashboard;

/// <summary>
/// DTO com resumo geral do dashboard.
/// </summary>
public class DashboardSummaryDto
{
    public decimal ReceitaTotal { get; set; }
    public int TotalPedidos { get; set; }
    public decimal MediaValorPedido { get; set; }
    public int TotalClientesAtivos { get; set; }
    public int TotalProdutos { get; set; }
    public int TotalProdutosSemEstoque { get; set; }
}

