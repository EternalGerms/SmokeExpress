// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação padrão do serviço de categorias para as operações administrativas.
/// </summary>
public class CategoryService(ApplicationDbContext context) : ICategoryService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IReadOnlyCollection<Category>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Produtos)
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Produtos)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Category> CriarAsync(Category category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category;
    }

    public async Task AtualizarAsync(Category category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);

        var existente = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Categoria com Id {category.Id} não encontrada.");

        existente.Nome = category.Nome;
        existente.Descricao = category.Descricao;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var existente = await _context.Categories
            .Include(c => c.Produtos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Categoria com Id {id} não encontrada.");

        // Verificar se a categoria tem produtos associados
        if (existente.Produtos.Any())
        {
            throw new InvalidOperationException($"Não é possível excluir a categoria '{existente.Nome}' pois ela possui produtos associados.");
        }

        _context.Categories.Remove(existente);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

