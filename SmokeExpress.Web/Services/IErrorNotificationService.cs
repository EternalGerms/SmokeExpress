// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto

namespace SmokeExpress.Web.Services;

/// <summary>
/// Define um contrato simples para notificação de erros dentro da aplicação.
/// </summary>
public interface IErrorNotificationService
{
    /// <summary>
    /// Registra uma ocorrência de erro a partir de uma exceção.
    /// </summary>
    /// <param name="exception">Exceção capturada.</param>
    /// <param name="contextMessage">Mensagem adicional opcional.</param>
    void Notify(Exception exception, string? contextMessage = null);

    /// <summary>
    /// Registra uma ocorrência de erro ou aviso a partir de uma mensagem.
    /// </summary>
    /// <param name="message">Mensagem a ser registrada.</param>
    void Notify(string message);
}

