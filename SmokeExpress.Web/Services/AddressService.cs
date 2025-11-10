// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmokeExpress.Web.Common;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Resources;

namespace SmokeExpress.Web.Services;

// Convenção de logging: mensagens com propriedades nomeadas {Prop}; usar BeginScope para contexto (ex.: {UserId}, {AddressId}).

/// <summary>
/// Implementação de <see cref="IAddressService"/> baseada em Entity Framework Core.
/// </summary>
public class AddressService(ApplicationDbContext db, ILogger<AddressService> logger) : IAddressService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Address>> ListAsync(string userId, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(userId, nameof(userId));
        return await db.Addresses.AsNoTracking()
            .Where(a => a.ApplicationUserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Address> CreateAsync(string userId, Address address, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(userId, nameof(userId));
        Guard.AgainstNull(address, nameof(address));
        using var _ = logger.BeginScope(new { UserId = userId });
        var validacao = ValidarEndereco(address);
        if (!validacao.IsSuccess)
        {
            throw new ValidationException(validacao.ErrorMessage!);
        }

        address.Id = 0;
        address.ApplicationUserId = userId;

        if (address.IsDefault)
        {
            await UnsetDefault(userId, ct);
        }

        db.Addresses.Add(address);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao salvar endereço no banco de dados. UserId: {UserId}", userId);
            throw new BusinessException(ErrorMessages.ErrorCreatingAddress);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar endereço. UserId: {UserId}", userId);
            throw;
        }
        logger.LogInformation("Endereço salvo. AddressId: {AddressId}, UserId: {UserId}, IsDefault: {IsDefault}", address.Id, userId, address.IsDefault);
        return address;
    }

    /// <inheritdoc />
    public async Task<Address?> UpdateAsync(string userId, int id, Address address, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(userId, nameof(userId));
        Guard.AgainstNull(address, nameof(address));
        using var _ = logger.BeginScope(new { UserId = userId, AddressId = id });
        var validacao = ValidarEndereco(address);
        if (!validacao.IsSuccess)
        {
            throw new ValidationException(validacao.ErrorMessage!);
        }

        var existing = await db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.ApplicationUserId == userId, ct);
        if (existing is null) return null;

        existing.Rua = address.Rua;
        existing.Numero = address.Numero;
        existing.Cidade = address.Cidade;
        existing.Bairro = address.Bairro;
        existing.Complemento = address.Complemento;

        if (address.IsDefault && !existing.IsDefault)
        {
            await UnsetDefault(userId, ct);
            existing.IsDefault = true;
        }
        else if (!address.IsDefault && existing.IsDefault)
        {
            existing.IsDefault = false;
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao atualizar endereço {AddressId} no banco de dados", id);
            throw new BusinessException(ErrorMessages.ErrorUpdatingAddress);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar endereço {AddressId}", id);
            throw;
        }
        logger.LogInformation("Endereço atualizado. AddressId: {AddressId}, UserId: {UserId}, IsDefault: {IsDefault}", id, userId, existing.IsDefault);
        return existing;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string userId, int id, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(userId, nameof(userId));
        using var _ = logger.BeginScope(new { UserId = userId, AddressId = id });
        var existing = await db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.ApplicationUserId == userId, ct);
        if (existing is null) return false;

        var wasDefault = existing.IsDefault;
        db.Addresses.Remove(existing);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao remover endereço {AddressId} do banco de dados", id);
            throw new BusinessException(ErrorMessages.ErrorRemovingAddress);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover endereço {AddressId}", id);
            throw;
        }
        logger.LogInformation("Endereço removido. AddressId: {AddressId}, UserId: {UserId}", id, userId);

        if (wasDefault)
        {
            // Promove o primeiro endereço restante como padrão, se existir
            var first = await db.Addresses.Where(a => a.ApplicationUserId == userId)
                .OrderBy(a => a.Id).FirstOrDefaultAsync(ct);
            if (first != null)
            {
                first.IsDefault = true;
                try
                {
                    await db.SaveChangesAsync(ct);
                }
                catch (DbUpdateException ex)
                {
                    logger.LogError(ex, "Erro ao atualizar endereço padrão após remoção. UserId: {UserId}", userId);
                    // Não relançar exceção aqui, pois a remoção já foi bem-sucedida
                }
                logger.LogInformation("Endereço definido como padrão após remoção. AddressId: {AddressId}, UserId: {UserId}", first.Id, userId);
            }
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> MakeDefaultAsync(string userId, int id, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(userId, nameof(userId));
        using var _ = logger.BeginScope(new { UserId = userId, AddressId = id });
        var existing = await db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.ApplicationUserId == userId, ct);
        if (existing is null) return false;

        await UnsetDefault(userId, ct);
        existing.IsDefault = true;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro ao definir endereço {AddressId} como padrão no banco de dados", id);
            throw new BusinessException(ErrorMessages.ErrorSettingDefaultAddress);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao definir endereço {AddressId} como padrão", id);
            throw;
        }
        logger.LogInformation("Endereço definido como padrão. AddressId: {AddressId}, UserId: {UserId}", id, userId);
        return true;
    }

    private async Task UnsetDefault(string userId, CancellationToken ct)
    {
        await db.Addresses
            .Where(a => a.ApplicationUserId == userId && a.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);
    }

    private Result ValidarEndereco(Address? address)
    {
        if (address == null)
        {
            return Result.Failure(ErrorMessages.AddressCannotBeNull);
        }

        if (string.IsNullOrWhiteSpace(address.Rua))
        {
            return Result.Failure(ErrorMessages.AddressStreetRequiredSimple);
        }

        if (string.IsNullOrWhiteSpace(address.Cidade))
        {
            return Result.Failure(ErrorMessages.AddressCityRequiredSimple);
        }

        if (string.IsNullOrWhiteSpace(address.Bairro))
        {
            return Result.Failure(ErrorMessages.AddressNeighborhoodRequiredSimple);
        }

        return Result.Success();
    }
}


