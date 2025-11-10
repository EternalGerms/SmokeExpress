// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Models.ViewModels;

/// <summary>
/// DTO simplificado de produto para APIs e operações leves.
/// </summary>
public class ProductLiteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string? ImagemUrl { get; set; }
}

