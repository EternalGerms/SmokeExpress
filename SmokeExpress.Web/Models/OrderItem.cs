// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.ComponentModel.DataAnnotations;

namespace SmokeExpress.Web.Models;

/// <summary>
/// Itens individuais que compõem o pedido do cliente.
/// </summary>
public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    [Required]
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Range(1, 10000)]
    public int Quantidade { get; set; }
        = 1;

    [Range(0, 100000)]
    public decimal PrecoUnitario { get; set; }
        = 0m;
}

