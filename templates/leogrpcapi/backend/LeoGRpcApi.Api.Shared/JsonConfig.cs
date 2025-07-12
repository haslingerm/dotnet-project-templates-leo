using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime.Serialization.SystemTextJson;

namespace LeoGRpcApi.Api.Shared;

public static class JsonConfig
{
    public static void ConfigureJsonSerialization(JsonSerializerOptions options, bool isDev)
    {
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.PropertyNameCaseInsensitive = true;
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.Converters.Add(new JsonStringEnumConverter());
        options.WriteIndented = isDev;
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    }
}
