using LeoMiniApi.Endpoints;
using LeoMiniApi.Util;

namespace LeoMiniApi;

public static class Setup
{
    public const string CorsPolicyName = "DefaultCorsPolicy";

    extension(IEndpointRouteBuilder app)
    {
        public void MapApplicationEndpoints()
        {
            app.MapGreetingEndpoints();
        }
    }

    extension(IServiceCollection services)
    {
        public void AddApplicationServices()
        {
            services.AddSingleton<IClock>(SystemClock.Instance);
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
        
        public void ConfigureProblemDetails()
        {
            services.AddProblemDetails();
            services.AddExceptionHandler<GlobalExceptionHandler>();
        }
    }
}
