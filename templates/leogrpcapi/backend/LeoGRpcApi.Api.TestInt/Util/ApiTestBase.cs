using AwesomeAssertions.Specialized;
using Grpc.Core;
using LeoGRpcApi.Api.Persistence.Model;
using LeoGRpcApi.Api.Persistence.Util;
using LeoGRpcApi.Shared.ApiContract;

namespace LeoGRpcApi.Api.TestInt.Util;

public abstract class ApiTestBase<TClient> : IClassFixture<ApiTestFixture>, IAsyncLifetime
    where TClient : ClientBase<TClient>
{
    private readonly ApiTestFixture _fixture;

    public ApiTestBase(ApiTestFixture apiFixture)
    {
        _fixture = apiFixture;
        GrpcClient = CreateGrpcClient<TClient>();
    }

    protected IClock TestClock => _fixture.Clock;
    protected CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;
    protected TClient GrpcClient { get; private set; }

    public async ValueTask InitializeAsync()
    {
        await _fixture.RestoreDatabaseAsync(ImportSeedDataAsync);
    }

    public ValueTask DisposeAsync()
    {
        // nothing to dispose
        // database is reset during init to ensure a clean slate even if a test run is interrupted
        return ValueTask.CompletedTask;
    }

    protected TRpcClient CreateGrpcClient<TRpcClient>() where TRpcClient : ClientBase<TRpcClient>
    {
        object? instance = Activator.CreateInstance(typeof(TRpcClient), _fixture.Channel);
        if (instance is not TRpcClient client)
        {
            throw new InvalidOperationException($"Could not create gRPC client of type {typeof(TRpcClient).Name}");
        }

        return client;
    }

    protected virtual async ValueTask ImportSeedDataAsync(DatabaseContext context)
    {
        var shadow = new Ninja
        {
            CodeName = "Shadow",
            SpecialSkills = ["invisible"],
            WeaponProficiencies = [NinjaWeapon.Sword, NinjaWeapon.Bow],
            Rank = NinjaRank.Jonin
        };
        context.Ninjas.Add(shadow);
        await context.SaveChangesAsync(TestCancellationToken);
        context.Ninjas.Add(new Ninja
        {
            CodeName = "Storm",
            SpecialSkills = [],
            WeaponProficiencies = [NinjaWeapon.Shuriken],
            Rank = NinjaRank.Chunin
        });
        context.Missions.AddRange(new Mission
                                  {
                                      Title = "Shogun",
                                      Description = "Infiltrate the Shogun's palace and gather intelligence",
                                      Dangerousness = 0.62D,
                                      AssignedNinjas = [shadow]
                                  },
                                  new Mission
                                  {
                                      Title = "Black Lotus",
                                      Description = null,
                                      Dangerousness = 0.999D,
                                      AssignedNinjas = []
                                  });

        await context.SaveChangesAsync(TestCancellationToken);
    }

    protected async ValueTask ModifyDatabaseContentAsync(Func<DatabaseContext, ValueTask> modifier)
    {
        await _fixture.ModifyDatabaseContentAsync(modifier);
    }

    protected async ValueTask AssertExceptionStatusAsync<TResponse>(Func<Task<TResponse>> action,
                                                                    StatusCode expectedStatusCode,
                                                                    string? exceptionReason = null)
    {
        ExceptionAssertions<RpcException>? exceptionAssertion
            = await action.Should().ThrowAsync<RpcException>(exceptionReason ?? string.Empty);
        exceptionAssertion?.And.Should().NotBeNull();
        exceptionAssertion?.Which.Status.StatusCode.Should().Be(expectedStatusCode);
    }
}
