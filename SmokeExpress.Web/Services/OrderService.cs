// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmokeExpress.Web.Common;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Helpers;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação padrão de <see cref="IOrderService"/> responsável pelo ciclo de vida de pedidos e suas validações.
/// </summary>
public class OrderService(ApplicationDbContext dbContext, ILogger<OrderService> logger) : IOrderService
{
    /// <inheritdoc />
    public async Task<int> CriarPedidoAsync(
        string userId,
        IEnumerable<CartItemDto> cartItems,
        EnderecoEntregaDto endereco,
        decimal frete = 0m,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNullOrWhiteSpace(userId, nameof(userId));
        Guard.AgainstNull(cartItems, nameof(cartItems));
        Guard.AgainstNull(endereco, nameof(endereco));
        // Padrão de logging: propriedades nomeadas {Prop} e BeginScope com contexto quando disponível.
        using var _ = logger.BeginScope(new { UserId = userId });
        // Validar entrada
        var validacao = await ValidarPedidoAsync(userId, cartItems, endereco, cancellationToken);
        if (!validacao.IsSuccess)
        {
            throw new ValidationException(validacao.ErrorMessage!);
        }

        var itensLista = cartItems!.ToList();
        var productIds = itensLista.Select(i => i.ProductId).Distinct().ToList();
        var products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var productById = products.ToDictionary(p => p.Id);

        var order = new Order
        {
            ApplicationUserId = userId,
            Status = OrderStatus.Processando,
            Rua = endereco.Rua,
            Numero = endereco.Numero,
            Cidade = endereco.Cidade,
            Bairro = endereco.Bairro,
            Complemento = endereco.Complemento,
            DataPedido = DateTime.UtcNow,
        };

        decimal total = 0m;
        foreach (var item in itensLista)
        {
            var product = productById[item.ProductId];
            var preco = product.Preco;
            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantidade = item.Quantidade,
                PrecoUnitario = preco
            };
            total += preco * item.Quantidade;
            order.Itens.Add(orderItem);

            // Deduz estoque (MVP)
            product.Estoque -= item.Quantidade;
        }

        order.TotalPedido = total + Math.Max(0m, frete);

        dbContext.Orders.Add(order);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao salvar pedido no banco de dados. UserId: {UserId}", userId);
            throw new BusinessException("Erro ao processar pedido. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar pedido. UserId: {UserId}", userId);
            throw;
        }

        logger.LogInformation("Pedido criado. OrderId: {OrderId}, UserId: {UserId}, ItemCount: {ItemCount}, Total: {Total}", order.Id, userId, itensLista.Count, FormatHelper.FormatCurrency(order.TotalPedido));

        return order.Id;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Order>> ListarPedidosPorUsuarioAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNullOrWhiteSpace(userId, nameof(userId));

        var pedidos = await dbContext.Orders
            .Where(o => o.ApplicationUserId == userId)
            .OrderByDescending(o => o.DataPedido)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return pedidos;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Order>> ListarTodosAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(o => o.Cliente)
            .OrderByDescending(o => o.DataPedido)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Order?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        return await dbContext.Orders
            .Include(o => o.Cliente)
            .Include(o => o.Itens)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AtualizarStatusAsync(
        int id,
        OrderStatus novoStatus,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (order is null) return false;
        order.Status = novoStatus;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao atualizar status do pedido {OrderId} no banco de dados", id);
            throw new BusinessException("Erro ao atualizar status do pedido. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar status do pedido {OrderId}", id);
            throw;
        }
        return true;
    }

    private async Task<Result> ValidarPedidoAsync(
        string userId,
        IEnumerable<CartItemDto>? cartItems,
        EnderecoEntregaDto? endereco,
        CancellationToken cancellationToken)
    {
        // Validar userId
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure("UserId inválido.");
        }

        // Validar carrinho
        var itensLista = cartItems?.ToList() ?? [];
        if (itensLista.Count == 0)
        {
            return Result.Failure("Carrinho vazio.");
        }

        // Validar quantidades
        if (itensLista.Any(i => i.Quantidade <= 0))
        {
            return Result.Failure("Quantidade inválida em um ou mais itens.");
        }

        // Validar produtos existentes
        var productIds = itensLista.Select(i => i.ProductId).Distinct().ToList();
        var products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            return Result.Failure("Um ou mais produtos não foram encontrados.");
        }

        var productById = products.ToDictionary(p => p.Id);

        // Validar estoque
        foreach (var item in itensLista)
        {
            var product = productById[item.ProductId];
            if (item.Quantidade > product.Estoque)
            {
                return Result.Failure($"Quantidade acima do estoque para '{product.Nome}'. Disponível: {product.Estoque}.");
            }
        }

        // Validar endereço
        if (endereco == null)
        {
            return Result.Failure("Endereço de entrega é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(endereco.Rua))
        {
            return Result.Failure("O campo Rua do endereço de entrega é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(endereco.Cidade))
        {
            return Result.Failure("O campo Cidade do endereço de entrega é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(endereco.Bairro))
        {
            return Result.Failure("O campo Bairro do endereço de entrega é obrigatório.");
        }

        return Result.Success();
    }
}


