using System.Data;
using System.Data.Odbc;
using DWH.Application.Interfaces.Repositories;

namespace DWH.Infrastructure.Repositories;

public class StagingRepository : IStagingRepository
{
    private const string ConnectionString = "DSN=OTH EXASOL DAVID;";
    private const string Schema = "DAVID_MEIER";

    public Task TruncateFactAiExposureAsync(CancellationToken cancellationToken = default)
        => ExecuteNonQueryAsync(
            $"TRUNCATE TABLE {GetQualifiedTableName("FACT_AI_EXPOSURE")}",
            cancellationToken);

    public Task TruncateFactSalaryAsync(CancellationToken cancellationToken = default)
        => ExecuteNonQueryAsync(
            $"TRUNCATE TABLE {GetQualifiedTableName("FACT_SALARY")}",
            cancellationToken);

    public Task TruncateDimGeoAsync(CancellationToken cancellationToken = default)
        => ExecuteNonQueryAsync(
            $"TRUNCATE TABLE {GetQualifiedTableName("DIM_GEO")}",
            cancellationToken);

    public Task TruncateFactEmploymentIndustryAsync(CancellationToken cancellationToken = default)
        => ExecuteNonQueryAsync(
            $"TRUNCATE TABLE {GetQualifiedTableName("FACT_EMPLOYMENT_INDUSTRY")}",
            cancellationToken);

    public Task TruncateDimJobAsync(CancellationToken cancellationToken = default)
        => ExecuteNonQueryAsync(
            $"TRUNCATE TABLE {GetQualifiedTableName("DIM_JOB")}",
            cancellationToken);

    public Task TruncateDimTimeAsync(CancellationToken cancellationToken = default)
        => ExecuteNonQueryAsync(
            $"TRUNCATE TABLE {GetQualifiedTableName("DIM_TIME")}",
            cancellationToken);

    public Task TruncateFactJobVacancyAsync(CancellationToken cancellationToken = default)
        => ExecuteNonQueryAsync(
            $"TRUNCATE TABLE {GetQualifiedTableName("FACT_JOB_VACANCY")}",
            cancellationToken);

    public Task BuildDimTimeAsync(CancellationToken cancellationToken = default)
    {
        // DimTime is used for FactEmploymentJob, FactSalary, FactEmploymentIndustry and FactJobVacancy
        // Sources: Salaries and IlostatInformationEmploymentData have only full years, but GermanEmployment has months too

        var sql =
            $"""
             INSERT INTO {GetQualifiedTableName("DIM_TIME")}
             (
                 {QuoteIdentifier("REFERENCE_DATE")},
                 {QuoteIdentifier("YEAR_NUM")},
                 {QuoteIdentifier("MONTH_NUM")},
                 {QuoteIdentifier("QUARTER_NUM")},
                 {QuoteIdentifier("YEAR_MONTH")}
             )
             SELECT DISTINCT
                 SOURCE_DATA.REFERENCE_DATE,
                 EXTRACT(YEAR FROM SOURCE_DATA.REFERENCE_DATE) AS YEAR_NUM,
                 EXTRACT(MONTH FROM SOURCE_DATA.REFERENCE_DATE) AS MONTH_NUM,
                 ((EXTRACT(MONTH FROM SOURCE_DATA.REFERENCE_DATE) - 1) / 3) + 1 AS QUARTER_NUM,
                 TO_CHAR(SOURCE_DATA.REFERENCE_DATE, 'YYYY-MM') AS YEAR_MONTH
             FROM
             (
                 SELECT
                     TO_DATE(CAST(S.{QuoteIdentifier("DATA_YEAR")} AS VARCHAR(4)) || '-01-01', 'YYYY-MM-DD') AS REFERENCE_DATE
                 FROM {GetQualifiedTableName("SALARIES")} S
                 WHERE S.{QuoteIdentifier("DATA_YEAR")} IS NOT NULL

                 UNION

                 SELECT
                     CAST(GE.{QuoteIdentifier("REFERENCE_DATE")} AS DATE) AS REFERENCE_DATE
                 FROM {GetQualifiedTableName("GERMAN_EMPLOYMENT")} GE
                 WHERE GE.{QuoteIdentifier("REFERENCE_DATE")} IS NOT NULL

                 UNION

                 SELECT
                     TO_DATE(CAST(I.{QuoteIdentifier("REFERENCE_DATE")} AS VARCHAR(4)) || '-01-01', 'YYYY-MM-DD') AS REFERENCE_DATE
                 FROM {GetQualifiedTableName("ILOSTAT_INFORMATION_EMPLOYMENT_DATA")} I
                 WHERE I.{QuoteIdentifier("REFERENCE_DATE")} IS NOT NULL
                 
                 UNION 
                 
                 SELECT
                 TO_DATE(
                     SUBSTR(JV."TIME_PERIOD", 1, 4) || '-' ||
                     CASE SUBSTR(JV."TIME_PERIOD", 7, 1)
                         WHEN '1' THEN '01'
                         WHEN '2' THEN '04'
                         WHEN '3' THEN '07'
                         WHEN '4' THEN '10'
                     END || '-01',
                     'YYYY-MM-DD'
                 ) AS REFERENCE_DATE
             FROM DAVID_MEIER.EUROSTAT_JOB_VACANCIES JV
             WHERE JV."TIME_PERIOD" IS NOT NULL
             ) SOURCE_DATA
             WHERE SOURCE_DATA.REFERENCE_DATE IS NOT NULL
             """;

        return ExecuteNonQueryAsync(sql, cancellationToken);
    }

    public Task BuildDimGeoAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"""
                      INSERT INTO {GetQualifiedTableName("DIM_GEO")}
                      ({QuoteIdentifier("GEO_CODE")})
                      SELECT DISTINCT
                          JV.{QuoteIdentifier("GEO")} AS GEO_CODE
                          FROM {GetQualifiedTableName("EUROSTAT_JOB_VACANCIES")} JV
                   WHERE JV.{QuoteIdentifier("GEO")} IS NOT NULL
                   """;

        return ExecuteNonQueryAsync(sql, cancellationToken);
    }

    public Task BuildDimJobAsync(CancellationToken cancellationToken = default)
    {
        // Job needs to cover and combine AiExposures and Salaries
        // For that, the table JOB_TRANSLATION_MAPPING_BETWEEN_AI_EXPOSURE_AND_SALARIES was created
        // German jobs are stored from Salaries, English jobs from AiExposures

        var sql =
            $"""
             INSERT INTO {GetQualifiedTableName("DIM_JOB")}
             (
                 {QuoteIdentifier("JOB_NAME_GERMAN")},
                 {QuoteIdentifier("JOB_NAME_ENGLISH")}
             )
             SELECT DISTINCT
                 M.{QuoteIdentifier("GERMAN_JOB")} AS JOB_NAME_GERMAN,
                 M.{QuoteIdentifier("ENGLISH_JOB")} AS JOB_NAME_ENGLISH
             FROM {GetQualifiedTableName("JOB_TRANSLATION_MAPPING_BETWEEN_AI_EXPOSURE_AND_SALARIES")} M
             WHERE M.{QuoteIdentifier("GERMAN_JOB")} IS NOT NULL
               AND M.{QuoteIdentifier("ENGLISH_JOB")} IS NOT NULL
             """;

        return ExecuteNonQueryAsync(sql, cancellationToken);
    }

    public Task BuildFactAiExposureAsync(CancellationToken cancellationToken = default)
    {
        var sql =
            $"""
             INSERT INTO {GetQualifiedTableName("FACT_AI_EXPOSURE")}
             (
                 {QuoteIdentifier("JOB_ID")},
                 {QuoteIdentifier("AI_EXPOSURE_SCORE")},
                 {QuoteIdentifier("EMPLOYMENT")},
                 {QuoteIdentifier("MEDIAN_WAGE")},
                 {QuoteIdentifier("PROJECTED_GROWTH_PCT")},
                 {QuoteIdentifier("TOP_RISK_FACTORS")},
                 {QuoteIdentifier("RISK_SCORE")},
                 {QuoteIdentifier("TOP_SAFE_TASKS")}
             )
             SELECT
                 X.{QuoteIdentifier("JOB_ID")},
                 X.{QuoteIdentifier("AI_EXPOSURE_SCORE")},
                 X.{QuoteIdentifier("EMPLOYMENT")},
                 X.{QuoteIdentifier("MEDIAN_WAGE")},
                 X.{QuoteIdentifier("PROJECTED_GROWTH_PCT")},
                 X.{QuoteIdentifier("TOP_RISK_FACTORS")},
                 X.{QuoteIdentifier("RISK_SCORE")},
                 X.{QuoteIdentifier("TOP_SAFE_TASKS")}
             FROM
             (
                 SELECT
                     DJ.{QuoteIdentifier("ID")} AS JOB_ID,
                     AE.{QuoteIdentifier("AI_EXPOSURE_SCORE")} AS AI_EXPOSURE_SCORE,
                     AE.{QuoteIdentifier("EMPLOYMENT")} AS EMPLOYMENT,
                     AE.{QuoteIdentifier("MEDIAN_WAGE")} AS MEDIAN_WAGE,
                     AE.{QuoteIdentifier("PROJECTED_GROWTH_PCT")} AS PROJECTED_GROWTH_PCT,
                     AE.{QuoteIdentifier("TOP_RISK_FACTORS")} AS TOP_RISK_FACTORS,
                     AE.{QuoteIdentifier("RISK_SCORE")} AS RISK_SCORE,
                     AE.{QuoteIdentifier("TOP_SAFE_TASKS")} AS TOP_SAFE_TASKS,
                     ROW_NUMBER() OVER
                     (
                         PARTITION BY AE.{QuoteIdentifier("OCCUPATION")}
                         ORDER BY DJ.{QuoteIdentifier("ID")}
                     ) AS RN
                 FROM {GetQualifiedTableName("AI_EXPOSURES")} AE
                 INNER JOIN {GetQualifiedTableName("DIM_JOB")} DJ
                     ON DJ.{QuoteIdentifier("JOB_NAME_ENGLISH")} = AE.{QuoteIdentifier("OCCUPATION")}
                 WHERE AE.{QuoteIdentifier("AI_EXPOSURE_SCORE")} IS NOT NULL
             ) X
             WHERE X.RN = 1
             """;

        return ExecuteNonQueryAsync(sql, cancellationToken);
    }

    public Task BuildFactSalaryAsync(CancellationToken cancellationToken = default)
    {
        var sql =
            $"""
             INSERT INTO {GetQualifiedTableName("FACT_SALARY")}
             (
                 {QuoteIdentifier("JOB_ID")},
                 {QuoteIdentifier("TIME_ID")},
                 {QuoteIdentifier("SALARY_AMOUNT")}
             )
             SELECT
                 DJ.{QuoteIdentifier("ID")} AS JOB_ID,
                 DT.{QuoteIdentifier("ID")} AS TIME_ID,
                 S.{QuoteIdentifier("SALARY")} AS SALARY_AMOUNT
             FROM {GetQualifiedTableName("SALARIES")} S
             INNER JOIN {GetQualifiedTableName("DIM_JOB")} DJ
                 ON DJ.{QuoteIdentifier("JOB_NAME_GERMAN")} = S.{QuoteIdentifier("JOB")}
             INNER JOIN {GetQualifiedTableName("DIM_TIME")} DT
                 ON DT.{QuoteIdentifier("YEAR_NUM")} = S.{QuoteIdentifier("DATA_YEAR")}
                AND DT.{QuoteIdentifier("MONTH_NUM")} = 1
             WHERE S.{QuoteIdentifier("SALARY")} IS NOT NULL
             """;

        return ExecuteNonQueryAsync(sql, cancellationToken);
    }

    public Task BuildFactEmploymentIndustryAsync(CancellationToken cancellationToken = default)
    {
        // First, only Ilostat Employment Data which only contains the IT field will be used to fill this table

        var sql =
            $"""
             INSERT INTO {GetQualifiedTableName("FACT_EMPLOYMENT_INDUSTRY")}
             (
                 {QuoteIdentifier("TIME_ID")},
                 {QuoteIdentifier("EMPLOYMENT_COUNT")}
             )
             SELECT
                 DT.{QuoteIdentifier("ID")} AS TIME_ID,
                 SUM(I.{QuoteIdentifier("TOTAL")}) AS EMPLOYMENT_COUNT
             FROM {GetQualifiedTableName("ILOSTAT_INFORMATION_EMPLOYMENT_DATA")} I
             INNER JOIN {GetQualifiedTableName("DIM_TIME")} DT
                 ON DT.{QuoteIdentifier("YEAR_NUM")} = I.{QuoteIdentifier("REFERENCE_DATE")}
                AND DT.{QuoteIdentifier("MONTH_NUM")} = 1
             WHERE I.{QuoteIdentifier("TOTAL")} IS NOT NULL
             GROUP BY DT.{QuoteIdentifier("ID")}
             """;

        return ExecuteNonQueryAsync(sql, cancellationToken);
    }

    public Task BuildFactJobVacancyAsync(CancellationToken cancellationToken = default)
    {
        var sql =
            $"""
             INSERT INTO {GetQualifiedTableName("FACT_JOB_VACANCY")}
             (
                 {QuoteIdentifier("TIME_ID")},
                 {QuoteIdentifier("JOB_VACANCIES")},
                 {QuoteIdentifier("GEO_ID")}
             )
             SELECT
                 DT.{QuoteIdentifier("ID")} AS TIME_ID,
                 JV.{QuoteIdentifier("OBSERVATION_VALUE")} AS JOB_VACANCIES,
                 DG.{QuoteIdentifier("ID")} as GEO_ID
             FROM {GetQualifiedTableName("EUROSTAT_JOB_VACANCIES")} JV
             INNER JOIN {GetQualifiedTableName("DIM_TIME")} DT
                 ON DT.{QuoteIdentifier("REFERENCE_DATE")} =
                    TO_DATE(
                        SUBSTR(JV.{QuoteIdentifier("TIME_PERIOD")}, 1, 4) || '-' ||
                        CASE SUBSTR(JV.{QuoteIdentifier("TIME_PERIOD")}, 7, 1)
                            WHEN '1' THEN '01'
                            WHEN '2' THEN '04'
                            WHEN '3' THEN '07'
                            WHEN '4' THEN '10'
                        END || '-01',
                        'YYYY-MM-DD'
                    )
             Join {GetQualifiedTableName("DIM_GEO")} DG 
                ON JV.{QuoteIdentifier("GEO")} = DG.{QuoteIdentifier("GEO_CODE")}
             WHERE JV.{QuoteIdentifier("OBSERVATION_VALUE")} IS NOT NULL
             """;

        return ExecuteNonQueryAsync(sql, cancellationToken);
    }

    private static async Task ExecuteNonQueryAsync(
        string sql,
        CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = sql;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static OdbcConnection CreateConnection()
    {
        return new OdbcConnection(ConnectionString);
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