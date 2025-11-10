// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Routing;

/// <summary>
/// Extensões responsáveis por organizar os endpoints de autenticação utilizados pelos componentes Blazor.
/// Mantém o <c>Program.cs</c> mais limpo e facilita a evolução desses fluxos.
/// </summary>
public static class AccountEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Registra os endpoints auxiliares utilizados pelas páginas de conta (login e logout).
    /// </summary>
    /// <param name="endpoints">Instância do construtor de rotas.</param>
    /// <returns>O próprio <see cref="IEndpointRouteBuilder"/> para encadeamento.</returns>
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/account/login", async ([FromForm] LoginRequest login,
            SignInManager<ApplicationUser> signInManager) =>
            {
                var signInResult = await signInManager.PasswordSignInAsync(
                    login.Email,
                    login.Password,
                    login.RememberMe,
                    lockoutOnFailure: true);

                if (signInResult.Succeeded)
                {
                    var destination = string.IsNullOrWhiteSpace(login.ReturnUrl)
                        ? "/"
                        : login.ReturnUrl;

                    return Results.Redirect(destination);
                }

                var errorCode = signInResult.IsLockedOut ? "locked" : "invalid";
                var returnUrl = string.IsNullOrWhiteSpace(login.ReturnUrl) ? "/" : login.ReturnUrl;
                return Results.Redirect($"/account/login?error={errorCode}&returnUrl={Uri.EscapeDataString(returnUrl)}");
            })
            .AllowAnonymous()
            .DisableAntiforgery();

        endpoints.MapPost("/account/logout", async (SignInManager<ApplicationUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Redirect("/");
            })
            .RequireAuthorization()
            .DisableAntiforgery();

        return endpoints;
    }

    private sealed record LoginRequest(string Email, string Password, bool RememberMe = false, string? ReturnUrl = "/");
}

