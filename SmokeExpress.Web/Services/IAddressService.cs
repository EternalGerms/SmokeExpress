// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

/// <summary>
/// Define operações para gerenciamento de endereços de entrega dos usuários.
/// </summary>
public interface IAddressService
{
    /// <summary>
    /// Lista todos os endereços cadastrados por um usuário, priorizando o endereço padrão.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Lista somente com os endereços do usuário.</returns>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="userId"/> é vazio.</exception>
    Task<IReadOnlyList<Address>> ListAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Cadastra um novo endereço para o usuário informado.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="address">Entidade contendo os dados do endereço.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Endereço persistido com seus identificadores.</returns>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="userId"/> é vazio.</exception>
    /// <exception cref="ArgumentNullException">Lançada quando <paramref name="address"/> é nulo.</exception>
    /// <exception cref="ValidationException">Lançada quando algum campo obrigatório está ausente.</exception>
    /// <exception cref="BusinessException">Lançada quando ocorre erro ao salvar o endereço.</exception>
    Task<Address> CreateAsync(string userId, Address address, CancellationToken ct = default);

    /// <summary>
    /// Atualiza um endereço existente pertencente ao usuário.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="id">Identificador do endereço.</param>
    /// <param name="address">Novos dados do endereço.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns>Endereço atualizado ou <c>null</c> quando não encontrado.</returns>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="userId"/> é vazio.</exception>
    /// <exception cref="ArgumentNullException">Lançada quando <paramref name="address"/> é nulo.</exception>
    /// <exception cref="ValidationException">Lançada quando os dados informados são inválidos.</exception>
    /// <exception cref="BusinessException">Lançada quando ocorre erro ao salvar a alteração.</exception>
    Task<Address?> UpdateAsync(string userId, int id, Address address, CancellationToken ct = default);

    /// <summary>
    /// Remove um endereço do usuário.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="id">Identificador do endereço.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns><c>true</c> quando o endereço foi removido; caso contrário, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="userId"/> é vazio.</exception>
    /// <exception cref="BusinessException">Lançada quando ocorre erro ao persistir a remoção.</exception>
    Task<bool> DeleteAsync(string userId, int id, CancellationToken ct = default);

    /// <summary>
    /// Define um endereço do usuário como padrão para entregas futuras.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="id">Identificador do endereço.</param>
    /// <param name="ct">Token opcional para cancelar a operação.</param>
    /// <returns><c>true</c> quando o endereço foi promovido para padrão.</returns>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="userId"/> é vazio.</exception>
    /// <exception cref="BusinessException">Lançada quando ocorre erro ao salvar a alteração.</exception>
    Task<bool> MakeDefaultAsync(string userId, int id, CancellationToken ct = default);
}


