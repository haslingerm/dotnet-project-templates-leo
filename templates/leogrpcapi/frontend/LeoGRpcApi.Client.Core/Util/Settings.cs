namespace LeoGRpcApi.Client.Core.Util;

/// <summary>
///     Application settings
/// </summary>
public class Settings
{
    public const string SectionKey = "General";
    
    public required string Host { get; set; }
}
