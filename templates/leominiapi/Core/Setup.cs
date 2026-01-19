using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime.Serialization.SystemTextJson;

namespace LeoMiniApi.Core;

public static class Setup
{
    public const string CorsPolicyName = "CorsDefaultPolicy";
    
    extension(IServiceCollection services)
    {
        public void RegisterServices()
        {
            services.AddSingleton<IClock>(SystemClock.Instance);
        }

        public void ConfigureServices(bool isDevelopment)
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.WriteIndented = isDevelopment;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            });
        }

        public void ConfigureCors()
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    // not production/auth ready!
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
        }
    }
}
