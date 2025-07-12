namespace LeoGRpcApi.Api.Persistence.Model;

/// <summary>
///     Represents a mission entity
/// </summary>
public class Mission
{
    /// <summary>
    ///     The unique identifier for the mission
    ///     Note: long is not really needed, only here to demonstrate int64 in the proto contract
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     The title of the mission
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    ///     An optional description of the mission
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     The level of danger associated with the mission in the range [0,1]
    /// </summary>
    public double Dangerousness { get; set; }

    /// <summary>
    ///     Ninjas currently assigned to this mission
    /// </summary>
    public required List<Ninja> AssignedNinjas { get; set; } = [];
}
