// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Exceptions;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

public class AddressService(ApplicationDbContext db, ILogger<AddressService> logger) : IAddressService
{
    public async Task<IReadOnlyList<Address>> ListAsync(string userId, CancellationToken ct = default)
    {
        return await db.Addresses.AsNoTracking()
            .Where(a => a.ApplicationUserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Id)
            .ToListAsync(ct);
    }

    public async Task<Address> CreateAsync(string userId, Address address, CancellationToken ct = default)
    {
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
            logger.LogError(ex, "Erro ao salvar endereço no banco de dados");
            throw new BusinessException("Erro ao criar endereço. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar endereço");
            throw;
        }
        return address;
    }

    public async Task<Address?> UpdateAsync(string userId, int id, Address address, CancellationToken ct = default)
    {
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
            throw new BusinessException("Erro ao atualizar endereço. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao atualizar endereço {AddressId}", id);
            throw;
        }
        return existing;
    }

    public async Task<bool> DeleteAsync(string userId, int id, CancellationToken ct = default)
    {
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
            throw new BusinessException("Erro ao remover endereço. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao remover endereço {AddressId}", id);
            throw;
        }

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
                    logger.LogError(ex, "Erro ao atualizar endereço padrão após remoção");
                    // Não relançar exceção aqui, pois a remoção já foi bem-sucedida
                }
            }
        }

        return true;
    }

    public async Task<bool> MakeDefaultAsync(string userId, int id, CancellationToken ct = default)
    {
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
            throw new BusinessException("Erro ao definir endereço como padrão. Tente novamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao definir endereço {AddressId} como padrão", id);
            throw;
        }
        return true;
    }

    private async Task UnsetDefault(string userId, CancellationToken ct)
    {
        await db.Addresses
            .Where(a => a.ApplicationUserId == userId && a.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);
    }
}


