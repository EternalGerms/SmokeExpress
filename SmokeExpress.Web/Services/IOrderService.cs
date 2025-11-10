// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Define operações de leitura e escrita relacionadas a pedidos dos clientes.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Cria um novo pedido para o usuário informado após validar carrinho, estoque e endereço.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="cartItems">Itens presentes no carrinho de compras.</param>
    /// <param name="endereco">Endereço de entrega utilizado no pedido.</param>
    /// <param name="frete">Valor do frete a ser somado ao total do pedido (padrão: 0).</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>ID do pedido criado e persistido.</returns>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="userId"/> é vazio.</exception>
    /// <exception cref="ArgumentNullException">
    /// Lançada quando <paramref name="cartItems"/> ou <paramref name="endereco"/> são nulos.
    /// </exception>
    /// <exception cref="ValidationException">Lançada quando os dados de negócio do pedido não são válidos.</exception>
    /// <exception cref="BusinessException">Lançada quando ocorre erro ao persistir o pedido.</exception>
    Task<int> CriarPedidoAsync(
        string userId,
        IEnumerable<CartItemDto> cartItems,
        EnderecoEntregaDto endereco,
        decimal frete = 0m,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista todos os pedidos associados ao usuário informado.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Lista apenas com pedidos do usuário ou vazia quando não há registros.</returns>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="userId"/> é vazio.</exception>
    Task<IReadOnlyList<Order>> ListarPedidosPorUsuarioAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista todos os pedidos registrados na plataforma, ordenados pela data mais recente.
    /// </summary>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Lista completa de pedidos.</returns>
    Task<IReadOnlyList<Order>> ListarTodosAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um pedido com seus relacionamentos pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do pedido.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Instância do pedido encontrada ou <c>null</c> quando não existe.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="id"/> é menor ou igual a zero.</exception>
    Task<Order?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o status de um pedido existente.
    /// </summary>
    /// <param name="id">Identificador do pedido.</param>
    /// <param name="novoStatus">Novo status que deve ser aplicado.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns><c>true</c> quando o status foi atualizado; caso contrário, <c>false</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="id"/> é menor ou igual a zero.</exception>
    /// <exception cref="BusinessException">Lançada quando ocorre erro ao salvar a alteração.</exception>
    Task<bool> AtualizarStatusAsync(
        int id,
        OrderStatus novoStatus,
        CancellationToken cancellationToken = default);
}


