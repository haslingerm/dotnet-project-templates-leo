using FluentValidation;
using LeoWebApi.Endpoints;
using LeoWebApi.Services.Util;
using LeoWebApi.Persistence.Util;
using LeoWebApi.Util;
using Serilog;

namespace LeoWebApi;

public static class Setup
{
    public const string CorsPolicyName = "DefaultCorsPolicy";

    /// <summary>
    ///     Aligns FluentValidation with the JSON contract
    /// </summary>
    public static void ConfigureValidation()
    {
        ValidatorOptions.Global.PropertyNameResolver = (_, member, _) => member is null
        ? null
        : ValidationNaming.ToPropertyName(member.Name);

        ValidatorOptions.Global.DisplayNameResolver = (_, member, _) => member is null
        ? null
        : ValidationNaming.ToDisplayName(member.Name);
    }

    extension(IEndpointRouteBuilder app)
    {
        /// <summary>
        ///     Maps the endpoints of every resource of this API.
        ///     Add one call per resource here - no reflection!
        /// </summary>
        public void MapApplicationEndpoints()
        {
            app.MapRocketEndpoints();
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

    extension(IServiceCollection services)
    {
        public void AddCors(Settings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.ClientOrigin))
            {
                throw new InvalidOperationException("Client origin has to be configured");
            }

            services.AddCors(o => o.AddPolicy(CorsPolicyName, builder =>
            {
                builder.WithOrigins(settings.ClientOrigin)
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            }));

            Log.Logger.Debug("Added CORS policy with client origin {ClientOrigin}", settings.ClientOrigin);
        }

        /// <summary>
        ///     Registers the problem details services, so that unhandled exceptions are answered with the same
        ///     error shape as failed validations
        /// </summary>
        public void ConfigureProblemDetails()
        {
            services.AddProblemDetails();
            services.AddExceptionHandler<GlobalExceptionHandler>();
        }

        public void ConfigureAdditionalRouteConstraints()
        {
            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add(nameof(LocalDate),
                                          typeof(LocalDateRouteConstraint));
            });
        }
    }
}
