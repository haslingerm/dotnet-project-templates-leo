using System.Text.Json;
using System.Text.RegularExpressions;

namespace LeoWebApi.Util;

/// <summary>
///     Naming conventions shared by FluentValidation and JSON serialization, so that the keys of a validation
///     error response match the property names the client sees
/// </summary>
internal static partial class ValidationNaming
{
    /// <summary>
    ///     Converts a property name to the camelCase form used in the JSON contract
    /// </summary>
    /// <param name="memberName">The name of the property as declared in C#</param>
    /// <returns>The property name as it appears in the JSON contract</returns>
    public static string ToPropertyName(string memberName) => JsonNamingPolicy.CamelCase.ConvertName(memberName);

    /// <summary>
    ///     Converts a property name to the human-readable display name used inside validation messages
    /// </summary>
    /// <param name="memberName">The name of the property as declared in C#</param>
    /// <returns>The display name to use in validation messages</returns>
    public static string ToDisplayName(string memberName) => PascalCaseBoundary.Replace(memberName, " $1");

    [GeneratedRegex("(?<=[a-z0-9])([A-Z])")]
    private static partial Regex PascalCaseBoundary { get; }
}
