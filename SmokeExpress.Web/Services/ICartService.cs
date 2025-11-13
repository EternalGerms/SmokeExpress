// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Contrato para manipulação do carrinho de compras dos usuários.
/// Em Blazor Server, serviços Scoped são instanciados por circuito (por usuário conectado).
/// </summary>
public interface ICartService
{
    Task AddAsync(Product product, int quantity = 1, CancellationToken cancellationToken = default);
    Task RemoveAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CartItemViewModel>> GetItemsAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

