namespace DWH.Domain.Entities;

public class DestatisSalaryData
{
    public required int Year { get; set; }

    /// <summary>
    ///  arithmetischer Durchschnitt aller Gehälter
    /// </summary>
    public required decimal Salary { get; set; }

    public required string Job { get; set; }
}