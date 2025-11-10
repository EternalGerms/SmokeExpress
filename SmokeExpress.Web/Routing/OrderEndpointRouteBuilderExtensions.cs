// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Extensions;
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
                catch (NotFoundException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (BusinessException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (ArgumentNullException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (DbUpdateException)
                {
                    return Results.Problem("Erro ao processar pedido. Tente novamente.", statusCode: 500);
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

                var dto = order.ToOrderApiDto();
                return Results.Ok(dto);
            })
            .RequireAuthorization();

        return endpoints;
    }
}


