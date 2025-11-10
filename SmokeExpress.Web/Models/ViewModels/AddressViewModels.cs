// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.ComponentModel.DataAnnotations;

namespace SmokeExpress.Web.Models.ViewModels;

/// <summary>
/// DTO para transferência de dados de endereço.
/// </summary>
public class AddressDto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Rua é obrigatória")]
    public string Rua { get; set; } = string.Empty;
    
    public string? Numero { get; set; }
    
    [Required(ErrorMessage = "Bairro é obrigatório")]
    public string Bairro { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Cidade é obrigatória")]
    public string Cidade { get; set; } = string.Empty;
    
    public string? Complemento { get; set; }
    
    public bool IsDefault { get; set; }
}

