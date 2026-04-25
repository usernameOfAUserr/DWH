using System.Data;
using System.Data.Odbc;
using DWH.Application.Interfaces.Repositories;

namespace DWH.Infrastructure.Repositories;

public class LoadRepository : ILoadRepository
{
    private const string ConnectionString =
        "DSN=OTH EXASOL DAVID;";

    private const string Schema = "DAVID_MEIER";

    public async Task ImportAllFromCsvAsync(
        CsvImportFiles csvImportFiles,
        string importBaseUrl,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(csvImportFiles);
        ArgumentException.ThrowIfNullOrWhiteSpace(importBaseUrl);

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await TruncateTableAsync(connection, transaction, "AI_EXPOSURES", cancellationToken);
            await TruncateTableAsync(connection, transaction, "GERMAN_EMPLOYMENT", cancellationToken);
            await TruncateTableAsync(connection, transaction, "SALARIES", cancellationToken);
            await TruncateTableAsync(connection, transaction, "ILOSTAT_INFORMATION_EMPLOYMENT_DATA", cancellationToken);
            await TruncateTableAsync(connection, transaction, "EUROSTAT_JOB_VACANCIES", cancellationToken);

            await ImportCsvAsync(
                connection,
                transaction,
                "EUROSTAT_JOB_VACANCIES",
                importBaseUrl,
                csvImportFiles.EurostatJobVacanciesFileName,
                cancellationToken);

            await ImportCsvAsync(
                connection,
                transaction,
                "AI_EXPOSURES",
                importBaseUrl,
                csvImportFiles.AiExposuresFileName,
                cancellationToken);

            await ImportCsvAsync(
                connection,
                transaction,
                "GERMAN_EMPLOYMENT",
                importBaseUrl,
                csvImportFiles.GermanEmploymentsFileName,
                cancellationToken);

            await ImportCsvAsync(
                connection,
                transaction,
                "SALARIES",
                importBaseUrl,
                csvImportFiles.SalariesFileName,
                cancellationToken);

            await ImportCsvAsync(
                connection,
                transaction,
                "ILOSTAT_INFORMATION_EMPLOYMENT_DATA",
                importBaseUrl,
                csvImportFiles.IlostatInformationEmploymentsFileName,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ImportAiExposuresFromCsvAsync(
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importBaseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        await ImportSingleAsync(
            "AI_EXPOSURES",
            importBaseUrl,
            fileName,
            cancellationToken);
    }

    public async Task ImportGermanEmploymentsFromCsvAsync(
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importBaseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        await ImportSingleAsync(
            "GERMAN_EMPLOYMENT",
            importBaseUrl,
            fileName,
            cancellationToken);
    }

    public async Task ImportSalariesFromCsvAsync(
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importBaseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        await ImportSingleAsync(
            "SALARIES",
            importBaseUrl,
            fileName,
            cancellationToken);
    }

    public async Task ImportIlostatInformationEmploymentsFromCsvAsync(
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importBaseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        await ImportSingleAsync(
            "ILOSTAT_INFORMATION_EMPLOYMENT_DATA",
            importBaseUrl,
            fileName,
            cancellationToken);
    }

    public async Task ImportEurostatJobVacanciesFromCsvAsync(
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importBaseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        await ImportSingleAsync(
            "EUROSTAT_JOB_VACANCIES",
            importBaseUrl,
            fileName,
            cancellationToken);
    }

    private static async Task ImportSingleAsync(
        string tableName,
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await TruncateTableAsync(connection, transaction, tableName, cancellationToken);
            await ImportCsvAsync(connection, transaction, tableName, importBaseUrl, fileName, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static OdbcConnection CreateConnection()
    {
        return new OdbcConnection(ConnectionString);
    }

    private static async Task TruncateTableAsync(
        OdbcConnection connection,
        IDbTransaction transaction,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = (OdbcTransaction)transaction;
        command.CommandType = CommandType.Text;
        command.CommandText = $"TRUNCATE TABLE {GetQualifiedTableName(tableName)}";

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ImportCsvAsync(
        OdbcConnection connection,
        IDbTransaction transaction,
        string tableName,
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken)
    {
        var columns = GetImportColumns(tableName);

        await using var command = connection.CreateCommand();
        command.Transaction = (OdbcTransaction)transaction;
        command.CommandType = CommandType.Text;
        command.CommandText =
            $"""
             IMPORT INTO {GetQualifiedTableName(tableName)} ({columns})
             FROM CSV
             AT '{EscapeSqlLiteral(NormalizeBaseUrl(importBaseUrl))}'
             FILE '{EscapeSqlLiteral(fileName)}'
             SKIP = 1
             """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string GetImportColumns(string tableName)
    {
        return tableName switch
        {
            "AI_EXPOSURES" =>
                string.Join(", ",
                    QuoteIdentifier("OCCUPATION"),
                    QuoteIdentifier("AI_EXPOSURE_SCORE"),
                    QuoteIdentifier("EMPLOYMENT"),
                    QuoteIdentifier("MEDIAN_WAGE"),
                    QuoteIdentifier("PROJECTED_GROWTH_PCT"),
                    QuoteIdentifier("TOP_RISK_FACTORS"),
                    QuoteIdentifier("RISK_SCORE"),
                    QuoteIdentifier("TOP_SAFE_TASKS")),

            "GERMAN_EMPLOYMENT" =>
                $"{QuoteIdentifier("REFERENCE_DATE")}, {QuoteIdentifier("DATA_YEAR")}, {QuoteIdentifier("ECONOMIC_SECTION_CODE")}, {QuoteIdentifier("ECONOMIC_SECTION")}, {QuoteIdentifier("EMPLOYMENT_COUNT")}",

            "SALARIES" =>
                $"{QuoteIdentifier("DATA_YEAR")}, {QuoteIdentifier("SALARY")}, {QuoteIdentifier("JOB")}",

            "ILOSTAT_INFORMATION_EMPLOYMENT_DATA" =>
                $"{QuoteIdentifier("REF_AREA")}, {QuoteIdentifier("AREA_LABEL")}, {QuoteIdentifier("SOURCE_LABEL")}, {QuoteIdentifier("TOTAL")}, {QuoteIdentifier("REFERENCE_DATE")}",

            "EUROSTAT_JOB_VACANCIES" =>
                string.Join(", ",
                    QuoteIdentifier("FREQ"),
                    QuoteIdentifier("S_ADJ"),
                    QuoteIdentifier("NACE_R2"),
                    QuoteIdentifier("SIZECLAS"),
                    QuoteIdentifier("INDIC_EM"),
                    QuoteIdentifier("GEO"),
                    QuoteIdentifier("TIME_PERIOD"),
                    QuoteIdentifier("OBSERVATION_VALUE")),

            _ => throw new ArgumentOutOfRangeException(nameof(tableName), tableName, "Unbekannte Tabelle")
        };
    }

    private static string NormalizeBaseUrl(string importBaseUrl)
    {
        return importBaseUrl.TrimEnd('/');
    }

    private static string EscapeSqlLiteral(string value)
    {
        return value.Replace("'", "''");
    }

    private static string GetQualifiedTableName(string tableName)
    {
        return $"{QuoteIdentifier(Schema)}.{QuoteIdentifier(tableName)}";
    }

    private static string QuoteIdentifier(string identifier)
    {
        var escapedIdentifier = identifier.Replace("\"", "\"\"");

        return $"\"{escapedIdentifier}\"";
    }
}