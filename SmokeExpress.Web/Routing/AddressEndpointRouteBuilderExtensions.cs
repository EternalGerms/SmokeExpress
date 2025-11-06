// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Services;

namespace SmokeExpress.Web.Routing;

public static class AddressEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/addresses").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IAddressService svc, CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
            var list = await svc.ListAsync(userId, ct);
            return Results.Ok(list.Select(a => new AddressDto(a)));
        });

        group.MapPost("/", async ([FromBody] AddressDto dto, ClaimsPrincipal user, IAddressService svc, CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
            var created = await svc.CreateAsync(userId, dto.ToEntity(), ct);
            return Results.Ok(new AddressDto(created));
        });

        group.MapPut("/{id:int}", async (int id, [FromBody] AddressDto dto, ClaimsPrincipal user, IAddressService svc, CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
            var updated = await svc.UpdateAsync(userId, id, dto.ToEntity(), ct);
            return updated is null ? Results.NotFound() : Results.Ok(new AddressDto(updated));
        });

        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal user, IAddressService svc, CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
            var ok = await svc.DeleteAsync(userId, id, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        group.MapPost("/{id:int}/make-default", async (int id, ClaimsPrincipal user, IAddressService svc, CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
            var ok = await svc.MakeDefaultAsync(userId, id, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        return endpoints;
    }

    private sealed record AddressDto
    {
        public int Id { get; init; }
        public string Rua { get; init; } = string.Empty;
        public string? Numero { get; init; }
        public string Cidade { get; init; } = string.Empty;
        public string Bairro { get; init; } = string.Empty;
        public string? Complemento { get; init; }
        public bool IsDefault { get; init; }

        public AddressDto() { }
        public AddressDto(Address a)
        {
            Id = a.Id;
            Rua = a.Rua;
            Numero = a.Numero;
            Cidade = a.Cidade;
            Bairro = a.Bairro;
            Complemento = a.Complemento;
            IsDefault = a.IsDefault;
        }

        public Address ToEntity() => new()
        {
            Rua = Rua,
            Numero = Numero,
            Cidade = Cidade,
            Bairro = Bairro,
            Complemento = Complemento,
            IsDefault = IsDefault
        };
    }
}


