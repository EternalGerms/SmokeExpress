// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Identity;

namespace SmokeExpress.Web.Models;

/// <summary>
/// Representa o usuário autenticado do sistema, estendendo o IdentityUser para aceitar os campos personalizados exigidos.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;

    public string Endereco { get; set; } = string.Empty;

    public DateTime DataNascimento { get; set; }
        = DateTime.UtcNow.AddYears(-18); // Garantia mínima para inicialização

    public ICollection<Order> Pedidos { get; set; } = new List<Order>();

    public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();

    public ICollection<Referral> IndicacoesRealizadas { get; set; } = new List<Referral>();

    public ICollection<Referral> IndicacoesRecebidas { get; set; } = new List<Referral>();
}


