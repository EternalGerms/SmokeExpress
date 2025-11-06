// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto

namespace SmokeExpress.Web.Models;

/// <summary>
/// Enum que define as opções de ordenação para busca de produtos.
/// </summary>
public enum ProductSortOrder
{
    /// <summary>
    /// Ordenação por nome (A-Z).
    /// </summary>
    Nome = 0,

    /// <summary>
    /// Ordenação por preço crescente (menor para maior).
    /// </summary>
    PrecoAsc = 1,

    /// <summary>
    /// Ordenação por preço decrescente (maior para menor).
    /// </summary>
    PrecoDesc = 2,

    /// <summary>
    /// Ordenação por relevância (produtos que mais correspondem ao termo de busca).
    /// </summary>
    Relevancia = 3
}

