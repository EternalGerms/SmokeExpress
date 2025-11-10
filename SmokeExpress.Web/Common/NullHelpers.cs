// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Common;

public static class NullHelpers
{
    public static string GetSafeString(string? value) => value ?? string.Empty;
}


