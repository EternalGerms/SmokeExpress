// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.ComponentModel.DataAnnotations;

namespace SmokeExpress.Web.Models;

public class EnderecoEntregaDto
{
    [Required]
    public string Rua { get; set; } = string.Empty;

    public string? Numero { get; set; }
        = null;

    [Required]
    public string Cidade { get; set; } = string.Empty;

    [Required]
    public string Bairro { get; set; } = string.Empty;

    public string? Complemento { get; set; }
        = null;
}

public class CheckoutRequest
{
    [Required]
    public List<CartItemDto> Itens { get; set; } = new();

    [Required]
    public EnderecoEntregaDto Endereco { get; set; } = new();
}


