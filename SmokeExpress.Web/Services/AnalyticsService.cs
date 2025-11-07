// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Models.Dashboard;

namespace SmokeExpress.Web.Services;

public class AnalyticsService(ApplicationDbContext dbContext, ILogger<AnalyticsService> logger) : IAnalyticsService
{
    public async Task<DashboardSummaryDto> ObterResumoAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default)
    {
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

        var pedidos = await queryPedidos.ToListAsync(ct);

        var receitaTotal = pedidos.Sum(o => o.TotalPedido);
        var totalPedidos = pedidos.Count;
        var mediaValorPedido = totalPedidos > 0 ? receitaTotal / totalPedidos : 0m;

        var clientesAtivos = await queryPedidos
            .Select(o => o.ApplicationUserId)
            .Distinct()
            .CountAsync(ct);

        var totalProdutos = await dbContext.Products.CountAsync(ct);
        var produtosSemEstoque = await dbContext.Products.CountAsync(p => p.Estoque <= 0, ct);

        return new DashboardSummaryDto
        {
            ReceitaTotal = receitaTotal,
            TotalPedidos = totalPedidos,
            MediaValorPedido = mediaValorPedido,
            TotalClientesAtivos = clientesAtivos,
            TotalProdutos = totalProdutos,
            TotalProdutosSemEstoque = produtosSemEstoque
        };
    }

    public async Task<IReadOnlyList<SalesByPeriodDto>> ObterVendasPorPeriodoAsync(PeriodFilter periodo, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default)
    {
        var (inicio, fim) = ObterDatasFiltro(dataInicio, dataFim, periodo);

        if (!inicio.HasValue || !fim.HasValue)
        {
            return new List<SalesByPeriodDto>();
        }

        var pedidos = await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Itens)
            .Where(o => o.DataPedido >= inicio.Value && o.DataPedido <= fim.Value)
            .ToListAsync(ct);

        var resultado = new List<SalesByPeriodDto>();

        if (periodo == PeriodFilter.Personalizado || periodo == PeriodFilter.Hoje)
        {
            // Agrupar por dia
            resultado = pedidos
                .GroupBy(o => o.DataPedido.Date)
                .Select(g => new SalesByPeriodDto
                {
                    Periodo = g.Key.ToString("dd/MM/yyyy"),
                    Receita = g.Sum(o => o.TotalPedido),
                    QuantidadePedidos = g.Count(),
                    QuantidadeItensVendidos = g.SelectMany(o => o.Itens).Sum(i => i.Quantidade)
                })
                .OrderBy(x => x.Periodo)
                .ToList();
        }
        else if (periodo == PeriodFilter.EstaSemana || periodo == PeriodFilter.EsteMes)
        {
            // Agrupar por dia
            resultado = pedidos
                .GroupBy(o => o.DataPedido.Date)
                .Select(g => new SalesByPeriodDto
                {
                    Periodo = g.Key.ToString("dd/MM/yyyy"),
                    Receita = g.Sum(o => o.TotalPedido),
                    QuantidadePedidos = g.Count(),
                    QuantidadeItensVendidos = g.SelectMany(o => o.Itens).Sum(i => i.Quantidade)
                })
                .OrderBy(x => x.Periodo)
                .ToList();
        }
        else if (periodo == PeriodFilter.EsteAno)
        {
            // Agrupar por mês
            resultado = pedidos
                .GroupBy(o => new { o.DataPedido.Year, o.DataPedido.Month })
                .Select(g => new SalesByPeriodDto
                {
                    Periodo = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MM/yyyy"),
                    Receita = g.Sum(o => o.TotalPedido),
                    QuantidadePedidos = g.Count(),
                    QuantidadeItensVendidos = g.SelectMany(o => o.Itens).Sum(i => i.Quantidade)
                })
                .OrderBy(x => x.Periodo)
                .ToList();
        }

        return resultado;
    }

    public async Task<IReadOnlyList<ProductSalesDto>> ObterProdutosMaisVendidosAsync(int top = 10, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default)
    {
        var (inicio, fim) = ObterDatasFiltro(dataInicio, dataFim);

        if (!inicio.HasValue || !fim.HasValue)
        {
            // Se não há filtro de data, buscar todos os produtos vendidos
            inicio = DateTime.MinValue;
            fim = DateTime.UtcNow;
        }

        var query = dbContext.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .Where(oi => oi.Order.DataPedido >= inicio.Value && oi.Order.DataPedido <= fim.Value);

        var produtosVendidos = await query
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

    public async Task<IReadOnlyList<Product>> ObterProdutosMenorEstoqueAsync(int top = 10, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Categoria)
            .OrderBy(p => p.Estoque)
            .ThenBy(p => p.Nome)
            .Take(top)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductRatingDto>> ObterProdutosMelhoresAvaliadosAsync(int top = 10, CancellationToken ct = default)
    {
        var produtosComAvaliacoes = await dbContext.Products
            .AsNoTracking()
            .Select(p => new ProductRatingDto
            {
                ProductId = p.Id,
                Nome = p.Nome,
                ImagemUrl = p.ImagemUrl,
                MediaAvaliacao = dbContext.Reviews
                    .Where(r => r.ProductId == p.Id)
                    .Select(r => (decimal?)r.Rating)
                    .DefaultIfEmpty()
                    .Average(),
                TotalAvaliacoes = dbContext.Reviews.Count(r => r.ProductId == p.Id)
            })
            .Where(p => p.MediaAvaliacao.HasValue && p.TotalAvaliacoes > 0)
            .OrderByDescending(p => p.MediaAvaliacao)
            .ThenByDescending(p => p.TotalAvaliacoes)
            .Take(top)
            .ToListAsync(ct);

        return produtosComAvaliacoes;
    }

    public async Task<IReadOnlyList<ProductRatingDto>> ObterProdutosPioresAvaliadosAsync(int top = 10, CancellationToken ct = default)
    {
        var produtosComAvaliacoes = await dbContext.Products
            .AsNoTracking()
            .Select(p => new ProductRatingDto
            {
                ProductId = p.Id,
                Nome = p.Nome,
                ImagemUrl = p.ImagemUrl,
                MediaAvaliacao = dbContext.Reviews
                    .Where(r => r.ProductId == p.Id)
                    .Select(r => (decimal?)r.Rating)
                    .DefaultIfEmpty()
                    .Average(),
                TotalAvaliacoes = dbContext.Reviews.Count(r => r.ProductId == p.Id)
            })
            .Where(p => p.MediaAvaliacao.HasValue && p.TotalAvaliacoes > 0)
            .OrderBy(p => p.MediaAvaliacao)
            .ThenBy(p => p.TotalAvaliacoes)
            .Take(top)
            .ToListAsync(ct);

        return produtosComAvaliacoes;
    }

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

    private (DateTime?, DateTime?) ObterDatasFiltro(DateTime? dataInicio = null, DateTime? dataFim = null, PeriodFilter? periodo = null)
    {
        DateTime? inicio = dataInicio;
        DateTime? fim = dataFim;

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
        else if (periodo == PeriodFilter.Personalizado)
        {
            // Se personalizado, usar as datas fornecidas
            // Se não especificou, usar último mês como padrão
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
        }
        else
        {
            // Se não especificou período nem datas, usar um período padrão (últimos 30 dias)
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
        }

        return (inicio, fim);
    }
}

