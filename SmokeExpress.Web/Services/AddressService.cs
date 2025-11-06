// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.EntityFrameworkCore;
using SmokeExpress.Web.Data;
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

public class AddressService(ApplicationDbContext db) : IAddressService
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
        await db.SaveChangesAsync(ct);
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

        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(string userId, int id, CancellationToken ct = default)
    {
        var existing = await db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.ApplicationUserId == userId, ct);
        if (existing is null) return false;

        var wasDefault = existing.IsDefault;
        db.Addresses.Remove(existing);
        await db.SaveChangesAsync(ct);

        if (wasDefault)
        {
            // Promove o primeiro endereço restante como padrão, se existir
            var first = await db.Addresses.Where(a => a.ApplicationUserId == userId)
                .OrderBy(a => a.Id).FirstOrDefaultAsync(ct);
            if (first != null)
            {
                first.IsDefault = true;
                await db.SaveChangesAsync(ct);
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
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task UnsetDefault(string userId, CancellationToken ct)
    {
        await db.Addresses
            .Where(a => a.ApplicationUserId == userId && a.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);
    }
}


