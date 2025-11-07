// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SmokeExpress.Web.Models;

/// <summary>
/// Representa o usuário autenticado do sistema, estendendo o IdentityUser para aceitar os campos personalizados exigidos.
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "O nome completo é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome completo deve ter entre 3 e 100 caracteres.")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "A rua é obrigatória.")]
    [StringLength(100, ErrorMessage = "A rua deve ter no máximo 100 caracteres.")]
    public string Rua { get; set; } = string.Empty;

    public string? Numero { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "A cidade é obrigatória.")]
    [StringLength(50, ErrorMessage = "A cidade deve ter no máximo 50 caracteres.")]
    public string Cidade { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "O bairro é obrigatório.")]
    [StringLength(50, ErrorMessage = "O bairro deve ter no máximo 50 caracteres.")]
    public string Bairro { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
    public DateTime DataNascimento { get; set; }
        = DateTime.UtcNow.AddYears(-18); // Garantia mínima para inicialização

    /// <summary>
    /// CPF (11 dígitos) ou CNPJ (14 dígitos) sem máscara.
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "O CPF/CNPJ é obrigatório.")]
    public string DocumentoFiscal { get; set; } = string.Empty;

    /// <summary>
    /// Define se o cliente é pessoa física (PF) ou jurídica (PJ).
    /// </summary>
    public string TipoCliente { get; set; } = "PF";

    public string? Genero { get; set; }
        = null;

    public bool ConsentiuMarketing { get; set; }
        = false;

    public DateTime? TermosAceitosEm { get; set; }
        = null;

    public ICollection<Order> Pedidos { get; set; } = new List<Order>();

    public ICollection<Referral> IndicacoesRealizadas { get; set; } = new List<Referral>();

    public ICollection<Referral> IndicacoesRecebidas { get; set; } = new List<Referral>();

    public ICollection<Address> Enderecos { get; set; } = new List<Address>();
}



