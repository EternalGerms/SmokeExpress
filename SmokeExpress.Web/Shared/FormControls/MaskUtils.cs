using System.Text;

namespace SmokeExpress.Web.Shared.FormControls;

public static class MaskUtils
{
    public static string DigitsOnly(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            if (char.IsDigit(ch))
            {
                builder.Append(ch);
            }
        }

        return builder.ToString();
    }

    public static string ApplyMask(string? value, MaskType mask)
    {
        return mask switch
        {
            MaskType.CpfCnpj => FormatCpfCnpj(DigitsOnly(value)),
            MaskType.Telefone => FormatTelefone(DigitsOnly(value)),
            MaskType.Cep => FormatCep(DigitsOnly(value)),
            _ => value?.Trim() ?? string.Empty
        };
    }

    public static string RemoveMask(string? value, MaskType mask)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return mask switch
        {
            MaskType.CpfCnpj or MaskType.Telefone or MaskType.Cep => DigitsOnly(value),
            _ => value.Trim()
        };
    }

    public static string FormatCpfCnpj(string digits)
    {
        if (string.IsNullOrEmpty(digits))
        {
            return string.Empty;
        }

        if (digits.Length <= 11)
        {
            digits = digits[..Math.Min(digits.Length, 11)];

            return digits.Length switch
            {
                <= 3 => digits,
                <= 6 => $"{digits[..3]}.{digits[3..]}",
                <= 9 => $"{digits[..3]}.{digits[3..6]}.{digits[6..]}",
                _ => $"{digits[..3]}.{digits[3..6]}.{digits[6..9]}-{digits[9..]}"
            };
        }

        digits = digits[..Math.Min(digits.Length, 14)];

        return digits.Length switch
        {
            <= 2 => digits,
            <= 5 => $"{digits[..2]}.{digits[2..]}",
            <= 8 => $"{digits[..2]}.{digits[2..5]}.{digits[5..]}",
            <= 12 => $"{digits[..2]}.{digits[2..5]}.{digits[5..8]}/{digits[8..]}",
            _ => $"{digits[..2]}.{digits[2..5]}.{digits[5..8]}/{digits[8..12]}-{digits[12..]}"
        };
    }

    public static string FormatTelefone(string digits)
    {
        if (string.IsNullOrEmpty(digits))
        {
            return string.Empty;
        }

        digits = digits[..Math.Min(digits.Length, 11)];

        if (digits.Length <= 2)
        {
            return digits;
        }

        if (digits.Length <= 6)
        {
            return $"({digits[..2]}) {digits[2..]}";
        }

        if (digits.Length <= 10)
        {
            return $"({digits[..2]}) {digits[2..6]}-{digits[6..]}";
        }

        return $"({digits[..2]}) {digits[2..7]}-{digits[7..]}";
    }

    public static string FormatCep(string digits)
    {
        if (string.IsNullOrEmpty(digits))
        {
            return string.Empty;
        }

        digits = digits[..Math.Min(digits.Length, 8)];

        return digits.Length <= 5
            ? digits
            : $"{digits[..5]}-{digits[5..]}";
    }
}


