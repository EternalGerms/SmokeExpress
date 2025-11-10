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
    /// <param name="fileStream">Fluxo do arquivo de imagem a ser enviado.</param>
    /// <param name="fileName">Nome original do arquivo enviado.</param>
    /// <param name="fileSize">Tamanho do arquivo em bytes.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Caminho relativo da imagem (ex: /images/products/guid.jpg) ou <c>null</c> se o upload falhar.</returns>
    /// <exception cref="ArgumentNullException">
    /// Lançada quando <paramref name="fileStream"/> ou <paramref name="fileName"/> são nulos.
    /// </exception>
    Task<string?> UploadProductImageAsync(Stream fileStream, string fileName, long fileSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma imagem de produto do sistema de arquivos.
    /// </summary>
    /// <param name="imagePath">Caminho relativo da imagem (ex: /images/products/guid.jpg).</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns><c>true</c> quando a imagem é removida com sucesso; caso contrário, <c>false</c>.</returns>
    Task<bool> DeleteProductImageAsync(string? imagePath, CancellationToken cancellationToken = default);
}

