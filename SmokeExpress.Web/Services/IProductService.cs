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

    Task<Product?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Product> CriarAsync(Product product, CancellationToken cancellationToken = default);

    Task AtualizarAsync(Product product, CancellationToken cancellationToken = default);

    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
}


