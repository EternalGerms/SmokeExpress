// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação padrão do serviço de categorias para as operações administrativas.
/// </summary>
public class CategoryService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : ICategoryService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory = dbContextFactory;

    public async Task<IReadOnlyCollection<Category>> ListarAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Categories
            .Include(c => c.Produtos)
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Categories
            .Include(c => c.Produtos)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Category> CriarAsync(Category category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);

        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        return category;
    }

    public async Task AtualizarAsync(Category category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);

        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existente = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Categoria com Id {category.Id} não encontrada.");

        existente.Nome = category.Nome;
        existente.Descricao = category.Descricao;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existente = await context.Categories
            .Include(c => c.Produtos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Categoria com Id {id} não encontrada.");

        // Verificar se a categoria tem produtos associados
        if (existente.Produtos.Any())
        {
            throw new InvalidOperationException($"Não é possível excluir a categoria '{existente.Nome}' pois ela possui produtos associados.");
        }

        context.Categories.Remove(existente);
        await context.SaveChangesAsync(cancellationToken);
    }
}

