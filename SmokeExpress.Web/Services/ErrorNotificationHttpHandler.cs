// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SmokeExpress.Web.Services;

/// <summary>
/// DelegatingHandler que notifica o serviço de erros quando uma requisição HTTP falha.
/// </summary>
public class ErrorNotificationHttpHandler : DelegatingHandler
{
    private readonly IErrorNotificationService _errorNotificationService;

    public ErrorNotificationHttpHandler(IErrorNotificationService errorNotificationService)
    {
        _errorNotificationService = errorNotificationService ?? throw new ArgumentNullException(nameof(errorNotificationService));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = response.Content != null
                    ? await response.Content.ReadAsStringAsync(cancellationToken)
                    : string.Empty;

                _errorNotificationService.Notify(
                    $"Requisição para {request.RequestUri} retornou status {(int)response.StatusCode} ({response.StatusCode}). Conteúdo: {responseContent}");
            }

            return response;
        }
        catch (HttpRequestException httpEx)
        {
            _errorNotificationService.Notify(httpEx, $"Falha ao enviar requisição para {request.RequestUri}");
            throw;
        }
    }
}

