using LeoGRpcApi.Api.Persistence.Model;
using LeoGRpcApi.Api.Persistence.Util;
using OneOf;
using OneOf.Types;

namespace LeoGRpcApi.Api.Core.Services;

/// <summary>
///     Provides methods to manage missions
/// </summary>
public interface IMissionService
{
    /// <summary>
    ///     Creates a new mission with the given title, description, and dangerousness level
    /// </summary>
    /// <param name="title">The title of the mission</param>
    /// <param name="description">The description of the mission</param>
    /// <param name="dangerousness">The dangerousness of the mission</param>
    /// <returns></returns>
    public ValueTask<Mission> CreateMissionAsync(string title, string? description, double dangerousness);

    /// <summary>
    ///     Retrieves all missions, without assigned ninjas
    /// </summary>
    /// <returns>A collection of all missions</returns>
    public ValueTask<IReadOnlyCollection<Mission>> GetAllMissionsAsync();

    /// <summary>
    ///     Attempts to delete a mission by its ID
    /// </summary>
    /// <param name="missionId">The unique ID of the mission to delete</param>
    /// <returns>Success if the mission was found and deleted, or an information that the mission was not found</returns>
    public ValueTask<OneOf<Success, NotFound>> DeleteMissionAsync(long missionId);

    /// <summary>
    ///     Updates the description and dangerousness of a mission
    /// </summary>
    /// <param name="missionId">The unique ID of the mission to update</param>
    /// <param name="description">The new description of the mission</param>
    /// <param name="dangerousness">The new dangerousness of the mission</param>
    /// <returns>A result indicating if the mission was successfully updated or not found</returns>
    public ValueTask<OneOf<Success, NotFound>> UpdateMissionAsync(long missionId, string? description,
                                                                  double dangerousness);

    /// <summary>
    ///     Assigns a ninja to a mission.
    ///     Ninja must not be already on mission to be able to be assigned.
    /// </summary>
    /// <param name="missionId">The unique ID of the mission</param>
    /// <param name="ninjaId">The unique ID of the ninja</param>
    /// <returns>Detailed result status indicating success or error reason</returns>
    public ValueTask<OneOf<Success, MissionNotFound, NinjaNotFound, NinjaBusy>> AssignMissionAsync(
        long missionId, int ninjaId);

    /// <summary>
    ///     Indicates that the requested mission was not found
    /// </summary>
    public readonly record struct MissionNotFound;

    /// <summary>
    ///     Indicates that the requested ninja was not found
    /// </summary>
    public readonly record struct NinjaNotFound;

    /// <summary>
    ///     Indicates that the requested ninja is already on a mission and cannot be assigned to another one
    /// </summary>
    public readonly record struct NinjaBusy;
}

internal sealed class MissionService(
    IUnitOfWork uow,
    ILogger<MissionService> logger,
    INinjaService ninjaService) : IMissionService
{
    public async ValueTask<Mission> CreateMissionAsync(string title, string? description, double dangerousness)
    {
        var mission = new Mission
        {
            Title = title,
            Description = description,
            Dangerousness = dangerousness,
            AssignedNinjas = []
        };

        uow.MissionRepository.AddMission(mission);
        await uow.SaveChangesAsync();

        logger.LogInformation("Created new mission with id {MissionId}", mission.Id);

        return mission;
    }

    public async ValueTask<IReadOnlyCollection<Mission>> GetAllMissionsAsync()
    {
        IReadOnlyCollection<Mission> allMissions = await uow.MissionRepository.GetAllMissionsAsync();

        return allMissions;
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteMissionAsync(long missionId)
    {
        var mission = await uow.MissionRepository.GetMissionByIdAsync(missionId);
        if (mission is null)
        {
            logger.LogWarning("Mission with id {MissionId} not found", missionId);

            return new NotFound();
        }

        uow.MissionRepository.RemoveMission(mission);
        await uow.SaveChangesAsync();

        logger.LogInformation("Deleted mission with id {MissionId}", missionId);

        return new Success();
    }

    public async ValueTask<OneOf<Success, NotFound>> UpdateMissionAsync(long missionId, string? description,
                                                                        double dangerousness)
    {
        var mission = await uow.MissionRepository.GetMissionByIdAsync(missionId);
        if (mission is null)
        {
            logger.LogWarning("Mission with id {MissionId} not found", missionId);

            return new NotFound();
        }

        mission.Description = description;
        mission.Dangerousness = dangerousness;

        await uow.SaveChangesAsync();

        logger.LogInformation("Updated mission with id {MissionId} with description {Desc} and dangerousness {Danger}",
                              missionId, description, dangerousness);

        return new Success();
    }

    public async ValueTask<OneOf<Success, IMissionService.MissionNotFound, IMissionService.NinjaNotFound,
        IMissionService.NinjaBusy>> AssignMissionAsync(long missionId, int ninjaId)
    {
        var ninja = await GetNinjaAsync();
        if (ninja is null)
        {
            logger.LogWarning("Ninja with id {NinjaId} could not be assigned to mission {MissionId}, because it was not found",
                              ninjaId, missionId);

            return new IMissionService.NinjaNotFound();
        }

        if (ninja.CurrentMission is not null)
        {
            logger.LogWarning("Ninja with id {NinjaId} could not be assigned to mission {MissionId}, because they are already on a mission",
                              ninjaId, missionId);

            return new IMissionService.NinjaBusy();
        }

        var mission = await uow.MissionRepository.GetMissionByIdAsync(missionId);
        if (mission is null)
        {
            logger.LogWarning("Mission with id {MissionId} could not be assigned to ninja {NinjaId}, because it was not found",
                              missionId, ninjaId);

            return new IMissionService.MissionNotFound();
        }

        mission.AssignedNinjas.Add(ninja);
        await uow.SaveChangesAsync();

        logger.LogInformation("Assigned ninja with id {NinjaId} to mission with id {MissionId}", ninjaId, missionId);

        return new Success();

        async ValueTask<Ninja?> GetNinjaAsync()
        {
            OneOf<Ninja, NotFound> ninjaResult = await ninjaService.GetNinjaByIdAsync(ninjaId);

            return ninjaResult.Match<Ninja?>(n => n,
                                             notFound => null);
        }
    }
}
