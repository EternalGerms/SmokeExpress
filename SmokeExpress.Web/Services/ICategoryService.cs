// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Contrato para lidar com operações de categorias no contexto administrativo.
/// </summary>
public interface ICategoryService
{
    Task<IReadOnlyCollection<Category>> ListarAsync(CancellationToken cancellationToken = default);

    Task<Category?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Category> CriarAsync(Category category, CancellationToken cancellationToken = default);

    Task AtualizarAsync(Category category, CancellationToken cancellationToken = default);

    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
}

