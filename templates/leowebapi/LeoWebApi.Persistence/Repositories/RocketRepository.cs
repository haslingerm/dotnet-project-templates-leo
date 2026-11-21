using LeoWebApi.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace LeoWebApi.Persistence.Repositories;

public interface IRocketRepository
{
    public Rocket AddRocket(string modelName, string manufacturer, double maxThrust, long totalDeltaV);
    public Task<IReadOnlyCollection<Rocket>> GetAllRocketsAsync(bool tracking);
    public Task<Rocket?> GetRocketByIdAsync(int id, bool tracking);
    public void RemoveRocket(Rocket rocket);
}

internal sealed class RocketRepository(DbSet<Rocket> rocketSet) : IRocketRepository
{
    private IQueryable<Rocket> Rockets => rocketSet;
    private IQueryable<Rocket> RocketsNoTracking => Rockets.AsNoTracking();

    public Rocket AddRocket(string modelName, string manufacturer, double maxThrust, long totalDeltaV)
    {
        var rocket = new Rocket
        {
            ModelName = modelName,
            Manufacturer = manufacturer,
            MaxThrust = maxThrust,
            PayloadDeltaV = totalDeltaV
        };

        rocketSet.Add(rocket);

        return rocket;
    }

    public async Task<IReadOnlyCollection<Rocket>> GetAllRocketsAsync(bool tracking)
    {
        IQueryable<Rocket> source = tracking ? Rockets : RocketsNoTracking;

        List<Rocket> rockets = await source.ToListAsync();

        return rockets;
    }

    public async Task<Rocket?> GetRocketByIdAsync(int id, bool tracking)
    {
        IQueryable<Rocket> source = tracking ? Rockets : RocketsNoTracking;

        var rocket = await source.FirstOrDefaultAsync(r => r.Id == id);

        return rocket;
    }

    public void RemoveRocket(Rocket rocket)
    {
        rocketSet.Remove(rocket);
    }
}
