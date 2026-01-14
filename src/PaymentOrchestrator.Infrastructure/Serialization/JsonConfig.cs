using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentOrchestrator.Infrastructure.Serialization;

public static class JsonConfig
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };
}