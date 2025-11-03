// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.Linq;
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

// HttpContextAccessor para serviços que precisam acessar HTTP context
builder.Services.AddHttpContextAccessor();

// Serviços de domínio
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAgeVerificationService, AgeVerificationService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

var app = builder.Build();

/// <summary>
/// Valida se uma URL é local/relativa e segura para redirecionamento.
/// Previne ataques de open redirect validando que a URL:
/// - Não é absoluta (não começa com http:// ou https://)
/// - Não é protocol-relative (não começa com //)
/// - É relativa e começa com "/"
/// - Não contém sequências perigosas (ex: "../")
/// </summary>
static bool IsLocalUrl(string? url)
{
    if (string.IsNullOrWhiteSpace(url))
    {
        return false;
    }

    // Rejeita URLs absolutas (http://, https://)
    if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
        url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    // Rejeita protocol-relative URLs (//)
    if (url.StartsWith("//", StringComparison.Ordinal))
    {
        return false;
    }

    // Rejeita javascript: e outros protocolos perigosos
    if (url.Contains(':', StringComparison.Ordinal) && !url.StartsWith("/", StringComparison.Ordinal))
    {
        return false;
    }

    // Aceita apenas URLs relativas que começam com "/"
    if (!url.StartsWith("/", StringComparison.Ordinal))
    {
        return false;
    }

    // Rejeita sequências de directory traversal
    if (url.Contains("../", StringComparison.Ordinal) || url.Contains("..\\", StringComparison.Ordinal))
    {
        return false;
    }

    return true;
}

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
            var destination = IsLocalUrl(login.ReturnUrl)
                ? login.ReturnUrl!
                : "/";

            return Results.Redirect(destination);
        }

        var errorCode = signInResult.IsLockedOut ? "locked" : "invalid";
        var returnUrl = IsLocalUrl(login.ReturnUrl) ? login.ReturnUrl! : "/";
        return Results.Redirect($"/account/login?error={errorCode}&returnUrl={Uri.EscapeDataString(returnUrl)}");
    })
    .AllowAnonymous()
    .DisableAntiforgery();

app.MapGet("/account/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
})
.AllowAnonymous();

app.MapPost("/account/register", async ([FromForm] RegisterRequest request,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<Program> logger) =>
{
    // Validação de data futura
    if (request.DataNascimento > DateTime.UtcNow)
    {
        return Results.Redirect("/account/register?error=date&message=" + Uri.EscapeDataString("A data de nascimento não pode ser no futuro."));
    }

    // Validação de idade mínima
    if (request.DataNascimento > DateTime.UtcNow.AddYears(-18))
    {
        return Results.Redirect("/account/register?error=age&message=" + Uri.EscapeDataString("É necessário ter ao menos 18 anos para criar uma conta."));
    }

    // Validação de senhas
    if (request.Senha != request.ConfirmacaoSenha)
    {
        return Results.Redirect("/account/register?error=password&message=" + Uri.EscapeDataString("As senhas informadas não coincidem."));
    }

    var usuario = new ApplicationUser
    {
        UserName = request.Email,
        Email = request.Email,
        NomeCompleto = request.NomeCompleto,
        Rua = request.Rua,
        Numero = string.IsNullOrWhiteSpace(request.Numero) ? null : request.Numero,
        Cidade = request.Cidade,
        Bairro = request.Bairro,
        Complemento = string.IsNullOrWhiteSpace(request.Complemento) ? null : request.Complemento,
        DataNascimento = request.DataNascimento,
        EmailConfirmed = true
    };

    var resultado = await userManager.CreateAsync(usuario, request.Senha);

    if (resultado.Succeeded)
    {
        await signInManager.SignInAsync(usuario, isPersistent: false);
        return Results.Redirect("/");
    }

    var errorMessage = string.Join(" ", resultado.Errors.Select(e => e.Description));
    logger.LogError("Erro ao registrar usuário {Email}: {Errors}", request.Email, errorMessage);
    return Results.Redirect("/account/register?error=registration&message=" + Uri.EscapeDataString(errorMessage));
})
.AllowAnonymous()
.DisableAntiforgery();

app.MapPost("/age-verification/consent", async ([FromForm] bool isAdult,
    IAgeVerificationService ageVerificationService) =>
{
    await ageVerificationService.SetConsentAsync(isAdult);
    
    if (isAdult)
    {
        return Results.Redirect("/");
    }
    else
    {
        return Results.Redirect("/?blocked=true");
    }
})
.AllowAnonymous()
.DisableAntiforgery();

app.MapPost("/age-verification/reset", async (IAgeVerificationService ageVerificationService) =>
{
    await ageVerificationService.ClearConsentAsync();
    return Results.Redirect("/");
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

internal sealed record RegisterRequest(
    string NomeCompleto,
    string Email,
    string Rua,
    string? Numero,
    string Cidade,
    string Bairro,
    string? Complemento,
    DateTime DataNascimento,
    string Senha,
    string ConfirmacaoSenha);
