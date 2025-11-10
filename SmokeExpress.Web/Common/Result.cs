// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Common;

/// <summary>
/// Representa o resultado de uma operação que pode falhar.
/// Usado para separar validações da lógica de negócio.
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    private Result(bool isSuccess, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
    
    public static Result Success() => new(true);
    public static Result Failure(string message) => new(false, message);
}

/// <summary>
/// Representa o resultado de uma operação que pode falhar e retornar um valor.
/// Usado para separar validações da lógica de negócio.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    private Result(bool isSuccess, T? value, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }
    
    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(string message) => new(false, default, message);
}

