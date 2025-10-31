// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Security;
using SmokeExpress.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração da conexão com o SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A string de conexão 'DefaultConnection' não foi configurada.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        // Regras de segurança básicas para senhas e contas
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, BcryptPasswordHasher>();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Garante cookies seguros e HttpOnly de acordo com o requisito
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddCascadingAuthenticationState();

// Serviços de domínio
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

app.MapPost("/account/login", async ([FromForm] LoginRequest login,
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

// Pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

internal sealed record LoginRequest(string Email, string Password, bool RememberMe = false, string? ReturnUrl = "/");
