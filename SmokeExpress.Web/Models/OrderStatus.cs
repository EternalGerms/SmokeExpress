// Projeto Smoke Express - Autores: Bruno Bueno e Matheus Esposto

namespace SmokeExpress.Web.Models;

/// <summary>
/// Representa os possíveis status de um pedido na plataforma Smoke Express.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Pedido recebido e sendo processado.
    /// </summary>
    Processando = 0,

    /// <summary>
    /// Pedido confirmado e aguardando preparação.
    /// </summary>
    Confirmado = 1,

    /// <summary>
    /// Pedido em preparação para envio.
    /// </summary>
    EmPreparacao = 2,

    /// <summary>
    /// Pedido enviado ao cliente.
    /// </summary>
    Enviado = 3,

    /// <summary>
    /// Pedido entregue ao cliente.
    /// </summary>
    Entregue = 4,

    /// <summary>
    /// Pedido cancelado.
    /// </summary>
    Cancelado = 5
}

