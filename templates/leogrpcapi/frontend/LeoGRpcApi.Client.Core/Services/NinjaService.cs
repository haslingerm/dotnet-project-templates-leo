using LeoGRpcApi.Client.Core.Util;
using LeoGRpcApi.Shared.ApiContract;

namespace LeoGRpcApi.Client.Core.Services;

/// <summary>
///     Allows calling remote methods for managing ninjas
/// </summary>
public interface INinjaService
{
    /// <summary>
    ///     Gets all ninja IDs
    /// </summary>
    /// <returns>Collection of all ninja IDs</returns>
    public ValueTask<IReadOnlyCollection<int>> GetAllNinjaIdsAsync();

    /// <summary>
    ///     Retrieves a ninja by its ID.
    ///     Will throw an exception if the ninja does not exist.
    /// </summary>
    /// <param name="id">Unique ID of the ninja</param>
    /// <returns>The ninja DTO</returns>
    public ValueTask<NinjaDto> GetNinjaByIdAsync(int id);
}

internal sealed class NinjaService(GrpcClientFactory clientFactory)
    : ServiceBase<NinjaMgmt.NinjaMgmtClient>(clientFactory), INinjaService
{
    public async ValueTask<IReadOnlyCollection<int>> GetAllNinjaIdsAsync()
    {
        var request = new GetAllRequest();
        var response = await ApiClient.GetAllNinjaIdsAsync(request);

        return response.NinjaIds.ToList();
    }

    public async ValueTask<NinjaDto> GetNinjaByIdAsync(int id)
    {
        var request = new GetNinjaByIdRequest { Id = id };
        // no need to construct a route, we simply call a method remotely
        var response = await ApiClient.GetNinjaByIdAsync(request);

        return response;
    }
}
