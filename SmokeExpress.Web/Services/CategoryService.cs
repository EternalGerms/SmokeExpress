// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmokeExpress.Web.Common;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Resources;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Implementação padrão do serviço de categorias para as operações administrativas.
/// </summary>
public class CategoryService(ApplicationDbContext context, ILogger<CategoryService> logger) : ICategoryService
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Category>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Produtos)
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Category?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        return await _context.Categories
            .Include(c => c.Produtos)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Category> CriarAsync(Category category, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(category, nameof(category));
        var validacao = ValidarCategoria(category);
        if (!validacao.IsSuccess)
        {
            throw new ValidationException(validacao.ErrorMessage!);
        }

        _context.Categories.Add(category);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao salvar categoria no banco de dados");
            throw new BusinessException(ErrorMessages.ErrorCreatingCategory);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar categoria");
            throw;
        }

        return category;
    }

    /// <inheritdoc />
    public async Task AtualizarAsync(Category category, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(category, nameof(category));
        var validacao = ValidarCategoria(category);
        if (!validacao.IsSuccess)
        {
            throw new ValidationException(validacao.ErrorMessage!);
        }

        var existente = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);
        
        if (existente == null)
        {
            throw new NotFoundException("Categoria", category.Id);
        }

        existente.Nome = category.Nome;
        existente.Descricao = category.Descricao;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao atualizar categoria {CategoryId} no banco de dados", category.Id);
            throw new BusinessException(ErrorMessages.ErrorUpdatingCategory);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar categoria {CategoryId}", category.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        var validacao = await ValidarRemocaoAsync(id, cancellationToken);
        if (!validacao.IsSuccess)
        {
            if (validacao.ErrorMessage!.Contains("não encontrada"))
            {
                throw new NotFoundException("Categoria", id);
            }
            throw new ValidationException(validacao.ErrorMessage);
        }

        var existente = await _context.Categories
            .Include(c => c.Produtos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (existente == null)
        {
            // Não deveria acontecer pois já validamos, mas garantimos segurança
            throw new NotFoundException("Categoria", id);
        }

        _context.Categories.Remove(existente);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao remover categoria {CategoryId} do banco de dados", id);
            throw new BusinessException(ErrorMessages.ErrorRemovingCategory);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover categoria {CategoryId}", id);
            throw;
        }
    }

    private Result ValidarCategoria(Category? category)
    {
        if (category == null)
        {
            return Result.Failure(ErrorMessages.CategoryCannotBeNull);
        }

        if (string.IsNullOrWhiteSpace(category.Nome))
        {
            return Result.Failure(ErrorMessages.CategoryNameRequired);
        }

        return Result.Success();
    }

    private async Task<Result> ValidarRemocaoAsync(int id, CancellationToken cancellationToken)
    {
        var existente = await _context.Categories
            .Include(c => c.Produtos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (existente == null)
        {
            return Result.Failure(string.Format(ErrorMessages.CategoryWithIdNotFound, id));
        }

        if (existente.Produtos.Any())
        {
            return Result.Failure(string.Format(ErrorMessages.CannotDeleteCategoryWithProducts, existente.Nome));
        }

        return Result.Success();
    }
}

