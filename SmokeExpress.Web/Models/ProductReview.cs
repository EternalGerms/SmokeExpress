// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.ComponentModel.DataAnnotations;

namespace SmokeExpress.Web.Models;

/// <summary>
/// Avaliação de produto realizada por um usuário.
/// </summary>
public class ProductReview
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser ApplicationUser { get; set; } = null!;

    /// <summary>
    /// ID do pedido associado a esta avaliação. Permite múltiplas avaliações do mesmo produto em pedidos diferentes.
    /// </summary>
    public int? OrderId { get; set; }

    public Order? Order { get; set; }

    [Range(0, 5)]
    [Required]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }

    public DateTime DataAvaliacao { get; set; } = DateTime.UtcNow;
}

