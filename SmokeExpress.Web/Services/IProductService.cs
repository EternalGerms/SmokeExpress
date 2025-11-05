// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Contrato para lidar com operações de produtos no contexto administrativo.
/// </summary>
public interface IProductService
{
    Task<IReadOnlyCollection<Product>> ListarAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<Product>> ListarPaginadoAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca produtos paginados com filtros opcionais por termo de busca e categoria.
    /// </summary>
    /// <param name="termoBusca">Termo para buscar em Nome ou Descrição (opcional).</param>
    /// <param name="categoriaId">ID da categoria para filtrar (opcional).</param>
    /// <param name="pageNumber">Número da página (inicia em 1).</param>
    /// <param name="pageSize">Itens por página.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado paginado de produtos.</returns>
    Task<PagedResult<Product>> BuscarPaginadoAsync(string? termoBusca, int? categoriaId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<Product?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Product> CriarAsync(Product product, CancellationToken cancellationToken = default);

    Task AtualizarAsync(Product product, CancellationToken cancellationToken = default);

    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
}



