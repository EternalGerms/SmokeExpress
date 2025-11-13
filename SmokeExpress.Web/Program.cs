// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.Linq;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using MudBlazor.Services;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Middleware;
using SmokeExpress.Web.Routing;
using SmokeExpress.Web.Security;
using SmokeExpress.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();

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
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 10;
        options.Password.RequiredUniqueChars = 4;
        
        // Configurações de lockout para prevenir ataques de força bruta
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5; // Bloqueia após 5 tentativas falhadas
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Bloqueio por 15 minutos
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, BcryptPasswordHasher>();
builder.Services.AddMemoryCache();

// Registrar o descritor de erros customizado em português
builder.Services.AddScoped<IdentityErrorDescriber, PortugueseIdentityErrorDescriber>();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Garante cookies seguros e HttpOnly de acordo com o requisito
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/login";
    options.ReturnUrlParameter = "returnUrl";
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("AuthEndpoints", context =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 10,
                TokensPerPeriod = 10,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

// Configurar Antiforgery para proteção CSRF
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

builder.Services.AddMudServices();
builder.Services.AddCascadingAuthenticationState();

// Configurar HttpClient com handler de notificação de erros (se necessário no futuro)
builder.Services.AddHttpClient("DefaultClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
    .AddHttpMessageHandler<ErrorNotificationHttpHandler>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<AntiforgeryTokenStore>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IAgeVerificationService, AgeVerificationService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Serviço de notificação de erros
builder.Services.AddScoped<IErrorNotificationService, ErrorNotificationService>();

// Handler HTTP para notificação de erros
builder.Services.AddScoped<ErrorNotificationHttpHandler>();

var app = builder.Build();

// Executar migrações pendentes automaticamente (com proteção para bancos já provisionados manualmente)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var pendingMigrations = dbContext.Database.GetPendingMigrations();

        if (pendingMigrations.Any())
        {
            dbContext.Database.Migrate();
            logger.LogInformation("Migrações aplicadas: {Migrations}", string.Join(", ", pendingMigrations));
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Falha ao aplicar migrações automaticamente. Considere executar 'dotnet ef database update' manualmente.");
        throw;
    }
}

app.MapAccountEndpoints();

app.MapGet("/antiforgery/token", (IAntiforgery antiforgery, HttpContext httpContext) =>
    {
        var tokens = antiforgery.GetAndStoreTokens(httpContext);
        httpContext.Response.Headers.CacheControl = "no-store";
        return Results.Json(new { requestToken = tokens.RequestToken });
    })
    .AllowAnonymous();

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

// Middleware de tratamento de erros (deve vir antes do UseRouting)
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapStaticAssets();

app.UseRouting();

app.Use(async (context, next) =>
{
    if (!context.Request.Cookies.ContainsKey("__RequestVerificationToken"))
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.RequestServices.GetRequiredService<AntiforgeryTokenStore>().SetToken(tokens.RequestToken);
    }
    else
    {
        var tokenStore = context.RequestServices.GetRequiredService<AntiforgeryTokenStore>();
        if (string.IsNullOrEmpty(tokenStore.RequestToken))
        {
            var tokenSet = context.RequestServices.GetRequiredService<IAntiforgery>().GetTokens(context);
            tokenStore.SetToken(tokenSet.RequestToken);
        }
    }

    await next();
});

app.UseRateLimiter();
app.UseAntiforgery();

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.TryAdd("Permissions-Policy", "geolocation=(), microphone=(), camera=(), payment=()");
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com data:; img-src 'self' https://images.unsplash.com https://placehold.co data:; connect-src 'self' wss:;";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
