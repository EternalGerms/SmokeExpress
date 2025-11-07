// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Models.Dashboard;

/// <summary>
/// DTO para representar vendas de um produto.
/// </summary>
public class ProductSalesDto
{
    public int ProductId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int QuantidadeVendida { get; set; }
    public decimal Receita { get; set; }
    public string? ImagemUrl { get; set; }
}

