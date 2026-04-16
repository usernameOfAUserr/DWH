using System.Text.Json;

namespace DWH.Domain.Dtos.Destatis;

/// <summary>
/// Dto eines technischen Destatis-Objekts
/// </summary>
public sealed class DestatisObjectDto
{
    /// <summary>
    /// Inhalt
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Rohwert des Objekts
    /// </summary>
    public JsonElement Raw { get; init; }
}