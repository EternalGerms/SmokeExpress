// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.ComponentModel.DataAnnotations;

namespace SmokeExpress.Web.Models;

/// <summary>
/// Conteúdo editorial usado na estratégia de marketing orgânico da Smoke Express.
/// </summary>
public class BlogPost
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    public string Conteudo { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser Autor { get; set; } = null!;

    public DateTime DataPublicacao { get; set; } = DateTime.UtcNow;

    public bool Publicado { get; set; }
        = false;
}



