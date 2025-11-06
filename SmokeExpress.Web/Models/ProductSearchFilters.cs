// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto

namespace SmokeExpress.Web.Models;

/// <summary>
/// DTO para filtros de busca avançada de produtos.
/// </summary>
public class ProductSearchFilters
{
    /// <summary>
    /// Termo de busca para filtrar por nome ou descrição.
    /// </summary>
    public string? TermoBusca { get; set; }

    /// <summary>
    /// ID da categoria para filtrar produtos.
    /// </summary>
    public int? CategoriaId { get; set; }

    /// <summary>
    /// Preço mínimo para filtrar produtos.
    /// </summary>
    public decimal? PrecoMin { get; set; }

    /// <summary>
    /// Preço máximo para filtrar produtos.
    /// </summary>
    public decimal? PrecoMax { get; set; }

    /// <summary>
    /// Se true, retorna apenas produtos com estoque disponível (Estoque > 0).
    /// </summary>
    public bool? ApenasEmEstoque { get; set; }

    /// <summary>
    /// Tipo de ordenação a ser aplicada nos resultados.
    /// </summary>
    public ProductSortOrder Ordenacao { get; set; } = ProductSortOrder.Nome;
}

