// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

public class OrderService(ApplicationDbContext dbContext, ILogger<OrderService> logger) : IOrderService
{
    public async Task<int> CriarPedidoAsync(
        string userId,
        IEnumerable<CartItemDto> cartItems,
        EnderecoEntregaDto endereco,
        decimal frete = 0m,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId inválido.", nameof(userId));
        }

        var itensLista = cartItems?.ToList() ?? [];
        if (itensLista.Count == 0)
        {
            throw new ValidationException("Carrinho vazio.");
        }

        if (itensLista.Any(i => i.Quantidade <= 0))
        {
            throw new ValidationException("Quantidade inválida em um ou mais itens.");
        }

        var productIds = itensLista.Select(i => i.ProductId).Distinct().ToList();
        var products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new NotFoundException("Um ou mais produtos não foram encontrados.");
        }

        var productById = products.ToDictionary(p => p.Id);

        // Validação de estoque
        foreach (var item in itensLista)
        {
            var product = productById[item.ProductId];
            if (item.Quantidade > product.Estoque)
            {
                throw new ValidationException($"Quantidade acima do estoque para '{product.Nome}'. Disponível: {product.Estoque}.");
            }
        }

        // Validação de endereço obrigatório
        if (endereco == null)
        {
            throw new ArgumentNullException(nameof(endereco), "Endereço de entrega é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(endereco.Rua))
        {
            throw new ValidationException("O campo Rua do endereço de entrega é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(endereco.Cidade))
        {
            throw new ValidationException("O campo Cidade do endereço de entrega é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(endereco.Bairro))
        {
            throw new ValidationException("O campo Bairro do endereço de entrega é obrigatório.");
        }

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
            logger.LogError(ex, "Erro ao salvar pedido no banco de dados");
            throw new BusinessException("Erro ao processar pedido. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar pedido");
            throw;
        }

        logger.LogInformation("Pedido {OrderId} criado para usuário {UserId} com {Itens} itens.", order.Id, userId, itensLista.Count);

        return order.Id;
    }

    public async Task<IReadOnlyList<Order>> ListarPedidosPorUsuarioAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<Order>();
        }

        var pedidos = await dbContext.Orders
            .Where(o => o.ApplicationUserId == userId)
            .OrderByDescending(o => o.DataPedido)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return pedidos;
    }

    public async Task<IReadOnlyList<Order>> ListarTodosAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .OrderByDescending(o => o.DataPedido)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(o => o.Cliente)
            .Include(o => o.Itens)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<bool> AtualizarStatusAsync(
        int id,
        OrderStatus novoStatus,
        CancellationToken cancellationToken = default)
    {
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
}


