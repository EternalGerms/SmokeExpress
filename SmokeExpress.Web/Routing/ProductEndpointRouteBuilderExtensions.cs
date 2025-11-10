// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Extensions;

namespace SmokeExpress.Web.Routing;

/// <summary>
/// Extensões de roteamento para funcionalidades de consulta de produtos via API.
/// </summary>
public static class ProductEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Registra endpoints relacionados à busca simplificada de produtos.
    /// </summary>
    /// <param name="endpoints">Instância do construtor de rotas.</param>
    /// <returns>O próprio <see cref="IEndpointRouteBuilder"/> para encadeamento.</returns>
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/products/by-ids", async ([FromBody] int[] ids, ApplicationDbContext db, CancellationToken ct) =>
        {
            ids ??= Array.Empty<int>();
            var products = await db.Products
                .Where(p => ids.Contains(p.Id))
                .ToListAsync(ct);
            var list = products.Select(p => p.ToProductLiteDto()).ToList();
            return Results.Ok(list);
        });

        return endpoints;
    }
}


