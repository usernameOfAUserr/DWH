using System.Text.Json;

namespace DWH.Domain.Dtos;

public sealed class EurostatResponse(JsonDocument rawJson)
{
    public JsonDocument RawJson { get; } = rawJson;
}