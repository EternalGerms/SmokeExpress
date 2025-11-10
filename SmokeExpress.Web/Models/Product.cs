// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.ComponentModel.DataAnnotations;
using SmokeExpress.Web.Constants;

namespace SmokeExpress.Web.Models;

/// <summary>
/// Modelo de domínio para os produtos ofertados na Smoke Express.
/// </summary>
public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(ApplicationConstants.MaxProductNameLength)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(ApplicationConstants.MaxProductDescriptionLength)]
    public string? Descricao { get; set; }
        = "";

    [Range(0, 100000)]
    public decimal Preco { get; set; }
        = 0m;

    [Range(0, int.MaxValue)]
    public int Estoque { get; set; }
        = 0;

    [StringLength(500)]
    public string? ImagemUrl { get; set; }
        = "";

    [Required]
    public int CategoriaId { get; set; }
        = 0;

    public Category Categoria { get; set; } = null!;

    public ICollection<OrderItem> ItensPedido { get; set; } = new List<OrderItem>();
}



