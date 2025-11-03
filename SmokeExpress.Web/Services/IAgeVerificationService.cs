// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto

namespace SmokeExpress.Web.Services;

/// <summary>
/// Serviço responsável por gerenciar o consentimento de verificação de idade (18+).
/// </summary>
public interface IAgeVerificationService
{
    /// <summary>
    /// Verifica se o usuário já forneceu consentimento sobre maioridade.
    /// </summary>
    /// <returns>True se já consentiu, False caso contrário. Null se não há cookie definido.</returns>
    Task<bool?> HasConsentedAsync();

    /// <summary>
    /// Registra o consentimento do usuário sobre maioridade.
    /// </summary>
    /// <param name="isAdult">True se o usuário confirmou ter 18+ anos, False caso contrário.</param>
    Task SetConsentAsync(bool isAdult);

    /// <summary>
    /// Remove o consentimento armazenado (limpa o cookie).
    /// </summary>
    Task ClearConsentAsync();
}

