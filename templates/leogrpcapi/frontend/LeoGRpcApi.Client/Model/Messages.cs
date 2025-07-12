namespace LeoGRpcApi.Client.Model;

/// <summary>
///     Message to initiate the assignment of a ninja to a mission
/// </summary>
/// <param name="MissionId">ID of the mission to which to assign a ninja</param>
public sealed record AssignNinjaRequestMessage(long MissionId);

/// <summary>
///     Message to indicate the completion of a ninja assignment
/// </summary>
/// <param name="Succeeded">Flag indicating if assignment succeeded</param>
/// <param name="NinjaId">ID of the assigned ninja, null if none was assigned</param>
/// <param name="MissionId">ID of the mission a ninja was (or should have been) assigned to</param>
public sealed record AssignNinjaCompletedMessage(bool Succeeded, int? NinjaId, long MissionId);
