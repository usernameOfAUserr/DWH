namespace DWH.Domain.Dtos.Destatis;

/// <summary>
/// Dto einer technischen Destatis-Ident
/// </summary>
public sealed class DestatisIdentDto
{
    /// <summary>
    /// Service
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Methode
    /// </summary>
    public string? Method { get; init; }
}