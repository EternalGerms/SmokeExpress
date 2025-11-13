// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.Extensions.Logging;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação básica do serviço de notificação de erros que utiliza ILogger.
/// </summary>
public class ErrorNotificationService(ILogger<ErrorNotificationService> logger) : IErrorNotificationService
{
    private readonly ILogger<ErrorNotificationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public void Notify(Exception exception, string? contextMessage = null)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (!string.IsNullOrWhiteSpace(contextMessage))
        {
            _logger.LogError(exception, contextMessage);
        }
        else
        {
            _logger.LogError(exception, "Erro inesperado durante a execução da aplicação.");
        }
    }

    public void Notify(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _logger.LogWarning("Notificação de erro: {Message}", message);
    }
}

