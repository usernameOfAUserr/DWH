using System.Text.Json;
using DWH.Domain.Dtos.Destatis;

namespace DWH.Domain.Models.DataSources;

/// <summary>
/// Dto einer technischen Destatis-Response
/// </summary>
public sealed class DestatisApiResponse
{
    /// <summary>
    /// Pflicht JSON-Rohdaten der Destatis-Response
    /// </summary>
    public required JsonElement RawJson { get; init; }

    /// <summary>
    /// Technische Status-Daten
    /// </summary>
    public DestatisStatusDto? Status { get; init; }
}