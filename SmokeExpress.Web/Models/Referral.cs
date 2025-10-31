// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.ComponentModel.DataAnnotations;

namespace SmokeExpress.Web.Models;

/// <summary>
/// Registro do programa "Indique um Amigo", essencial para o marketing de retenção orgânico.
/// </summary>
public class Referral
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ReferrerUserId { get; set; } = string.Empty;

    public ApplicationUser ReferrerUser { get; set; } = null!;

    [Required]
    public string ReferredUserId { get; set; } = string.Empty;

    public ApplicationUser ReferredUser { get; set; } = null!;

    public DateTime DataIndicacao { get; set; } = DateTime.UtcNow;

    public bool BonusConcedido { get; set; }
        = false;
}


