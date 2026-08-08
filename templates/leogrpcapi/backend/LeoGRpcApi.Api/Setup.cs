using LeoGRpcApi.Api.Endpoints;
using LeoGRpcApi.Api.Services.Util;
using LeoGRpcApi.Api.Persistence.Util;
using LeoGRpcApi.Api.Util;
using Serilog;

namespace LeoGRpcApi.Api;

public static class Setup
{
    extension(IEndpointRouteBuilder app)
    {
        public void MapApplicationEndpoints()
        {
            app.MapGrpcService<NinjaMgmtEndpoint>();
            app.MapGrpcService<MissionMgmtEndpoint>();
        }
    }

    extension(IServiceCollection services)
    {
        public void AddApplicationServices(IConfigurationManager configurationManager,
                                           bool isDev)
        {
            services.ConfigurePersistence(configurationManager, isDev);
            services.ConfigureCore();
        }

        public Settings LoadAndConfigureSettings(IConfigurationManager configurationManager)
        {
            var configSection = configurationManager.GetSection(Settings.SectionKey);

            services.Configure<Settings>(s => configSection.Bind(s));

            // different instance, but the same values - used for startup config outside of DI context
            var settings = Activator.CreateInstance<Settings>();
            configSection.Bind(settings);

            return settings;
        }
        
        public void ConfigureGrpc(bool isDev)
        {
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<ExceptionInterceptor>();

                // exception details would be sent to the client, so this must never be on outside of development
                options.EnableDetailedErrors = isDev;
            });
        }
    }

    extension(WebApplicationBuilder builder)
    {
        public void AddLogging()
        {
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog((_, _, config) =>
            {
                config
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            });
        }
    }
}
