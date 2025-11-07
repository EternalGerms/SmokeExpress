// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
using Microsoft.AspNetCore.Identity;
using SmokeExpress.Web.Models;

using BCryptNet = BCrypt.Net.BCrypt;

namespace SmokeExpress.Web.Security;

/// <summary>
/// Implementação personalizada do IPasswordHasher para utilizar BCrypt conforme requisito de segurança.
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher<ApplicationUser>
{
    public string HashPassword(ApplicationUser user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        // Utiliza o modo "Enhanced" do BCrypt para oferecer proteção adicional a Unicode e timing attacks.
        return BCryptNet.EnhancedHashPassword(password);
    }

    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        try
        {
            return BCryptNet.EnhancedVerify(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // O hash não está em um formato válido, portanto, a senha não corresponde.
            return PasswordVerificationResult.Failed;
        }
    }
}

