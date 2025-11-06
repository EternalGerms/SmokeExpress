// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

public interface IOrderService
{
    Task<int> CriarPedidoAsync(
        string userId,
        IEnumerable<CartItemDto> cartItems,
        EnderecoEntregaDto endereco,
        decimal frete = 0m,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> ListarPedidosPorUsuarioAsync(
        string userId,
        CancellationToken cancellationToken = default);
}


