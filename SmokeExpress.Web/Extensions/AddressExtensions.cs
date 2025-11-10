// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using SmokeExpress.Web.Models;
using SmokeExpress.Web.Models.ViewModels;

namespace SmokeExpress.Web.Extensions;

/// <summary>
/// Extension methods para mapeamento de entidades Address para DTOs.
/// </summary>
public static class AddressExtensions
{
    /// <summary>
    /// Converte um Address para AddressDto.
    /// </summary>
    public static AddressDto ToAddressDto(this Address address)
    {
        return new AddressDto
        {
            Id = address.Id,
            Rua = address.Rua,
            Numero = address.Numero,
            Cidade = address.Cidade,
            Bairro = address.Bairro,
            Complemento = address.Complemento,
            IsDefault = address.IsDefault
        };
    }

    /// <summary>
    /// Converte um AddressDto para Address (entidade).
    /// </summary>
    public static Address ToEntity(this AddressDto dto)
    {
        return new Address
        {
            Id = dto.Id,
            Rua = dto.Rua,
            Numero = dto.Numero,
            Cidade = dto.Cidade,
            Bairro = dto.Bairro,
            Complemento = dto.Complemento,
            IsDefault = dto.IsDefault
        };
    }
}

