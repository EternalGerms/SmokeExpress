// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Services;

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

                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Results.Unauthorized();
                }

                try
                {
                    var orderId = await orderService.CriarPedidoAsync(userId, req.Itens, req.Endereco, ct);
                    return Results.Ok(new { orderId });
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

        return endpoints;
    }
}


