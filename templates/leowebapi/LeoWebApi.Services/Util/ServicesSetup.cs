using Microsoft.Extensions.DependencyInjection;

namespace LeoWebApi.Services.Util;

public static class ServicesSetup
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
