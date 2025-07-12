using LeoGRpcApi.Api.Persistence.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace LeoGRpcApi.Api.Persistence.Repositories;

/// <summary>
///     Repository for managing ninja entities
/// </summary>
public interface INinjaRepository
{
    /// <summary>
    ///     Adds a new ninja to the repository
    /// </summary>
    /// <param name="ninja">The ninja to add</param>
    public void AddNinja(Ninja ninja);

    /// <summary>
    ///     Attempts to retrieve a ninja by its ID
    /// </summary>
    /// <param name="id">The unique ID of the ninja</param>
    /// <returns>The ninja, if found</returns>
    public ValueTask<Ninja?> GetNinjaByIdAsync(int id);

    /// <summary>
    ///     Retrieves all ninja IDs from the repository
    /// </summary>
    /// <returns>Collection of all unique ninja IDs</returns>
    public ValueTask<IReadOnlyCollection<int>> GetAllNinjaIdsAsync();
}

internal sealed class NinjaRepository(DbSet<Ninja> ninjas) : INinjaRepository
{
    public void AddNinja(Ninja ninja)
    {
        ninjas.Add(ninja);
    }

    public async ValueTask<Ninja?> GetNinjaByIdAsync(int id)
    {
        IIncludableQueryable<Ninja, Mission?> query = ninjas.Include(n => n.CurrentMission);
        var ninja = await query.FirstOrDefaultAsync(n => n.Id == id);

        return ninja;
    }

    public async ValueTask<IReadOnlyCollection<int>> GetAllNinjaIdsAsync()
    {
        IOrderedQueryable<int> query = ninjas.Select(n => n.Id)
                                             .Order();
        List<int> ninjaIds = await query.ToListAsync();

        return ninjaIds;
    }
}
