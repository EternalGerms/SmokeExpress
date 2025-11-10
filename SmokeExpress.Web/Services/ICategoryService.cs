// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Contrato para lidar com operações de categorias no contexto administrativo.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Lista todas as categorias cadastradas com os respectivos produtos.
    /// </summary>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Coleção somente leitura de categorias.</returns>
    Task<IReadOnlyCollection<Category>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma categoria específica com seus produtos relacionados.
    /// </summary>
    /// <param name="id">Identificador da categoria.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Categoria encontrada ou <c>null</c> quando inexistente.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="id"/> é menor ou igual a zero.</exception>
    Task<Category?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma nova categoria após validar os campos obrigatórios.
    /// </summary>
    /// <param name="category">Dados da categoria.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Categoria persistida.</returns>
    /// <exception cref="ArgumentNullException">Lançada quando <paramref name="category"/> é nula.</exception>
    /// <exception cref="ValidationException">Lançada quando dados obrigatórios estão ausentes.</exception>
    /// <exception cref="BusinessException">Lançada quando ocorre erro ao salvar a categoria.</exception>
    Task<Category> CriarAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza os dados de uma categoria existente.
    /// </summary>
    /// <param name="category">Dados atualizados da categoria.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Tarefa representando a operação assíncrona.</returns>
    /// <exception cref="ArgumentNullException">Lançada quando <paramref name="category"/> é nula.</exception>
    /// <exception cref="ValidationException">Lançada quando dados obrigatórios estão ausentes.</exception>
    /// <exception cref="NotFoundException">Lançada quando a categoria não é encontrada.</exception>
    /// <exception cref="BusinessException">Lançada quando ocorre erro ao salvar a alteração.</exception>
    Task AtualizarAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma categoria, respeitando vínculos com produtos.
    /// </summary>
    /// <param name="id">Identificador da categoria.</param>
    /// <param name="cancellationToken">Token opcional para cancelar a operação.</param>
    /// <returns>Tarefa representando a operação assíncrona.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando <paramref name="id"/> é menor ou igual a zero.</exception>
    /// <exception cref="ValidationException">Lançada quando existem produtos associados impedindo a exclusão.</exception>
    /// <exception cref="NotFoundException">Lançada quando a categoria não existe.</exception>
    /// <exception cref="BusinessException">Lançada quando ocorre erro ao remover a categoria.</exception>
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
}

