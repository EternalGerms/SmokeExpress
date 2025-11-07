// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Models.Dashboard;

/// <summary>
/// DTO para representar vendas agrupadas por período.
/// </summary>
public class SalesByPeriodDto
{
    public string Periodo { get; set; } = string.Empty;
    public decimal Receita { get; set; }
    public int QuantidadePedidos { get; set; }
    public int QuantidadeItensVendidos { get; set; }
}

