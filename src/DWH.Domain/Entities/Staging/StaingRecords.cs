namespace DWH.Domain.Entities.Staging;

public sealed record DimTimeRow(
    DateTime? ReferenceDate,
    int YearNum,
    int? MonthNum,
    int? QuarterNum,
    string? YearMonth);

public sealed record DimIndustryRow(
    string? IndustryCode,
    string IndustryName);

public sealed record DimJobRow(
    string JobName,
    decimal IndustryId);

public sealed record FactAiExposureRow(
    decimal JobId,
    decimal AiExposureScore);

public sealed record FactSalaryRow(
    decimal JobId,
    decimal TimeId,
    decimal SalaryAmount);

public sealed record FactEmploymentIndustryRow(
    decimal IndustryId,
    decimal TimeId,
    decimal EmploymentCount);