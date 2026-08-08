using System.Text.Json;

namespace LeoMiniApi.Util;

/// <summary>
///     Request validation helpers for the API layer
/// </summary>
public static class Validation
{
    /// <summary>
    ///     Creates a validation error collection for a single property
    /// </summary>
    /// <param name="propertyName">The name of the property the check failed for</param>
    /// <param name="message">The message describing why the check failed</param>
    /// <returns>The validation errors, keyed by the property name as the client sees it in the JSON contract</returns>
    public static IDictionary<string, string[]> Failure(string propertyName, string message) =>
    new Dictionary<string, string[]>
    {
        [JsonNamingPolicy.CamelCase.ConvertName(propertyName)] = [message]
    };
}
