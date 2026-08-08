namespace LeoGRpcApi.Api.Services.Util;

public sealed class Settings
{
    public const string SectionKey = "General";

    public required string ClientOrigin { get; init; }
}
