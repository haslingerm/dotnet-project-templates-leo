namespace LeoGRpcApi.Api.Core.Util;

public sealed class Settings
{
    public const string SectionKey = "General";

    public required string ClientOrigin { get; init; }
}
