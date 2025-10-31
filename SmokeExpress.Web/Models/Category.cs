// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.ComponentModel.DataAnnotations;

namespace SmokeExpress.Web.Models;

/// <summary>
/// Categoria dos produtos, usada para organizar a vitrine e estratégias de conteúdo.
/// </summary>
public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }
        = "";

    public ICollection<Product> Produtos { get; set; } = new List<Product>();
}

