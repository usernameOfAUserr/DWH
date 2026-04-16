using System.Data;
using System.Data.Odbc;
using System.Globalization;
using DWH.Application.Interfaces.Repositories;
using DWH.Domain.Entities;

namespace DWH.Infrastructure.Repositories;

public class DbRepository : IDbRepository
{
    private const string ConnectionString =
        "DSN=OTH EXASOL DAVID;HOSTTIMEOUT=2000;LOGINTIMEOUT=10000;QUERYTIMEOUT=60;EXALOGFILE=C:\\temp\\exasol_odbc.log;LOGMODE=DEFAULT;";

    private const string Schema = "DAVID_MEIER";

    public async Task ReplaceAllAsync(
        IReadOnlyCollection<AiExposureData> aiExposures,
        IReadOnlyCollection<GermanEmploymentData> germanEmployments,
        IReadOnlyCollection<DestatisSalaryData> salaries,
        IReadOnlyCollection<IlostatInformationEmploymentData> ilostatInformationEmployments,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aiExposures);
        ArgumentNullException.ThrowIfNull(germanEmployments);
        ArgumentNullException.ThrowIfNull(salaries);
        ArgumentNullException.ThrowIfNull(ilostatInformationEmployments);

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await TruncateTableAsync(connection, transaction, GetQualifiedTableName("AI_EXPOSURES"), cancellationToken);
            await TruncateTableAsync(connection, transaction, GetQualifiedTableName("GERMAN_EMPLOYMENT"),
                cancellationToken);
            await TruncateTableAsync(connection, transaction, GetQualifiedTableName("SALARIES"), cancellationToken);
            await TruncateTableAsync(connection, transaction,
                GetQualifiedTableName("ILOSTAT_INFORMATION_EMPLOYMENT_DATA"), cancellationToken);

            await InsertSalariesAsync(connection, transaction, salaries, cancellationToken);
            await InsertGermanEmploymentsAsync(connection, transaction, germanEmployments, cancellationToken);
            await InsertAiExposuresAsync(connection, transaction, aiExposures, cancellationToken);
            await InsertIlostatInformationEmploymentsAsync(connection, transaction, ilostatInformationEmployments,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ReplaceAiExposuresAsync(
        IReadOnlyCollection<AiExposureData> aiExposures,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aiExposures);

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await TruncateTableAsync(connection, transaction, GetQualifiedTableName("AI_EXPOSURES"), cancellationToken);
            await InsertAiExposuresAsync(connection, transaction, aiExposures, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ReplaceGermanEmploymentsAsync(
        IReadOnlyCollection<GermanEmploymentData> germanEmployments,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(germanEmployments);

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await TruncateTableAsync(connection, transaction, GetQualifiedTableName("GERMAN_EMPLOYMENT"),
                cancellationToken);
            await InsertGermanEmploymentsAsync(connection, transaction, germanEmployments, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ReplaceSalariesAsync(
        IReadOnlyCollection<DestatisSalaryData> salaries,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(salaries);

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await TruncateTableAsync(connection, transaction, GetQualifiedTableName("SALARIES"), cancellationToken);
            await InsertSalariesAsync(connection, transaction, salaries, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ReplaceIlostatInformationEmploymentsAsync(
        IReadOnlyCollection<IlostatInformationEmploymentData> ilostatInformationEmployments,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ilostatInformationEmployments);

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await TruncateTableAsync(connection, transaction,
                GetQualifiedTableName("ILOSTAT_INFORMATION_EMPLOYMENT_DATA"), cancellationToken);
            await InsertIlostatInformationEmploymentsAsync(connection, transaction, ilostatInformationEmployments,
                cancellationToken);
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
        string qualifiedTableName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = (OdbcTransaction)transaction;
        command.CommandText = $"TRUNCATE TABLE {qualifiedTableName}";
        command.CommandType = CommandType.Text;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertAiExposuresAsync(
        OdbcConnection connection,
        IDbTransaction transaction,
        IReadOnlyCollection<AiExposureData> aiExposures,
        CancellationToken cancellationToken)
    {
        if (aiExposures.Count == 0)
        {
            return;
        }

        const int batchSize = 1000;

        foreach (var batch in Chunk(aiExposures, batchSize))
        {
            await using var command = connection.CreateCommand();
            command.Transaction = (OdbcTransaction)transaction;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 60;

            var valuesParts = new List<string>(batch.Count);

            foreach (var _ in batch)
            {
                valuesParts.Add("(?, ?)");
            }

            command.CommandText =
                $"""
                 INSERT INTO {GetQualifiedTableName("AI_EXPOSURES")}
                 (
                     OCCUPATION,
                     AI_EXPOSURE_SCORE
                 )
                 VALUES
                 {string.Join(",\n", valuesParts)}
                 """;

            foreach (var item in batch)
            {
                var occupationParameter = CreateParameter(command, OdbcType.VarChar);
                occupationParameter.Value = item.Occupation;
                command.Parameters.Add(occupationParameter);

                var aiExposureScoreParameter = CreateParameter(command, OdbcType.Decimal);
                aiExposureScoreParameter.Value = item.AiExposureScore;
                command.Parameters.Add(aiExposureScoreParameter);
            }

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task InsertGermanEmploymentsAsync(
        OdbcConnection connection,
        IDbTransaction transaction,
        IReadOnlyCollection<GermanEmploymentData> germanEmployments,
        CancellationToken cancellationToken)
    {
        if (germanEmployments.Count == 0)
        {
            return;
        }

        const int batchSize = 300;

        foreach (var batch in Chunk(germanEmployments, batchSize))
        {
            await using var command = connection.CreateCommand();
            command.Transaction = (OdbcTransaction)transaction;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 60;

            var valuesParts = new List<string>(batch.Count);

            foreach (var _ in batch)
            {
                valuesParts.Add("(?, ?, ?, ?, ?)");
            }

            command.CommandText =
                $"""
                 INSERT INTO {GetQualifiedTableName("GERMAN_EMPLOYMENT")}
                 (
                     REFERENCE_DATE,
                     DATA_YEAR,
                     ECONOMIC_SECTION_CODE,
                     ECONOMIC_SECTION,
                     EMPLOYMENT_COUNT
                 )
                 VALUES
                 {string.Join(",\n", valuesParts)}
                 """;

            foreach (var item in batch)
            {
                var referenceDateParameter = CreateParameter(command, OdbcType.Date);
                referenceDateParameter.Value = item.ReferenceDate.Date;
                command.Parameters.Add(referenceDateParameter);

                var yearParameter = CreateParameter(command, OdbcType.Decimal);
                yearParameter.Value = item.Year;
                command.Parameters.Add(yearParameter);

                var economicSectionCodeParameter = CreateParameter(command, OdbcType.VarChar);
                economicSectionCodeParameter.Value = item.EconomicSectionCode;
                command.Parameters.Add(economicSectionCodeParameter);

                var economicSectionParameter = CreateParameter(command, OdbcType.VarChar);
                economicSectionParameter.Value = item.EconomicSection;
                command.Parameters.Add(economicSectionParameter);

                var employmentCountParameter = CreateParameter(command, OdbcType.Decimal);
                employmentCountParameter.Value = item.EmploymentCount;
                command.Parameters.Add(employmentCountParameter);
            }

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task InsertSalariesAsync(
        OdbcConnection connection,
        IDbTransaction transaction,
        IReadOnlyCollection<DestatisSalaryData> salaries,
        CancellationToken cancellationToken)
    {
        if (salaries.Count == 0)
        {
            return;
        }

        const int batchSize = 1000;

        foreach (var batch in Chunk(salaries, batchSize))
        {
            await using var command = connection.CreateCommand();
            command.Transaction = (OdbcTransaction)transaction;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 60;

            var valuesParts = new List<string>(batch.Count);

            foreach (var _ in batch)
            {
                valuesParts.Add("(?, ?, ?)");
            }

            command.CommandText =
                $"""
                 INSERT INTO {GetQualifiedTableName("SALARIES")}
                 (
                     REFERENCE_DATE,
                     SALARY,
                     JOB
                 )
                 VALUES
                 {string.Join(",\n", valuesParts)}
                 """;

            foreach (var item in batch)
            {
                var referenceDateParameter = CreateParameter(command, OdbcType.Date);
                referenceDateParameter.Value = new DateTime(item.Year, 1, 1);
                command.Parameters.Add(referenceDateParameter);

                var salaryParameter = CreateParameter(command, OdbcType.Decimal);
                salaryParameter.Value = item.Salary;
                command.Parameters.Add(salaryParameter);

                var jobParameter = CreateParameter(command, OdbcType.VarChar);
                jobParameter.Value = item.Job;
                command.Parameters.Add(jobParameter);
            }

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task InsertIlostatInformationEmploymentsAsync(
        OdbcConnection connection,
        IDbTransaction transaction,
        IReadOnlyCollection<IlostatInformationEmploymentData> ilostatInformationEmployments,
        CancellationToken cancellationToken)
    {
        if (ilostatInformationEmployments.Count == 0)
        {
            return;
        }

        const int batchSize = 300;

        foreach (var batch in Chunk(ilostatInformationEmployments, batchSize))
        {
            await using var command = connection.CreateCommand();
            command.Transaction = (OdbcTransaction)transaction;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 60;

            var valuesParts = new List<string>(batch.Count);

            foreach (var _ in batch)
            {
                valuesParts.Add("(?, ?, ?, ?, ?)");
            }

            command.CommandText =
                $"""
                 INSERT INTO {GetQualifiedTableName("ILOSTAT_INFORMATION_EMPLOYMENT_DATA")}
                 (
                     REF_AREA,
                     AREA_LABEL,
                     SOURCE_LABEL,
                     TOTAL,
                     REFERENCE_DATE
                 )
                 VALUES
                 {string.Join(",\n", valuesParts)}
                 """;

            foreach (var item in batch)
            {
                var refAreaParameter = CreateParameter(command, OdbcType.VarChar);
                refAreaParameter.Value = item.RefArea;
                command.Parameters.Add(refAreaParameter);

                var areaLabelParameter = CreateParameter(command, OdbcType.VarChar);
                areaLabelParameter.Value = item.AreaLabel;
                command.Parameters.Add(areaLabelParameter);

                var sourceLabelParameter = CreateParameter(command, OdbcType.VarChar);
                sourceLabelParameter.Value = item.SourceLabel;
                command.Parameters.Add(sourceLabelParameter);

                var totalParameter = CreateParameter(command, OdbcType.Decimal);
                totalParameter.Value = item.Total.HasValue ? item.Total.Value : DBNull.Value;
                command.Parameters.Add(totalParameter);

                var referenceDateParameter = CreateParameter(command, OdbcType.Date);
                referenceDateParameter.Value = ParseReferenceDate(item.Time);
                command.Parameters.Add(referenceDateParameter);
            }

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static DateTime ParseReferenceDate(string time)
    {
        if (DateTime.TryParseExact(time, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var yearOnly))
        {
            return new DateTime(yearOnly.Year, 1, 1);
        }

        if (DateTime.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            return parsedDate.Date;
        }

        throw new InvalidOperationException($"The ILOSTAT time value '{time}' could not be converted to a date.");
    }

    private static IEnumerable<List<T>> Chunk<T>(IReadOnlyCollection<T> source, int batchSize)
    {
        var batch = new List<T>(batchSize);

        foreach (var item in source)
        {
            batch.Add(item);

            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    private static OdbcParameter CreateParameter(OdbcCommand command, OdbcType odbcType)
    {
        var parameter = command.CreateParameter();
        parameter.OdbcType = odbcType;

        return parameter;
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