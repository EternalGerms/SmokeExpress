// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto

namespace SmokeExpress.Web.Services;

/// <summary>
/// Serviço responsável por gerenciar o consentimento de verificação de idade (18+).
/// </summary>
public interface IAgeVerificationService
{
    /// <summary>
    /// Verifica se o usuário já forneceu consentimento quanto à maioridade.
    /// </summary>
    /// <returns>
    /// <c>true</c> quando o consentimento indica que o usuário é maior de idade,
    /// <c>false</c> quando o consentimento indica o oposto e <c>null</c> quando não existe registro.
    /// </returns>
    Task<bool?> HasConsentedAsync();

    /// <summary>
    /// Registra o consentimento do usuário sobre maioridade.
    /// </summary>
    /// <param name="isAdult"><c>true</c> quando o usuário declara ter 18+ anos; caso contrário <c>false</c>.</param>
    Task SetConsentAsync(bool isAdult);

    /// <summary>
    /// Remove o consentimento armazenado (limpa o cookie).
    /// </summary>
    Task ClearConsentAsync();
}

