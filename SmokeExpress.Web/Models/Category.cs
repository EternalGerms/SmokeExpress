// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.ComponentModel.DataAnnotations;
using SmokeExpress.Web.Constants;

namespace SmokeExpress.Web.Models;

/// <summary>
/// Categoria dos produtos, usada para organizar a vitrine e estratégias de conteúdo.
/// </summary>
public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(ApplicationConstants.MaxCategoryNameLength)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(ApplicationConstants.MaxCategoryDescriptionLength)]
    public string? Descricao { get; set; }
        = "";

    public ICollection<Product> Produtos { get; set; } = new List<Product>();
}



