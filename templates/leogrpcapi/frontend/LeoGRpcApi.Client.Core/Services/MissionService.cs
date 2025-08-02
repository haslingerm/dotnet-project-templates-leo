using LeoGRpcApi.Client.Core.Util;
using LeoGRpcApi.Shared.ApiContract;
using LeoGRpcApi.Shared.ApiContract.Validation;

namespace LeoGRpcApi.Client.Core.Services;

/// <summary>
///     Allows calling remote methods for managing missions
/// </summary>
public interface IMissionService
{
    /// <summary>
    ///     Retrieves all missions
    /// </summary>
    /// <returns>A collection of mission DTOs</returns>
    public ValueTask<IReadOnlyCollection<MissionDto>> GetAllMissionsAsync();

    /// <summary>
    ///     Updates a mission with the given ID.
    ///     Throws an exception if the update fails.
    /// </summary>
    /// <param name="missionId">The ID of the mission to update</param>
    /// <param name="dangerousness">The new dangerousness of the mission</param>
    /// <param name="description">The new description of the mission - optional</param>
    public ValueTask UpdateMissionAsync(long missionId, double dangerousness, string? description);

    /// <summary>
    ///     Deletes a mission with the given ID
    /// </summary>
    /// <param name="missionId">ID of the mission to delete</param>
    /// <returns>A flag indicating if the delete operation was successful or not</returns>
    public ValueTask<bool> DeleteMissionAsync(long missionId);

    /// <summary>
    ///     Assigns a ninja to a mission.
    ///     The ninja must not already be assigned to another mission.
    /// </summary>
    /// <param name="missionId">The ID of the mission to which to assign the ninja to</param>
    /// <param name="ninjaId">The ID of the ninja to assign</param>
    /// <returns>A detailed operation result</returns>
    public ValueTask<MissionAssignmentResult> AssignNinjaToMissionAsync(long missionId, int ninjaId);
}

internal sealed class MissionService(GrpcClientFactory clientFactory)
    : ServiceBase<MissionMgmt.MissionMgmtClient>(clientFactory), IMissionService
{
    public async ValueTask<IReadOnlyCollection<MissionDto>> GetAllMissionsAsync()
    {
        var request = new GetAllRequest();
        var response = await ApiClient.GetAllMissionsAsync(request);

        return response.Missions.ToList();
    }

    public async ValueTask UpdateMissionAsync(long missionId, double dangerousness, string? description)
    {
        var request = new UpdateMissionRequest
        {
            Id = missionId,
            Dangerousness = dangerousness,
            Description = description
        };
        ThrowIfInvalid<UpdateMissionRequest, UpdateMissionRequestValidator>(request);

        await ApiClient.UpdateMissionAsync(request);
    }

    public async ValueTask<bool> DeleteMissionAsync(long missionId)
    {
        var request = new DeleteMissionRequest { Id = missionId };
        ThrowIfInvalid<DeleteMissionRequest, DeleteMissionRequestValidator>(request);
        
        var result = await ApiClient.DeleteMissionAsync(request);

        return result.Success;
    }

    public async ValueTask<MissionAssignmentResult> AssignNinjaToMissionAsync(long missionId, int ninjaId)
    {
        var request = new AssignMissionRequest
        {
            MissionId = missionId,
            NinjaId = ninjaId
        };
        ThrowIfInvalid<AssignMissionRequest, AssignMissionRequestValidator>(request);
        
        var response = await ApiClient.AssignMissionAsync(request);

        return response.Result;
    }
}
