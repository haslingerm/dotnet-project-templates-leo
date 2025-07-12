using LeoGRpcApi.Api.Persistence.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace LeoGRpcApi.Api.Persistence.Repositories;

/// <summary>
///     A repository for managing mission entities
/// </summary>
public interface IMissionRepository
{
    /// <summary>
    ///     Adds a new mission to the repository
    /// </summary>
    /// <param name="mission">The mission to add</param>
    public void AddMission(Mission mission);

    /// <summary>
    ///     Attempts to retrieve a mission by its ID
    /// </summary>
    /// <param name="id">Unique ID of the mission</param>
    /// <returns>The mission, if found</returns>
    public ValueTask<Mission?> GetMissionByIdAsync(long id);

    /// <summary>
    ///     Removes a mission from the repository
    /// </summary>
    /// <param name="mission">The mission to remove</param>
    public void RemoveMission(Mission mission);

    /// <summary>
    ///     Retrieves all missions from the repository, without assigned ninjas and not tracking changes
    /// </summary>
    /// <returns>Collection of all missions</returns>
    public ValueTask<IReadOnlyCollection<Mission>> GetAllMissionsAsync();
}

internal sealed class MissionRepository(DbSet<Mission> missions) : IMissionRepository
{
    public void AddMission(Mission mission)
    {
        missions.Add(mission);
    }

    public async ValueTask<Mission?> GetMissionByIdAsync(long id)
    {
        IIncludableQueryable<Mission, List<Ninja>> query = missions
            .Include(m => m.AssignedNinjas);
        var mission = await query.FirstOrDefaultAsync(m => m.Id == id);

        return mission;
    }

    public void RemoveMission(Mission mission)
    {
        missions.Remove(mission);
    }

    public async ValueTask<IReadOnlyCollection<Mission>> GetAllMissionsAsync()
    {
        List<Mission> allMissions = await missions.AsNoTracking().ToListAsync();

        return allMissions;
    }
}
