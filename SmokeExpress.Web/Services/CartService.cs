// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação padrão do carrinho de compras utilizando armazenamento em memória.
/// Como o serviço é Scoped em Blazor Server, cada usuário tem sua própria instância.
/// </summary>
public class CartService : ICartService
{
    private readonly List<CartEntry> _items = new();
    private readonly object _syncRoot = new();

    public Task AddAsync(Product product, int quantity = 1, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser maior ou igual a 1.");
        }

        lock (_syncRoot)
        {
            var entry = _items.FirstOrDefault(item => item.ProductId == product.Id);

            if (entry is null)
            {
                _items.Add(new CartEntry
                {
                    ProductId = product.Id,
                    Nome = product.Nome,
                    Quantidade = quantity,
                    PrecoUnitario = product.Preco,
                    ImagemUrl = product.ImagemUrl
                });
            }
            else
            {
                entry.Quantidade += quantity;
                entry.Nome = product.Nome;
                entry.PrecoUnitario = product.Preco;
                entry.ImagemUrl = product.ImagemUrl;
            }
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(int productId, CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            var entry = _items.FirstOrDefault(item => item.ProductId == productId);
            if (entry is not null)
            {
                _items.Remove(entry);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<CartItemViewModel>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<CartItemViewModel> items;

        lock (_syncRoot)
        {
            items = _items
                .Select(item => new CartItemViewModel
                {
                    ProductId = item.ProductId,
                    Nome = item.Nome,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario,
                    Subtotal = item.PrecoUnitario * item.Quantidade,
                    ImagemUrl = item.ImagemUrl
                })
                .ToList()
                .AsReadOnly();
        }

        return Task.FromResult(items);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            _items.Clear();
        }

        return Task.CompletedTask;
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        int total;

        lock (_syncRoot)
        {
            total = _items.Sum(item => item.Quantidade);
        }

        return Task.FromResult(total);
    }

    private sealed class CartEntry
    {
        public int ProductId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public string? ImagemUrl { get; set; }
    }
}

