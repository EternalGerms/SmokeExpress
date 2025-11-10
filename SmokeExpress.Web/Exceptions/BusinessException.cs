// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
    public BusinessException(string message, Exception innerException) : base(message, innerException) { }
}

public class ValidationException : BusinessException
{
    public ValidationException(string message) : base(message) { }
}

public class NotFoundException : BusinessException
{
    public NotFoundException(string resource, object id) 
        : base($"{resource} com ID {id} não encontrado.") { }
    public NotFoundException(string message) : base(message) { }
}

