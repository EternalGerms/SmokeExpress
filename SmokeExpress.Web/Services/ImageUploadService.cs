// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using System.Globalization;
using SmokeExpress.Web.Constants;
using SmokeExpress.Web.Common;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação do serviço de upload de imagens de produtos.
/// </summary>
public class ImageUploadService : IImageUploadService
{
    private const string ProductsImageDirectory = "wwwroot/images/products";
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageUploadService> _logger;

    public ImageUploadService(IWebHostEnvironment environment, ILogger<ImageUploadService> logger)
    {
        _environment = environment;
        _logger = logger;

        // Garantir que o diretório existe
        var directoryPath = Path.Combine(_environment.ContentRootPath, ProductsImageDirectory);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            _logger.LogInformation("Diretório de imagens de produtos criado: {Directory}", directoryPath);
        }
    }

    public async Task<string?> UploadProductImageAsync(Stream fileStream, string fileName, long fileSize, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(fileStream, nameof(fileStream));
        Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName));
        try
        {
            // Validar tamanho do arquivo
            if (fileSize > ApplicationConstants.MaxImageSizeBytes)
            {
                _logger.LogWarning("Tentativa de upload de arquivo muito grande: {Size} bytes (máximo: {MaxSize} bytes)", fileSize, ApplicationConstants.MaxImageSizeBytes);
                return null;
            }

            // Validar extensão
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Tentativa de upload de arquivo com extensão não permitida: {Extension}", extension);
                return null;
            }

            // Gerar nome único para o arquivo
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var directoryPath = Path.Combine(_environment.ContentRootPath, ProductsImageDirectory);
            var filePath = Path.Combine(directoryPath, uniqueFileName);

            // Salvar arquivo
            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
            }

            // Retornar caminho relativo para acesso via HTTP
            var relativePath = $"/images/products/{uniqueFileName}";
            _logger.LogInformation("Imagem de produto enviada com sucesso: {Path}", relativePath);
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload da imagem de produto: {FileName}", fileName);
            return null;
        }
    }

    public Task<bool> DeleteProductImageAsync(string? imagePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return Task.FromResult(true); // Não há imagem para deletar
        }

        try
        {
            // Validar que o caminho é relativo e está dentro do diretório permitido
            if (!imagePath.StartsWith("/images/products/", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Tentativa de deletar arquivo fora do diretório permitido: {Path}", imagePath);
                return Task.FromResult(false);
            }

            // Extrair nome do arquivo do caminho relativo
            var fileName = Path.GetFileName(imagePath);
            var filePath = Path.Combine(_environment.ContentRootPath, ProductsImageDirectory, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Imagem de produto removida: {Path}", imagePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar imagem de produto: {Path}", imagePath);
            return Task.FromResult(false);
        }
    }
}

