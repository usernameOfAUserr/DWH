using System.Globalization;
using System.Text.Json;
using DWH.Domain.Entities;

namespace DWH.Application.Parsers;

public static class DestatisParser
{
    public static List<DestatisSalaryData> ParseSalaryDataResponse(string json, bool onlyItJobs = true)
    {
        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("Object", out var objectElement))
        {
            throw new InvalidOperationException("Die API-Antwort enthält kein 'Object'.");
        }

        if (!objectElement.TryGetProperty("Content", out var contentElement))
        {
            throw new InvalidOperationException("Die API-Antwort enthält kein 'Object.Content'.");
        }

        var content = contentElement.GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            return new List<DestatisSalaryData>();
        }

        return ParseContent(content, onlyItJobs);
    }

    public static List<DestatisSalaryData> ParseContent(string content, bool onlyItJobs = true)
    {
        var result = new List<DestatisSalaryData>();

        var lines = content
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.TrimEnd())
            .ToList();

        foreach (var line in lines)
        {
            if (!TryParseLine(line, onlyItJobs, out var data))
            {
                continue;
            }

            result.Add(data);
        }

        return result;
    }

    private static bool TryParseLine(string line, bool onlyItJobs, out DestatisSalaryData? data)
    {
        data = null;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        if (line.StartsWith("___") ||
            line.StartsWith("© ") ||
            line.StartsWith("Stand:") ||
            line.StartsWith("Tabelle:") ||
            line.StartsWith("list;") ||
            line.StartsWith("Statistik-Code:") ||
            line.StartsWith("Zeit:") ||
            line.StartsWith("Deutschland;") ||
            line.StartsWith("Jahr;"))
        {
            return false;
        }

        var parts = line.Split(';');
        
        if (parts.Length < 4)
        {
            return false;
        }

        var yearRaw = parts[0].Trim();
        var codeRaw = parts[1].Trim();
        var jobRaw = parts[2].Trim();

        if (!int.TryParse(yearRaw, out var year))
        {
            return false;
        }

        if (!codeRaw.StartsWith("KB10-", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (onlyItJobs && !IsRelevantItJob(codeRaw, jobRaw))
        {
            return false;
        }

        var salary = ExtractFirstValidSalary(parts.Skip(3));

        if (salary is null)
        {
            return false;
        }

        data = new DestatisSalaryData
        {
            Year = year,
            Job = NormalizeJob(jobRaw),
            Salary = salary.Value
        };

        return true;
    }

    private static bool IsRelevantItJob(string code, string job)
    {
        if (code.StartsWith("KB10-43", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var normalizedJob = job.ToLowerInvariant();

        return normalizedJob.Contains("informatik") ||
               normalizedJob.Contains("ikt") ||
               normalizedJob.Contains("it-");
    }

    private static decimal? ExtractFirstValidSalary(IEnumerable<string> salaryParts)
    {
        foreach (var raw in salaryParts)
        {
            var value = raw.Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (value is "." or "/" or "-" )
            {
                continue;
            }

            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var salary))
            {
                return salary;
            }
        }

        return null;
    }

    private static string NormalizeJob(string job)
    {
        return string.Join(" ", job.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}