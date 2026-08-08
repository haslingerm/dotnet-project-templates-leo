namespace LeoWebApi.Services.Util;

public sealed class Settings
{
    public const string SectionKey = "General";
    public double MaxPayloadWeight { get; init; }
    public IReadOnlyList<string> ValidLaunchSites { get; init; } = [];
    public required string ClientOrigin { get; init; }
}
