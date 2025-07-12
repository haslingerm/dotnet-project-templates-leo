using LeoGRpcApi.Shared.ApiContract;

namespace LeoGRpcApi.Api.Persistence.Model;

/// <summary>
///     Represents a ninja entity
/// </summary>
public class Ninja
{
    /// <summary>
    ///     The unique identifier for the ninja
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     The code name of the ninja
    /// </summary>
    public required string CodeName { get; set; }

    /// <summary>
    ///     The rank of the ninja
    /// </summary>
    public NinjaRank Rank { get; set; }

    /// <summary>
    ///     The unique identifier of the current mission the ninja is assigned to, can be null if not assigned to any mission
    /// </summary>
    public long? CurrentMissionId { get; set; }

    /// <summary>
    ///     The list of weapon proficiencies the ninja has
    /// </summary>
    public required List<NinjaWeapon> WeaponProficiencies { get; set; } = [];

    /// <summary>
    ///     The list of special skills the ninja possesses
    /// </summary>
    public required List<string> SpecialSkills { get; set; } = [];

    /// <summary>
    ///     Reference to the current mission the ninja is assigned to - null if not assigned to any mission
    /// </summary>
    public Mission? CurrentMission { get; set; }
}
