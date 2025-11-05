// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Http;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação do serviço de verificação de idade.
/// Gerencia cookies seguros para armazenar consentimento de maioridade.
/// </summary>
public class AgeVerificationService : IAgeVerificationService
{
    private const string CookieName = "AgeVerificationConsent";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AgeVerificationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<bool?> HasConsentedAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return Task.FromResult<bool?>(null);
        }

        if (httpContext.Request.Cookies.TryGetValue(CookieName, out var cookieValue))
        {
            if (bool.TryParse(cookieValue, out var isAdult))
            {
                return Task.FromResult<bool?>(isAdult);
            }
        }

        return Task.FromResult<bool?>(null);
    }

    public Task SetConsentAsync(bool isAdult)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return Task.CompletedTask;
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true // Permite cookie mesmo sem consentimento LGPD (compliance legal)
        };

        httpContext.Response.Cookies.Append(CookieName, isAdult.ToString(), cookieOptions);

        return Task.CompletedTask;
    }

    public Task ClearConsentAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return Task.CompletedTask;
        }

        httpContext.Response.Cookies.Delete(CookieName);

        return Task.CompletedTask;
    }
}

