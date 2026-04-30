namespace DWH.Application.Helpers;

/// <summary>
/// Dto einer Destatis-Konfiguration
/// </summary>
public sealed class DestatisOptions
{
    /// <summary>
    /// Benutzername
    /// </summary>
    public string Username { get; init; } = "davidmeier.hemau@gmail.com";

    /// <summary>
    /// Passwort
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Sprache
    /// </summary>
    public string Language { get; init; } = "de";

    /// <summary>
    /// Bereich
    /// </summary>
    public string Area { get; init; } = "free";

    /// <summary>
    /// Komprimierung
    /// </summary>
    public bool Compress { get; init; } = false;

    /// <summary>
    /// Transponierung
    /// </summary>
    public bool Transpose { get; init; } = false;
}