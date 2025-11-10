// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação padrão do serviço de categorias para as operações administrativas.
/// </summary>
public class CategoryService(ApplicationDbContext context, ILogger<CategoryService> logger) : ICategoryService
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
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao salvar categoria no banco de dados");
            throw new BusinessException("Erro ao criar categoria. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar categoria");
            throw;
        }

        return category;
    }

    public async Task AtualizarAsync(Category category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);

        var existente = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken)
            ?? throw new NotFoundException("Categoria", category.Id);

        existente.Nome = category.Nome;
        existente.Descricao = category.Descricao;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao atualizar categoria {CategoryId} no banco de dados", category.Id);
            throw new BusinessException("Erro ao atualizar categoria. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar categoria {CategoryId}", category.Id);
            throw;
        }
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var existente = await _context.Categories
            .Include(c => c.Produtos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Categoria", id);

        // Verificar se a categoria tem produtos associados
        if (existente.Produtos.Any())
        {
            throw new ValidationException($"Não é possível excluir a categoria '{existente.Nome}' pois ela possui produtos associados.");
        }

        _context.Categories.Remove(existente);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao remover categoria {CategoryId} do banco de dados", id);
            throw new BusinessException("Erro ao remover categoria. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover categoria {CategoryId}", id);
            throw;
        }
    }
}

