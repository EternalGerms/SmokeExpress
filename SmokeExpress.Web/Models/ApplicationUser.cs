// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Identity;

namespace SmokeExpress.Web.Models;

/// <summary>
/// Representa o usuário autenticado do sistema, estendendo o IdentityUser para aceitar os campos personalizados exigidos.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;

    public string Rua { get; set; } = string.Empty;

    public string? Numero { get; set; }

    public string Cidade { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    public DateTime DataNascimento { get; set; }
        = DateTime.UtcNow.AddYears(-18); // Garantia mínima para inicialização

    /// <summary>
    /// CPF (11 dígitos) ou CNPJ (14 dígitos) sem máscara.
    /// </summary>
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



