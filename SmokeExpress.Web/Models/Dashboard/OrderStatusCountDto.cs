// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Models.Dashboard;

/// <summary>
/// DTO para representar contagem de pedidos por status.
/// </summary>
public class OrderStatusCountDto
{
    public OrderStatus Status { get; set; }
    public string StatusTexto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal TotalReceita { get; set; }
}

