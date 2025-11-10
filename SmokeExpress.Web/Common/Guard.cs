// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Common;

public static class Guard
{
    public static void AgainstNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    public static void AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} não pode ser vazio.", paramName);
        }
    }

    public static void AgainstNegative(decimal value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentException($"{paramName} não pode ser negativo.", paramName);
        }
    }
}


