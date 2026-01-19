using Grpc.Net.Client;
using LeoGRpcApi.Api.Persistence.Util;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace LeoGRpcApi.Api.TestInt.Util;

// ReSharper disable once ClassNeverInstantiated.Global - Instantiated by xUnit
public sealed class ApiTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:latest")
                                                              .WithDatabase("public")
                                                              .WithUsername("postgres")
                                                              .WithPassword("postgres")
                                                              .Build();

    public GrpcChannel Channel
    {
        get => field ?? throw new InvalidOperationException("Channel not created");
        private set;
    }

    public HttpClient Client
    {
        get => field ?? throw new InvalidOperationException("Client not created");
        private set;
    }

    public IClock Clock
    {
        get => field ?? throw new InvalidOperationException("Clock not created");
        private set;
    }

    private AppFactory Factory
    {
        get => field ?? throw new InvalidOperationException("Factory not created");
        set;
    }

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        Environment.SetEnvironmentVariable("INT_TESTING", "true");

        Factory = new AppFactory(_postgresContainer.GetConnectionString());
        Client = Factory.CreateClient();
        Clock = Factory.TestClock;
        Channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpClient = Client });
    }

    public async ValueTask DisposeAsync()
    {
        await Channel.ShutdownAsync();
        Channel.Dispose();
        await Factory.DisposeAsync();
        await _postgresContainer.StopAsync();
        await _postgresContainer.DisposeAsync();
    }

    public async ValueTask RestoreDatabaseAsync(Func<DatabaseContext, ValueTask> seedDataImporter)
    {
        await using var contextScope = CreateContextScope();

        // Drop schema, if it exists
        await contextScope.Context.Database
                          .ExecuteSqlRawAsync($"DROP SCHEMA IF EXISTS \"{DatabaseContext.SchemaName}\" CASCADE;");

        // Apply migrations and ensure the database is created
        await contextScope.Context.Database.MigrateAsync();

        await seedDataImporter(contextScope.Context);
    }

    public async ValueTask ModifyDatabaseContentAsync(Func<DatabaseContext, ValueTask> modifier)
    {
        await using var contextScope = CreateContextScope();

        await modifier(contextScope.Context);
    }

    private ContextScope CreateContextScope()
    {
        var serviceProvider = Factory.ServiceProvider;
        if (serviceProvider is null)
        {
            throw new InvalidOperationException("Service provider not set");
        }

        return new ContextScope(serviceProvider);
    }

    private sealed class ContextScope : IAsyncDisposable
    {
        private readonly IServiceScope _scope;

        public ContextScope(IServiceProvider serviceProvider)
        {
            _scope = serviceProvider.CreateScope();
            Context = _scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        }

        public DatabaseContext Context { get; }

        public async ValueTask DisposeAsync()
        {
            if (_scope is IAsyncDisposable scopeAsyncDisposable)
            {
                await scopeAsyncDisposable.DisposeAsync();
            }
            else
            {
                _scope.Dispose();
            }

            await Context.DisposeAsync();
        }
    }
}
