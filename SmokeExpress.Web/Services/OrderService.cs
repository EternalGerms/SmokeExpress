// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

public class OrderService(ApplicationDbContext dbContext, ILogger<OrderService> logger) : IOrderService
{
    public async Task<int> CriarPedidoAsync(
        string userId,
        IEnumerable<CartItemDto> cartItems,
        EnderecoEntregaDto endereco,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId inválido.", nameof(userId));
        }

        var itensLista = cartItems?.ToList() ?? [];
        if (itensLista.Count == 0)
        {
            throw new InvalidOperationException("Carrinho vazio.");
        }

        if (itensLista.Any(i => i.Quantidade <= 0))
        {
            throw new InvalidOperationException("Quantidade inválida em um ou mais itens.");
        }

        var productIds = itensLista.Select(i => i.ProductId).Distinct().ToList();
        var products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new KeyNotFoundException("Um ou mais produtos não foram encontrados.");
        }

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
        }

        order.TotalPedido = total;

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Pedido {OrderId} criado para usuário {UserId} com {Itens} itens.", order.Id, userId, itensLista.Count);

        return order.Id;
    }
}


