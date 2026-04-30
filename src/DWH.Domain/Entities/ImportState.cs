namespace DWH.Domain.Entities;

public static class ImportState
{
    /// <summary>
    /// Gibt an, ob gerade ein Import läuft
    /// </summary>
    public static bool IsRunning { get; set; }

    /// <summary>
    /// Fortschritt in Prozent
    /// </summary>
    public static int Progress { get; set; }

    /// <summary>
    /// Statusnachricht
    /// </summary>
    public static string Message { get; set; } = string.Empty;
}