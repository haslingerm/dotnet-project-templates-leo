using LeoGRpcApi.Api.Persistence.Util;
using LeoGRpcApi.Api.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LeoGRpcApi.Api.TestInt.Util;

internal sealed class AppFactory(string connectionString) : WebApplicationFactory<Program>
{
    public static readonly LocalDateTime CurrentDateTimeForTests = new(2025, 01, 01, 14, 30, 00);
    public IServiceProvider? ServiceProvider { get; private set; }
    
    public IClock TestClock =>
        ServiceProvider?.GetService<IClock>() ??
        throw new InvalidOperationException("Service provider not available or clock not registered");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            SetTestDbContext(services);
            SetTestClock(services);

            ServiceProvider = services.BuildServiceProvider();
        });
    }

    private void SetTestDbContext(IServiceCollection services)
    {
        RemoveServiceIfExists<DbContextOptions<DatabaseContext>>(services);

        services.AddDbContext<DatabaseContext>(options => PersistenceSetup.ConfigureDatabaseContextOptions(options,
                                                connectionString, false));
    }

    private static void SetTestClock(IServiceCollection services)
    {
        RemoveServiceIfExists<IClock>(services);

        var clockMock = Substitute.For<IClock>();
        var currentInstant = CurrentDateTimeForTests.InZoneLeniently(Const.TimeZone).ToInstant();
        clockMock.GetCurrentInstant().Returns(currentInstant);

        services.AddSingleton(clockMock);
    }

    private static void RemoveServiceIfExists<TService>(IServiceCollection services)
    {
        var descriptorType = typeof(TService);

        var descriptor = services
            .SingleOrDefault(s => s.ServiceType == descriptorType);

        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }
    }
}

