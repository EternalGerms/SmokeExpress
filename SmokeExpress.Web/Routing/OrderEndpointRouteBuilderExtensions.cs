// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Services;
using SmokeExpress.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace SmokeExpress.Web.Routing;

public static class OrderEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/orders");

        group.MapPost("/checkout", async (
            [FromBody] CheckoutRequest req,
            ClaimsPrincipal user,
            IOrderService orderService,
            CancellationToken ct) =>
            {
                if (req == null || req.Itens == null || req.Itens.Count == 0)
                {
                    return Results.BadRequest(new { message = "Carrinho vazio." });
                }

                // Validar endereço obrigatório
                if (req.Endereco == null)
                {
                    return Results.BadRequest(new { message = "Endereço de entrega é obrigatório." });
                }

                if (string.IsNullOrWhiteSpace(req.Endereco.Rua) ||
                    string.IsNullOrWhiteSpace(req.Endereco.Cidade) ||
                    string.IsNullOrWhiteSpace(req.Endereco.Bairro))
                {
                    return Results.BadRequest(new { message = "Por favor, selecione um endereço de entrega válido antes de finalizar o pedido." });
                }

                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Results.Unauthorized();
                }

                try
                {
                    var orderId = await orderService.CriarPedidoAsync(userId, req.Itens, req.Endereco, req.Frete, ct);
                    return Results.Ok(new { orderId });
                }
                catch (ArgumentNullException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .RequireAuthorization()
            .DisableAntiforgery();

        group.MapGet("/{id:int}", async (
            int id,
            ClaimsPrincipal user,
            ApplicationDbContext db,
            CancellationToken ct) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Results.Unauthorized();
                }

                var order = await db.Orders
                    .Include(o => o.Itens)
                    .ThenInclude(i => i.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == id && o.ApplicationUserId == userId, ct);

                if (order is null)
                {
                    return Results.NotFound(new { message = "Pedido não encontrado." });
                }

                var dto = new
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
                        Nome = i.Product.Nome,
                        i.Quantidade,
                        i.PrecoUnitario,
                        Subtotal = i.PrecoUnitario * i.Quantidade,
                        ImagemUrl = i.Product.ImagemUrl
                    }).ToList()
                };

                return Results.Ok(dto);
            })
            .RequireAuthorization();

        return endpoints;
    }
}


