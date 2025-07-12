using System.Text.Json;
using LeoWebApi.Persistence.Util;
using LeoWebApi.Shared;

namespace LeoWebApi.TestInt.Util;

public abstract class WebApiTestBase(WebApiTestFixture webApiFixture) : IClassFixture<WebApiTestFixture>, IAsyncLifetime
{
    private static readonly Lazy<JsonSerializerOptions> jsonOptions = new(() =>
    {
        var options = new JsonSerializerOptions(JsonSerializerOptions.Web);
        JsonConfig.ConfigureJsonSerialization(options, false);

        return options;
    });

    protected static JsonSerializerOptions JsonOptions => jsonOptions.Value;

    protected HttpClient ApiClient => webApiFixture.Client;
    protected IClock TestClock => webApiFixture.Clock;
    protected CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;
    
    public async ValueTask InitializeAsync()
    {
        await webApiFixture.RestoreDatabaseAsync(ImportSeedDataAsync);
    }

    public ValueTask DisposeAsync()
    {
        // nothing to dispose
        // database is reset during init to ensure a clean slate even if a test run is interrupted
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask ImportSeedDataAsync(DatabaseContext context)
    {
        // add seed data for all tests here if needed, override in derived classes to add test class specific seed data
        return ValueTask.CompletedTask;
    }

    protected async ValueTask ModifyDatabaseContentAsync(Func<DatabaseContext, ValueTask> modifier)
    {
        await webApiFixture.ModifyDatabaseContentAsync(modifier);
    }
}
