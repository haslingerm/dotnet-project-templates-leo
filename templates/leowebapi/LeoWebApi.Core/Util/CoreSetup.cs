using LeoWebApi.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LeoWebApi.Core.Util;

public static class CoreSetup
{
    extension(IServiceCollection services)
    {
        public void ConfigureCore()
        {
            services.AddSingleton<IClock>(SystemClock.Instance);
            
            services.AddScoped<IRocketService, RocketService>();
        }
    }
}
