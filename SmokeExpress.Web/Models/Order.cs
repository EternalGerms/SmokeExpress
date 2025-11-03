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

    public string Rua { get; set; } = string.Empty;

    public string? Numero { get; set; }

    public string Cidade { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    [Range(0, 1000000)]
    public decimal TotalPedido { get; set; }
        = 0m;

    public ICollection<OrderItem> Itens { get; set; } = new List<OrderItem>();
}


