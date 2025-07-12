using LeoGRpcApi.Api.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LeoGRpcApi.Api.Core.Util;

public static class CoreSetup
{
    public static void ConfigureCore(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);

        services.AddScoped<INinjaService, NinjaService>();
        services.AddScoped<IMissionService, MissionService>();
    }
}
