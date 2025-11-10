// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto
namespace SmokeExpress.Web.Helpers;

/// <summary>
/// Fornece métodos auxiliares para formatação de valores exibidos na interface.
/// </summary>
public static class FormatHelper
{
    /// <summary>
    /// Formata um valor monetário utilizando o formato padrão de moeda local.
    /// </summary>
    /// <param name="value">Valor monetário a ser formatado.</param>
    /// <returns>Representação textual do valor em moeda local.</returns>
    public static string FormatCurrency(decimal value) => value.ToString("C");

    /// <summary>
    /// Converte uma data para horário local e aplica formatação curta.
    /// </summary>
    /// <param name="dateTime">Data e hora em UTC.</param>
    /// <returns>Data e hora formatadas no fuso local.</returns>
    public static string FormatDate(DateTime dateTime) => dateTime.ToLocalTime().ToString("g");

    /// <summary>
    /// Monta uma string com endereço completo, tratando partes opcionais.
    /// </summary>
    /// <param name="rua">Nome da rua ou logradouro.</param>
    /// <param name="numero">Número do endereço.</param>
    /// <param name="bairro">Bairro.</param>
    /// <param name="cidade">Cidade.</param>
    /// <param name="complemento">Complemento ou observação.</param>
    /// <returns>Endereço formatado em uma única string.</returns>
    public static string FormatAddress(string rua, string? numero, string bairro, string cidade, string? complemento)
    {
        var parts = new List<string> { rua };

        if (!string.IsNullOrWhiteSpace(numero))
        {
            parts.Add(numero);
        }

        parts.Add(bairro);
        parts.Add(cidade);

        if (!string.IsNullOrWhiteSpace(complemento))
        {
            parts.Add(complemento);
        }

        return string.Join(", ", parts);
    }
}


