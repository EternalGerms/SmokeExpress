// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Data;

namespace SmokeExpress.Web.Routing;

public static class ProductEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/products/by-ids", async ([FromBody] int[] ids, ApplicationDbContext db, CancellationToken ct) =>
            {
                ids ??= Array.Empty<int>();
                var list = await db.Products
                    .Where(p => ids.Contains(p.Id))
                    .Select(p => new { p.Id, p.Nome, p.Preco, p.ImagemUrl })
                    .ToListAsync(ct);
                return Results.Ok(list);
            })
            .DisableAntiforgery();

        return endpoints;
    }
}


