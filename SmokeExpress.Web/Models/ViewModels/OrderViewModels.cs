// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Models.ViewModels;

/// <summary>
/// DTO para exibição de uma linha de pedido em listagens.
/// </summary>
public class OrderRowDto
{
    public int Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string DataLocal { get; set; } = string.Empty;
    public string DataPedidoLocal { get; set; } = string.Empty;
    public string StatusTexto { get; set; } = string.Empty;
    public string TotalTexto { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    
    // Propriedades para UI (Account/Orders.razor)
    public bool IsExpanded { get; set; }
    public bool CarregandoDetalhes { get; set; }
    public OrderDetailsDto? Detalhes { get; set; }
    public string? ErroDetalhes { get; set; }
}

/// <summary>
/// DTO para exibição de detalhes completos de um pedido.
/// </summary>
public class OrderDetailsDto
{
    public int Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string DataLocal { get; set; } = string.Empty;
    public string Rua { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Frete { get; set; }
    public decimal TotalPedido { get; set; }
    public List<OrderItemRowDto> ItensAdmin { get; set; } = new();
    public List<OrderItemDetailDto> Itens { get; set; } = new();
}

/// <summary>
/// DTO para exibição de um item de pedido na versão Admin.
/// </summary>
public class OrderItemRowDto
{
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}

/// <summary>
/// DTO para exibição de um item de pedido na versão Account.
/// </summary>
public class OrderItemDetailDto
{
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string? ImagemUrl { get; set; }
    public int ProductId { get; set; }
    public bool JaAvaliado { get; set; }
}

