// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Models.Dashboard;

/// <summary>
/// DTO para representar avaliações de um produto.
/// </summary>
public class ProductRatingDto
{
    public int ProductId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal? MediaAvaliacao { get; set; }
    public int TotalAvaliacoes { get; set; }
    public string? ImagemUrl { get; set; }
}

