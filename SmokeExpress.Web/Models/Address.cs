// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.ComponentModel.DataAnnotations;

namespace SmokeExpress.Web.Models;

public class Address
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Rua { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Numero { get; set; }
        = null;

    [Required]
    [MaxLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Bairro { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Complemento { get; set; }
        = null;

    public bool IsDefault { get; set; }
        = false;
}


