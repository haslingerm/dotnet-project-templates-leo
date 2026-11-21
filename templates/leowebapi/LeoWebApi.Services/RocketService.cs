using LeoWebApi.Persistence.Model;
using LeoWebApi.Persistence.Util;
using OneOf;
using OneOf.Types;

namespace LeoWebApi.Services;

public interface IRocketService
{
    public Task<Rocket> AddRocketAsync(string manufacturer, string modelName, double maxThrust, long payloadDeltaV);
    public Task<OneOf<Rocket, NotFound>> GetRocketByIdAsync(int id, bool tracking);
    public Task<IReadOnlyCollection<Rocket>> GetAllRocketsAsync();
    public Task<OneOf<Success, NotFound>> DeleteRocketAsync(int id);
}

internal sealed class RocketService(IUnitOfWork uow) : IRocketService
{
    public async Task<Rocket> AddRocketAsync(string manufacturer, string modelName, double maxThrust, long payloadDeltaV)
    {
        var rocket = uow.RocketRepository.AddRocket(modelName, manufacturer, maxThrust, payloadDeltaV);

        await uow.SaveChangesAsync();

        return rocket;
    }

    public async Task<OneOf<Rocket, NotFound>> GetRocketByIdAsync(int id, bool tracking)
    {
        var rocket = await uow.RocketRepository.GetRocketByIdAsync(id, tracking);

        return rocket is null ? new NotFound() : rocket;
    }

    public async Task<IReadOnlyCollection<Rocket>> GetAllRocketsAsync()
    {
        IReadOnlyCollection<Rocket> rockets = await uow.RocketRepository.GetAllRocketsAsync(false);

        return rockets;
    }

    public async Task<OneOf<Success, NotFound>> DeleteRocketAsync(int id)
    {
        var rocket = await uow.RocketRepository.GetRocketByIdAsync(id, true);

        if (rocket is null)
        {
            return new NotFound();
        }

        uow.RocketRepository.RemoveRocket(rocket);

        await uow.SaveChangesAsync();

        return new Success();
    }
}
