using System.ComponentModel.DataAnnotations;

namespace DWH.Domain.Dtos.AiExposure;

/// <summary>
/// Dto eines paginierten AI-Exposure-Requests
/// </summary>
public class AiExposurePagedRequest
{
    /// <summary>
    /// Sortierfeld
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// Sortierreihenfolge
    /// </summary>
    [RegularExpression("asc|desc")]
    public string? Order { get; set; }

    /// <summary>
    /// Limit
    /// </summary>
    [Range(1, 200)]
    public int? Limit { get; set; }

    /// <summary>
    /// Offset
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? Offset { get; set; }
}