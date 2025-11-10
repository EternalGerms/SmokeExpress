// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Constants;

public static class ApplicationConstants
{
    // Paginação
    public const int DefaultPageSize = 12;
    public const int AdminPageSize = 10;
    
    // Comprimentos máximos de campos
    public const int MaxProductNameLength = 150;
    public const int MaxProductDescriptionLength = 2000;
    public const int MaxCategoryNameLength = 120;
    public const int MaxCategoryDescriptionLength = 500;
    
    // Validação de senha
    public const int MinPasswordLength = 8;
    
    // Tamanho de arquivo
    public const int MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB
    
    // Resumo de produto
    public const int ProductSummaryLength = 150;
    
    // Analytics
    public const int DefaultTopItems = 10;
}

