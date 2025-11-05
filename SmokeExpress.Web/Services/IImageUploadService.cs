// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto

namespace SmokeExpress.Web.Services;

/// <summary>
/// Serviço responsável pelo upload de imagens de produtos.
/// </summary>
public interface IImageUploadService
{
    /// <summary>
    /// Faz upload de uma imagem de produto e retorna o caminho relativo para acesso via HTTP.
    /// </summary>
    /// <param name="file">Arquivo de imagem a ser enviado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Caminho relativo da imagem (ex: /images/products/guid.jpg) ou null se o upload falhar.</returns>
    Task<string?> UploadProductImageAsync(Stream fileStream, string fileName, long fileSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma imagem de produto do sistema de arquivos.
    /// </summary>
    /// <param name="imagePath">Caminho relativo da imagem (ex: /images/products/guid.jpg).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>True se a imagem foi removida com sucesso, False caso contrário.</returns>
    Task<bool> DeleteProductImageAsync(string? imagePath, CancellationToken cancellationToken = default);
}

