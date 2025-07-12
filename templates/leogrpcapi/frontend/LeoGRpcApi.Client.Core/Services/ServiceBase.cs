using Grpc.Core;
using LeoGRpcApi.Client.Core.Util;

namespace LeoGRpcApi.Client.Core.Services;

/// <summary>
///     Base class for gRPC service clients
/// </summary>
/// <param name="clientFactory">Factory for creating gRPC clients</param>
/// <typeparam name="TClient">Type of the primary endpoint client this service will use</typeparam>
internal abstract class ServiceBase<TClient>(GrpcClientFactory clientFactory)
    where TClient : ClientBase<TClient>
{
    /// <summary>
    ///     An instance of the primary gRPC client for this service
    /// </summary>
    protected readonly TClient ApiClient = clientFactory.CreateClient<TClient>();
}
