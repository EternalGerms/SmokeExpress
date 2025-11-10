// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Constants;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Models.Dashboard;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação de <see cref="IAnalyticsService"/> que consolida métricas a partir do banco de dados.
/// </summary>
public class AnalyticsService(ApplicationDbContext dbContext) : IAnalyticsService
{
    /// <inheritdoc />
    public async Task<DashboardSummaryDto> ObterResumoAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default)
    {
        // datas opcionais, sem guard aqui
        var (inicio, fim) = ObterDatasFiltro(dataInicio, dataFim);

        var queryPedidos = dbContext.Orders.AsNoTracking();
        if (inicio.HasValue && fim.HasValue)
        {
            queryPedidos = queryPedidos.Where(o => o.DataPedido >= inicio.Value && o.DataPedido <= fim.Value);
        }
        else
        {
            // Se não há filtro, usar todos os pedidos
            inicio = DateTime.MinValue;
            fim = DateTime.UtcNow;
            queryPedidos = queryPedidos.Where(o => o.DataPedido >= inicio.Value && o.DataPedido <= fim.Value);
        }

        // Calcular agregados diretamente no banco
        var receitaTotal = await queryPedidos.SumAsync(o => o.TotalPedido, ct);
        var totalPedidos = await queryPedidos.CountAsync(ct);
        var mediaValorPedido = totalPedidos > 0 ? receitaTotal / totalPedidos : 0m;

        var clientesAtivos = await queryPedidos
            .Select(o => o.ApplicationUserId)
            .Distinct()
            .CountAsync(ct);

        // Combinar queries de produtos em uma única consulta usando projeção
        var produtosStats = await dbContext.Products
            .AsNoTracking()
            .GroupBy(p => 1)
            .Select(g => new
            {
                TotalProdutos = g.Count(),
                ProdutosSemEstoque = g.Count(p => p.Estoque <= 0)
            })
            .FirstOrDefaultAsync(ct);

        return new DashboardSummaryDto
        {
            ReceitaTotal = receitaTotal,
            TotalPedidos = totalPedidos,
            MediaValorPedido = mediaValorPedido,
            TotalClientesAtivos = clientesAtivos,
            TotalProdutos = produtosStats?.TotalProdutos ?? 0,
            TotalProdutosSemEstoque = produtosStats?.ProdutosSemEstoque ?? 0
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SalesByPeriodDto>> ObterVendasPorPeriodoAsync(PeriodFilter periodo, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default)
    {
        // período é enum, sem guard
        var (inicio, fim) = ObterDatasFiltro(dataInicio, dataFim, periodo);

        if (!inicio.HasValue || !fim.HasValue)
        {
            return new List<SalesByPeriodDto>();
        }

        // Consulta base de pedidos filtrados
        var queryPedidos = dbContext.Orders
            .AsNoTracking()
            .Where(o => o.DataPedido >= inicio.Value && o.DataPedido <= fim.Value);

        // Calcular vendas por período: agrupar Orders primeiro, depois JOIN com OrderItems
        if (periodo == PeriodFilter.EsteAno)
        {
            // Primeiro agrupar Orders por mês e materializar
            var pedidosAgrupados = await queryPedidos
                .GroupBy(o => new { o.DataPedido.Year, o.DataPedido.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Receita = g.Sum(o => o.TotalPedido),
                    QuantidadePedidos = g.Count()
                })
                .ToListAsync(ct);

            // Depois calcular quantidade de itens por mês e materializar
            var itensPorMes = await dbContext.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.DataPedido >= inicio.Value && oi.Order.DataPedido <= fim.Value)
                .GroupBy(oi => new { oi.Order.DataPedido.Year, oi.Order.DataPedido.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    QuantidadeItens = g.Sum(oi => oi.Quantidade)
                })
                .ToListAsync(ct);

            // Fazer JOIN em memória
            var dadosAgrupados = pedidosAgrupados
                .GroupJoin(
                    itensPorMes,
                    p => new { p.Year, p.Month },
                    i => new { i.Year, i.Month },
                    (p, itens) => new
                    {
                        Year = p.Year,
                        Month = p.Month,
                        Receita = p.Receita,
                        QuantidadePedidos = p.QuantidadePedidos,
                        QuantidadeItensVendidos = itens.Select(i => i.QuantidadeItens).FirstOrDefault()
                    })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            return dadosAgrupados
                .Select(x => new SalesByPeriodDto
                {
                    Periodo = new DateTime(x.Year, x.Month, 1, 0, 0, 0, DateTimeKind.Utc).ToString("MM/yyyy"),
                    Receita = x.Receita,
                    QuantidadePedidos = x.QuantidadePedidos,
                    QuantidadeItensVendidos = x.QuantidadeItensVendidos
                })
                .ToList();
        }
        else
        {
            // Primeiro agrupar Orders por dia e materializar
            var pedidosAgrupados = await queryPedidos
                .GroupBy(o => o.DataPedido.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    Receita = g.Sum(o => o.TotalPedido),
                    QuantidadePedidos = g.Count()
                })
                .ToListAsync(ct);

            // Depois calcular quantidade de itens por dia e materializar
            var itensPorDia = await dbContext.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.DataPedido >= inicio.Value && oi.Order.DataPedido <= fim.Value)
                .GroupBy(oi => oi.Order.DataPedido.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    QuantidadeItens = g.Sum(oi => oi.Quantidade)
                })
                .ToListAsync(ct);

            // Fazer JOIN em memória
            var dadosAgrupados = pedidosAgrupados
                .GroupJoin(
                    itensPorDia,
                    p => p.Data,
                    i => i.Data,
                    (p, itens) => new
                    {
                        Data = p.Data,
                        Receita = p.Receita,
                        QuantidadePedidos = p.QuantidadePedidos,
                        QuantidadeItensVendidos = itens.Select(i => i.QuantidadeItens).FirstOrDefault()
                    })
                .OrderBy(x => x.Data)
                .ToList();

            return dadosAgrupados
                .Select(x => new SalesByPeriodDto
                {
                    Periodo = x.Data.ToString("dd/MM/yyyy"),
                    Receita = x.Receita,
                    QuantidadePedidos = x.QuantidadePedidos,
                    QuantidadeItensVendidos = x.QuantidadeItensVendidos
                })
                .ToList();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductSalesDto>> ObterProdutosMaisVendidosAsync(int top = ApplicationConstants.DefaultTopItems, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default)
    {
        if (top <= 0) throw new ArgumentOutOfRangeException(nameof(top));
        var (inicio, fim) = ObterDatasFiltro(dataInicio, dataFim);

        if (!inicio.HasValue || !fim.HasValue)
        {
            // Se não há filtro de data, buscar todos os produtos vendidos
            inicio = DateTime.MinValue;
            fim = DateTime.UtcNow;
        }

        var produtosVendidos = await dbContext.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Order.DataPedido >= inicio.Value && oi.Order.DataPedido <= fim.Value)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Nome, oi.Product.ImagemUrl })
            .Select(g => new ProductSalesDto
            {
                ProductId = g.Key.ProductId,
                Nome = g.Key.Nome,
                QuantidadeVendida = g.Sum(oi => oi.Quantidade),
                Receita = g.Sum(oi => oi.Quantidade * oi.PrecoUnitario),
                ImagemUrl = g.Key.ImagemUrl
            })
            .OrderByDescending(p => p.QuantidadeVendida)
            .Take(top)
            .ToListAsync(ct);

        return produtosVendidos;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> ObterProdutosMenorEstoqueAsync(int top = ApplicationConstants.DefaultTopItems, CancellationToken ct = default)
    {
        if (top <= 0) throw new ArgumentOutOfRangeException(nameof(top));
        return await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Categoria)
            .OrderBy(p => p.Estoque)
            .ThenBy(p => p.Nome)
            .Take(top)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductRatingDto>> ObterProdutosMelhoresAvaliadosAsync(int top = ApplicationConstants.DefaultTopItems, CancellationToken ct = default)
    {
        if (top <= 0) throw new ArgumentOutOfRangeException(nameof(top));
        // Agrupar Reviews primeiro para calcular agregações uma única vez e materializar
        var avaliacoesAgrupadas = await dbContext.Reviews
            .AsNoTracking()
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                MediaAvaliacao = (decimal?)g.Average(r => (decimal)r.Rating),
                TotalAvaliacoes = g.Count()
            })
            .Where(a => a.MediaAvaliacao.HasValue && a.TotalAvaliacoes > 0)
            .ToListAsync(ct);

        if (avaliacoesAgrupadas.Count == 0)
        {
            return new List<ProductRatingDto>();
        }

        // Buscar apenas os produtos que têm avaliações
        var productIds = avaliacoesAgrupadas.Select(a => a.ProductId).ToList();
        var produtos = await dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);

        // Fazer JOIN em memória e ordenar
        var produtosComAvaliacoes = produtos
            .Join(
                avaliacoesAgrupadas,
                produto => produto.Id,
                avaliacao => avaliacao.ProductId,
                (produto, avaliacao) => new ProductRatingDto
                {
                    ProductId = produto.Id,
                    Nome = produto.Nome,
                    ImagemUrl = produto.ImagemUrl,
                    MediaAvaliacao = avaliacao.MediaAvaliacao,
                    TotalAvaliacoes = avaliacao.TotalAvaliacoes
                })
            .OrderByDescending(p => p.MediaAvaliacao)
            .ThenByDescending(p => p.TotalAvaliacoes)
            .Take(top)
            .ToList();

        return produtosComAvaliacoes;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductRatingDto>> ObterProdutosPioresAvaliadosAsync(int top = ApplicationConstants.DefaultTopItems, CancellationToken ct = default)
    {
        if (top <= 0) throw new ArgumentOutOfRangeException(nameof(top));
        // Agrupar Reviews primeiro para calcular agregações uma única vez e materializar
        var avaliacoesAgrupadas = await dbContext.Reviews
            .AsNoTracking()
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                MediaAvaliacao = (decimal?)g.Average(r => (decimal)r.Rating),
                TotalAvaliacoes = g.Count()
            })
            .Where(a => a.MediaAvaliacao.HasValue && a.TotalAvaliacoes > 0)
            .ToListAsync(ct);

        if (avaliacoesAgrupadas.Count == 0)
        {
            return new List<ProductRatingDto>();
        }

        // Buscar apenas os produtos que têm avaliações
        var productIds = avaliacoesAgrupadas.Select(a => a.ProductId).ToList();
        var produtos = await dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);

        // Fazer JOIN em memória e ordenar
        var produtosComAvaliacoes = produtos
            .Join(
                avaliacoesAgrupadas,
                produto => produto.Id,
                avaliacao => avaliacao.ProductId,
                (produto, avaliacao) => new ProductRatingDto
                {
                    ProductId = produto.Id,
                    Nome = produto.Nome,
                    ImagemUrl = produto.ImagemUrl,
                    MediaAvaliacao = avaliacao.MediaAvaliacao,
                    TotalAvaliacoes = avaliacao.TotalAvaliacoes
                })
            .OrderBy(p => p.MediaAvaliacao)
            .ThenBy(p => p.TotalAvaliacoes)
            .Take(top)
            .ToList();

        return produtosComAvaliacoes;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderStatusCountDto>> ObterPedidosPorStatusAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default)
    {
        var (inicio, fim) = ObterDatasFiltro(dataInicio, dataFim);

        var query = dbContext.Orders.AsNoTracking();
        if (inicio.HasValue && fim.HasValue)
        {
            query = query.Where(o => o.DataPedido >= inicio.Value && o.DataPedido <= fim.Value);
        }

        var pedidosAgrupados = await query
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Quantidade = g.Count(), TotalReceita = g.Sum(o => o.TotalPedido) })
            .ToListAsync(ct);

        var pedidosPorStatus = pedidosAgrupados.Select(g => new OrderStatusCountDto
        {
            Status = g.Status,
            StatusTexto = g.Status switch
            {
                OrderStatus.Processando => "Processando",
                OrderStatus.Confirmado => "Confirmado",
                OrderStatus.EmPreparacao => "Em Preparação",
                OrderStatus.Enviado => "Enviado",
                OrderStatus.Entregue => "Entregue",
                OrderStatus.Cancelado => "Cancelado",
                _ => g.Status.ToString()
            },
            Quantidade = g.Quantidade,
            TotalReceita = g.TotalReceita
        }).ToList();

        return pedidosPorStatus;
    }

    /// <inheritdoc />
    public async Task<DashboardAnalyticsDto> ObterAnalyticsCompletoAsync(PeriodFilter periodo, int topProdutos = ApplicationConstants.DefaultTopItems, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default)
    {
        var (inicio, fim) = ObterDatasFiltro(dataInicio, dataFim, periodo);

        if (!inicio.HasValue || !fim.HasValue)
        {
            return new DashboardAnalyticsDto
            {
                Resumo = new DashboardSummaryDto(),
                PedidosPorStatus = new List<OrderStatusCountDto>(),
                VendasPorPeriodo = new List<SalesByPeriodDto>(),
                ProdutosMaisVendidos = new List<ProductSalesDto>()
            };
        }

        // Consulta base de pedidos filtrados (reutilizada para múltiplas agregações)
        var queryPedidos = dbContext.Orders
            .AsNoTracking()
            .Where(o => o.DataPedido >= inicio.Value && o.DataPedido <= fim.Value);

        // 1. Calcular resumo (agregados no banco)
        var receitaTotal = await queryPedidos.SumAsync(o => o.TotalPedido, ct);
        var totalPedidos = await queryPedidos.CountAsync(ct);
        var mediaValorPedido = totalPedidos > 0 ? receitaTotal / totalPedidos : 0m;
        var clientesAtivos = await queryPedidos
            .Select(o => o.ApplicationUserId)
            .Distinct()
            .CountAsync(ct);

        // 2. Pedidos por status (agrupamento no banco)
        var pedidosAgrupadosPorStatus = await queryPedidos
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Quantidade = g.Count(), TotalReceita = g.Sum(o => o.TotalPedido) })
            .ToListAsync(ct);

        var pedidosPorStatus = pedidosAgrupadosPorStatus.Select(g => new OrderStatusCountDto
        {
            Status = g.Status,
            StatusTexto = g.Status switch
            {
                OrderStatus.Processando => "Processando",
                OrderStatus.Confirmado => "Confirmado",
                OrderStatus.EmPreparacao => "Em Preparação",
                OrderStatus.Enviado => "Enviado",
                OrderStatus.Entregue => "Entregue",
                OrderStatus.Cancelado => "Cancelado",
                _ => g.Status.ToString()
            },
            Quantidade = g.Quantidade,
            TotalReceita = g.TotalReceita
        }).ToList();

        // 3. Vendas por período (agrupamento no banco com JOIN otimizado)
        List<SalesByPeriodDto> vendasPorPeriodo;
        if (periodo == PeriodFilter.EsteAno)
        {
            // Primeiro agrupar Orders por mês e materializar
            var pedidosAgrupados = await queryPedidos
                .GroupBy(o => new { o.DataPedido.Year, o.DataPedido.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Receita = g.Sum(o => o.TotalPedido),
                    QuantidadePedidos = g.Count()
                })
                .ToListAsync(ct);

            // Depois calcular quantidade de itens por mês e materializar
            var itensPorMes = await dbContext.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.DataPedido >= inicio.Value && oi.Order.DataPedido <= fim.Value)
                .GroupBy(oi => new { oi.Order.DataPedido.Year, oi.Order.DataPedido.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    QuantidadeItens = g.Sum(oi => oi.Quantidade)
                })
                .ToListAsync(ct);

            // Fazer JOIN em memória
            var dadosAgrupados = pedidosAgrupados
                .GroupJoin(
                    itensPorMes,
                    p => new { p.Year, p.Month },
                    i => new { i.Year, i.Month },
                    (p, itens) => new
                    {
                        Year = p.Year,
                        Month = p.Month,
                        Receita = p.Receita,
                        QuantidadePedidos = p.QuantidadePedidos,
                        QuantidadeItensVendidos = itens.Select(i => i.QuantidadeItens).FirstOrDefault()
                    })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            vendasPorPeriodo = dadosAgrupados
                .Select(x => new SalesByPeriodDto
                {
                    Periodo = new DateTime(x.Year, x.Month, 1, 0, 0, 0, DateTimeKind.Utc).ToString("MM/yyyy"),
                    Receita = x.Receita,
                    QuantidadePedidos = x.QuantidadePedidos,
                    QuantidadeItensVendidos = x.QuantidadeItensVendidos
                })
                .ToList();
        }
        else
        {
            // Primeiro agrupar Orders por dia e materializar
            var pedidosAgrupados = await queryPedidos
                .GroupBy(o => o.DataPedido.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    Receita = g.Sum(o => o.TotalPedido),
                    QuantidadePedidos = g.Count()
                })
                .ToListAsync(ct);

            // Depois calcular quantidade de itens por dia e materializar
            var itensPorDia = await dbContext.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.DataPedido >= inicio.Value && oi.Order.DataPedido <= fim.Value)
                .GroupBy(oi => oi.Order.DataPedido.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    QuantidadeItens = g.Sum(oi => oi.Quantidade)
                })
                .ToListAsync(ct);

            // Fazer JOIN em memória
            var dadosAgrupados = pedidosAgrupados
                .GroupJoin(
                    itensPorDia,
                    p => p.Data,
                    i => i.Data,
                    (p, itens) => new
                    {
                        Data = p.Data,
                        Receita = p.Receita,
                        QuantidadePedidos = p.QuantidadePedidos,
                        QuantidadeItensVendidos = itens.Select(i => i.QuantidadeItens).FirstOrDefault()
                    })
                .OrderBy(x => x.Data)
                .ToList();

            vendasPorPeriodo = dadosAgrupados
                .Select(x => new SalesByPeriodDto
                {
                    Periodo = x.Data.ToString("dd/MM/yyyy"),
                    Receita = x.Receita,
                    QuantidadePedidos = x.QuantidadePedidos,
                    QuantidadeItensVendidos = x.QuantidadeItensVendidos
                })
                .ToList();
        }

        // 4. Produtos mais vendidos (agrupamento no banco sem Include)
        var produtosMaisVendidos = await dbContext.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Order.DataPedido >= inicio.Value && oi.Order.DataPedido <= fim.Value)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Nome, oi.Product.ImagemUrl })
            .Select(g => new ProductSalesDto
            {
                ProductId = g.Key.ProductId,
                Nome = g.Key.Nome,
                QuantidadeVendida = g.Sum(oi => oi.Quantidade),
                Receita = g.Sum(oi => oi.Quantidade * oi.PrecoUnitario),
                ImagemUrl = g.Key.ImagemUrl
            })
            .OrderByDescending(p => p.QuantidadeVendida)
            .Take(topProdutos)
            .ToListAsync(ct);

        // 5. Estatísticas de produtos (total e sem estoque)
        var produtosStats = await dbContext.Products
            .AsNoTracking()
            .GroupBy(p => 1)
            .Select(g => new
            {
                TotalProdutos = g.Count(),
                ProdutosSemEstoque = g.Count(p => p.Estoque <= 0)
            })
            .FirstOrDefaultAsync(ct);

        var resumo = new DashboardSummaryDto
        {
            ReceitaTotal = receitaTotal,
            TotalPedidos = totalPedidos,
            MediaValorPedido = mediaValorPedido,
            TotalClientesAtivos = clientesAtivos,
            TotalProdutos = produtosStats?.TotalProdutos ?? 0,
            TotalProdutosSemEstoque = produtosStats?.ProdutosSemEstoque ?? 0
        };

        return new DashboardAnalyticsDto
        {
            Resumo = resumo,
            PedidosPorStatus = pedidosPorStatus,
            VendasPorPeriodo = vendasPorPeriodo,
            ProdutosMaisVendidos = produtosMaisVendidos
        };
    }

    private (DateTime?, DateTime?) ObterDatasFiltro(DateTime? dataInicio = null, DateTime? dataFim = null, PeriodFilter? periodo = null)
    {
        // 1. Definir valores padrão (aplicar preenchimento de nulos)
        DateTime? inicio = dataInicio;
        DateTime? fim = dataFim;

        // Preencher valores nulos com padrão (últimos 30 dias)
        if (!inicio.HasValue && !fim.HasValue)
        {
            var agora = DateTime.UtcNow;
            fim = agora;
            inicio = agora.AddDays(-30);
        }
        else if (!fim.HasValue)
        {
            fim = DateTime.UtcNow;
        }
        else if (!inicio.HasValue)
        {
            inicio = fim.Value.AddDays(-30);
        }

        // 2. Se período é um preset, sobrescrever com valores do preset
        if (periodo.HasValue && periodo.Value != PeriodFilter.Personalizado)
        {
            var agora = DateTime.UtcNow;
            switch (periodo.Value)
            {
                case PeriodFilter.Hoje:
                    inicio = agora.Date;
                    fim = agora.Date.AddDays(1).AddTicks(-1);
                    break;
                case PeriodFilter.EstaSemana:
                    var diasDaSemana = (int)agora.DayOfWeek;
                    inicio = agora.Date.AddDays(-diasDaSemana);
                    fim = agora.Date.AddDays(1).AddTicks(-1);
                    break;
                case PeriodFilter.EsteMes:
                    inicio = new DateTime(agora.Year, agora.Month, 1);
                    fim = new DateTime(agora.Year, agora.Month, DateTime.DaysInMonth(agora.Year, agora.Month), 23, 59, 59);
                    break;
                case PeriodFilter.EsteAno:
                    inicio = new DateTime(agora.Year, 1, 1);
                    fim = new DateTime(agora.Year, 12, 31, 23, 59, 59);
                    break;
            }
        }

        // 3. Validar datas personalizadas (se aplicável)
        if (periodo == PeriodFilter.Personalizado)
        {
            // Garantir que inicio <= fim
            if (inicio.HasValue && fim.HasValue && inicio.Value > fim.Value)
            {
                (inicio, fim) = (fim, inicio); // Trocar se invertido
            }
        }

        return (inicio, fim);
    }
}

