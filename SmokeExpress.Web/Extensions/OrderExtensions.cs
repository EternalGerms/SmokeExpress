// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Models.ViewModels;

namespace SmokeExpress.Web.Extensions;

/// <summary>
/// Extension methods para mapeamento de entidades Order e OrderItem para DTOs.
/// </summary>
public static class OrderExtensions
{
    /// <summary>
    /// Converte um Order para OrderRowDto (versão Admin ou Account).
    /// </summary>
    public static OrderRowDto ToOrderRowDto(this Order order, bool includeClientInfo = false)
    {
        var dto = new OrderRowDto
        {
            Id = order.Id,
            DataLocal = order.DataPedido.ToLocalTime().ToString("g"),
            DataPedidoLocal = order.DataPedido.ToLocalTime().ToString("g"),
            TotalTexto = order.TotalPedido.ToString("C"),
            Status = order.Status
        };

        if (includeClientInfo)
        {
            dto.ClienteNome = order.Cliente?.NomeCompleto ?? string.Empty;
            dto.ClienteEmail = order.Cliente?.Email ?? string.Empty;
        }

        return dto;
    }

    /// <summary>
    /// Converte um Order para OrderDetailsDto (versão Admin ou Account).
    /// </summary>
    public static OrderDetailsDto ToOrderDetailsDto(this Order order, bool includeClientInfo = false)
    {
        var dto = new OrderDetailsDto
        {
            Id = order.Id,
            DataLocal = order.DataPedido.ToLocalTime().ToString("g"),
            Rua = order.Rua,
            Numero = order.Numero ?? string.Empty,
            Bairro = order.Bairro,
            Cidade = order.Cidade,
            Complemento = order.Complemento ?? string.Empty,
            TotalPedido = order.TotalPedido
        };

        if (includeClientInfo)
        {
            dto.ClienteNome = order.Cliente?.NomeCompleto ?? string.Empty;
            dto.ClienteEmail = order.Cliente?.Email ?? string.Empty;
        }

        // Mapear itens
        if (includeClientInfo)
        {
            dto.ItensAdmin = order.Itens.Select(i => i.ToOrderItemRowDto()).ToList();
        }
        else
        {
            dto.Itens = order.Itens.Select(i => i.ToOrderItemDetailDto()).ToList();
        }

        // Calcular subtotal e frete
        if (includeClientInfo)
        {
            dto.Subtotal = dto.ItensAdmin.Sum(i => i.PrecoUnitario * i.Quantidade);
        }
        else
        {
            dto.Subtotal = dto.Itens.Sum(i => i.Subtotal);
        }
        dto.Frete = dto.TotalPedido - dto.Subtotal;

        return dto;
    }

    /// <summary>
    /// Converte um Order para DTO anônimo para API.
    /// </summary>
    public static object ToOrderApiDto(this Order order)
    {
        return new
        {
            order.Id,
            order.DataPedido,
            order.Status,
            order.Rua,
            order.Numero,
            order.Cidade,
            order.Bairro,
            order.Complemento,
            order.TotalPedido,
            Itens = order.Itens.Select(i => new
            {
                i.ProductId,
                Nome = i.Product?.Nome ?? $"Produto {i.ProductId}",
                i.Quantidade,
                i.PrecoUnitario,
                Subtotal = i.PrecoUnitario * i.Quantidade,
                ImagemUrl = i.Product?.ImagemUrl
            }).ToList()
        };
    }

    /// <summary>
    /// Converte um OrderItem para OrderItemRowDto (Admin).
    /// </summary>
    public static OrderItemRowDto ToOrderItemRowDto(this OrderItem item)
    {
        return new OrderItemRowDto
        {
            Nome = item.Product?.Nome ?? $"Produto {item.ProductId}",
            Quantidade = item.Quantidade,
            PrecoUnitario = item.PrecoUnitario
        };
    }

    /// <summary>
    /// Converte um OrderItem para OrderItemDetailDto (Account).
    /// </summary>
    public static OrderItemDetailDto ToOrderItemDetailDto(this OrderItem item)
    {
        return new OrderItemDetailDto
        {
            Nome = item.Product?.Nome ?? $"Produto {item.ProductId}",
            Quantidade = item.Quantidade,
            PrecoUnitario = item.PrecoUnitario,
            Subtotal = item.PrecoUnitario * item.Quantidade,
            ImagemUrl = item.Product?.ImagemUrl,
            ProductId = item.ProductId
        };
    }
}

