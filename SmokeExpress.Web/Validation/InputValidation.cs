using System.ComponentModel.DataAnnotations;
using System.Linq;
using SmokeExpress.Web.Shared.FormControls;

namespace SmokeExpress.Web.Validation;

public static class InputValidation
{
    public static ValidationResult? ValidateCpfCnpj(string? value, ValidationContext _)
    {
        var digits = MaskUtils.DigitsOnly(value);

        if (string.IsNullOrEmpty(digits))
        {
            return new ValidationResult("Informe seu CPF ou CNPJ.");
        }

        return digits.Length switch
        {
            11 when IsValidCpf(digits) => ValidationResult.Success,
            14 when IsValidCnpj(digits) => ValidationResult.Success,
            11 => new ValidationResult("CPF inválido."),
            14 => new ValidationResult("CNPJ inválido."),
            _ => new ValidationResult("Informe um CPF (11 dígitos) ou CNPJ (14 dígitos) válido.")
        };
    }

    public static ValidationResult? ValidateTelefone(string? value, ValidationContext _)
    {
        var digits = MaskUtils.DigitsOnly(value);

        if (string.IsNullOrEmpty(digits))
        {
            return new ValidationResult("Informe um telefone para contato.");
        }

        return digits.Length is 10 or 11
            ? ValidationResult.Success
            : new ValidationResult("Informe um telefone válido com DDD.");
    }

    public static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
        {
            return false;
        }

        var soma = 0;
        for (var i = 0; i < 9; i++)
        {
            soma += (cpf[i] - '0') * (10 - i);
        }

        var resto = soma % 11;
        var primeiroDigito = resto < 2 ? 0 : 11 - resto;

        if (primeiroDigito != cpf[9] - '0')
        {
            return false;
        }

        soma = 0;
        for (var i = 0; i < 10; i++)
        {
            soma += (cpf[i] - '0') * (11 - i);
        }

        resto = soma % 11;
        var segundoDigito = resto < 2 ? 0 : 11 - resto;

        return segundoDigito == cpf[10] - '0';
    }

    public static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1)
        {
            return false;
        }

        var pesosPrimeiraEtapa = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var pesosSegundaEtapa = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var soma = 0;
        for (var i = 0; i < 12; i++)
        {
            soma += (cnpj[i] - '0') * pesosPrimeiraEtapa[i];
        }

        var resto = soma % 11;
        var primeiroDigito = resto < 2 ? 0 : 11 - resto;

        if (primeiroDigito != cnpj[12] - '0')
        {
            return false;
        }

        soma = 0;
        for (var i = 0; i < 13; i++)
        {
            soma += (cnpj[i] - '0') * pesosSegundaEtapa[i];
        }

        resto = soma % 11;
        var segundoDigito = resto < 2 ? 0 : 11 - resto;

        return segundoDigito == cnpj[13] - '0';
    }

    public static ValidationResult? ValidateAceiteTermos(bool value, ValidationContext validationContext)
    {
        if (value)
        {
            return ValidationResult.Success;
        }

        if (!string.IsNullOrEmpty(validationContext.MemberName))
        {
            return new ValidationResult(
                "É necessário aceitar os termos e condições de uso para criar uma conta.",
                new[] { validationContext.MemberName });
        }

        return new ValidationResult("É necessário aceitar os termos e condições de uso para criar uma conta.");
    }
}

