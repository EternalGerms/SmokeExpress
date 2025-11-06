// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;

namespace SmokeExpress.Web.Services;

public interface IAddressService
{
    Task<IReadOnlyList<Address>> ListAsync(string userId, CancellationToken ct = default);
    Task<Address> CreateAsync(string userId, Address address, CancellationToken ct = default);
    Task<Address?> UpdateAsync(string userId, int id, Address address, CancellationToken ct = default);
    Task<bool> DeleteAsync(string userId, int id, CancellationToken ct = default);
    Task<bool> MakeDefaultAsync(string userId, int id, CancellationToken ct = default);
}


