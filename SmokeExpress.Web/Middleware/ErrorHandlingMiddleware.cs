// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmokeExpress.Web.Services;

namespace SmokeExpress.Web.Middleware;

/// <summary>
/// Middleware responsável por capturar exceções não tratadas e registrar logs amigáveis.
/// Também emite notificações através do serviço de notificação de erros.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var errorNotificationService = context.RequestServices.GetService<IErrorNotificationService>();

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado durante o processamento da requisição.");
            errorNotificationService?.Notify(ex, "Erro não tratado durante o processamento da requisição.");

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    title = "Erro inesperado",
                    status = StatusCodes.Status500InternalServerError,
                    detail = "Ocorreu um erro inesperado. Nossa equipe foi notificada e já está analisando."
                };

                await context.Response.WriteAsJsonAsync(payload);
            }
        }
    }
}

