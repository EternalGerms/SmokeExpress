// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Models.ViewModels;

namespace SmokeExpress.Web.Extensions;

/// <summary>
/// Extension methods para mapeamento de entidades Product para DTOs.
/// </summary>
public static class ProductExtensions
{
    /// <summary>
    /// Converte um Product para ProductLiteDto.
    /// </summary>
    public static ProductLiteDto ToProductLiteDto(this Product product)
    {
        return new ProductLiteDto
        {
            Id = product.Id,
            Nome = product.Nome,
            Preco = product.Preco,
            ImagemUrl = product.ImagemUrl
        };
    }

    /// <summary>
    /// Converte um Product e quantidade para CartItemViewModel.
    /// </summary>
    public static CartItemViewModel ToCartItemViewModel(this Product product, int quantidade)
    {
        var subtotal = product.Preco * quantidade;
        return new CartItemViewModel
        {
            ProductId = product.Id,
            Nome = product.Nome,
            Quantidade = quantidade,
            PrecoUnitario = product.Preco,
            Subtotal = subtotal,
            ImagemUrl = product.ImagemUrl ?? string.Empty
        };
    }
}

