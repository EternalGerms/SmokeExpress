// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.ComponentModel.DataAnnotations;

namespace SmokeExpress.Web.Models;

/// <summary>
/// Pedido realizado pelos clientes na plataforma Smoke Express.
/// </summary>
public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser Cliente { get; set; } = null!;

    public DateTime DataPedido { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Processando";

    [Required]
    [StringLength(500)]
    public string EnderecoEntrega { get; set; } = string.Empty;

    [Range(0, 1000000)]
    public decimal TotalPedido { get; set; }
        = 0m;

    public ICollection<OrderItem> Itens { get; set; } = new List<OrderItem>();
}

