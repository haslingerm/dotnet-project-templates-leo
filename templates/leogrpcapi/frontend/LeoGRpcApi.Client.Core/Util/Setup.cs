using LeoGRpcApi.Client.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LeoGRpcApi.Client.Core.Util;

/// <summary>
///     Core services setup
/// </summary>
public static class Setup
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCoreServices()
        {
            services.AddSingleton<GrpcClientFactory>();
            services.AddSingleton<INinjaService, NinjaService>();
            services.AddSingleton<IMissionService, MissionService>();

            return services;
        }
    }
}
