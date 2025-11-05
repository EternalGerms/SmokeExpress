// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto

namespace SmokeExpress.Web.Models;

/// <summary>
/// Representa um resultado paginado de uma consulta, contendo os itens da página atual e informações de paginação.
/// </summary>
/// <typeparam name="T">Tipo dos itens paginados.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Itens da página atual.
    /// </summary>
    public IReadOnlyCollection<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Número total de itens em todas as páginas.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Número da página atual (baseado em 1).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Tamanho da página (quantidade de itens por página).
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Número total de páginas.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Indica se existe uma página anterior.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica se existe uma página seguinte.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}

