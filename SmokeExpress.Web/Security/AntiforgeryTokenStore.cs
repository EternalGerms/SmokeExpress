// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto

namespace SmokeExpress.Web.Security;

/// <summary>
/// Armazena o token antiforgery atual para reaproveitamento em requisições subsequentes.
/// Como serviços Scoped em Blazor Server são por circuito, o armazenamento é per-user.
/// </summary>
public class AntiforgeryTokenStore
{
    private readonly object _syncRoot = new();

    public string? RequestToken { get; private set; }

    public void SetToken(string? token)
    {
        lock (_syncRoot)
        {
            RequestToken = token;
        }
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            RequestToken = null;
        }
    }
}

