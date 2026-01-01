using LeoWebApi.Persistence;
using LeoWebApi.Persistence.Util;
using LeoWebApi.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LeoWebApi.TestInt.Util;

internal sealed class WebAppFactory(string connectionString) : WebApplicationFactory<Program>
{
    public static readonly LocalDateTime CurrentDateTimeForTests = new(2026, 01, 01, 14, 30, 00);
    
    public IClock TestClock =>
        Services.GetService<IClock>() ??
        throw new InvalidOperationException("Service provider not available or clock not registered");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            SetTestDbContext(services);
            SetTestClock(services);
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

