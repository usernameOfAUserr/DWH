using System.Text.Json;

namespace DWH.Domain.Dtos.Destatis;

/// <summary>
/// Dto eines technischen Destatis-Status
/// </summary>
public sealed class DestatisStatusDto
{
    /// <summary>
    /// Statuscode
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Inhalt
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Typ
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Rohwert des Status
    /// </summary>
    public JsonElement Raw { get; init; }
}