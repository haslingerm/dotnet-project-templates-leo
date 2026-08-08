using Microsoft.Extensions.DependencyInjection;

namespace LeoGRpcApi.Api.Services.Util;

public static class ServicesSetup
{
    extension(IServiceCollection services)
    {
        public void ConfigureCore()
        {
            services.AddSingleton<IClock>(SystemClock.Instance);

            services.AddScoped<INinjaService, NinjaService>();
            services.AddScoped<IMissionService, MissionService>();
        }
    }
}
