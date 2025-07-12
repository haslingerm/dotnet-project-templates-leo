using LeoGRpcApi.Api.Core.Util;
using LeoGRpcApi.Api.Persistence.Util;
using Serilog;

namespace LeoGRpcApi.Api;

public static class Setup
{
    public static void AddApplicationServices(this IServiceCollection services,
                                              IConfigurationManager configurationManager,
                                              bool isDev)
    {
        services.ConfigurePersistence(configurationManager, isDev);
        services.ConfigureCore();
    }

    public static Settings LoadAndConfigureSettings(this IServiceCollection services,
                                                    IConfigurationManager configurationManager)
    {
        var configSection = configurationManager.GetSection(Settings.SectionKey);

        services.Configure<Settings>(s => configSection.Bind(s));

        // different instance, but the same values - used for startup config outside of DI context
        var settings = Activator.CreateInstance<Settings>();
        configSection.Bind(settings);

        return settings;
    }

    public static void AddLogging(this WebApplicationBuilder builder)
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
