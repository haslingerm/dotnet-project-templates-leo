using System.Net;
using LeoWebApi.Endpoints;
using LeoWebApi.Persistence.Model;
using LeoWebApi.TestInt.Util;

namespace LeoWebApi.TestInt;

public sealed class RocketTests(WebApiTestFixture webApiFixture) : WebApiTestBase(webApiFixture)
{
    // Note: make sure you have created a migration before running the tests!

    [Fact]
    public async ValueTask GetAllRockets_ExistingRockets_Success()
    {
        const string Manufacturer1 = "Blue Origin";
        const string ModelName1 = "New Shepard";
        const string Manufacturer2 = "JAXA";
        const string ModelName2 = "H-IIA";

        await ModifyDatabaseContentAsync(async ctx =>
        {
            ctx.Rockets.Add(new Rocket
            {
                Manufacturer = Manufacturer1,
                ModelName = ModelName1,
                MaxThrust = 1_000_000,
                PayloadDeltaV = 44_000_000
            });
            ctx.Rockets.Add(new Rocket
            {
                Manufacturer = Manufacturer2,
                ModelName = ModelName2,
                MaxThrust = 1_500_000,
                PayloadDeltaV = 36_000_000
            });

            await ctx.SaveChangesAsync();
        });

        var response = await ApiClient.GetAsync("api/rockets", TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<RocketDto>? content
            = await response.Content.ReadFromJsonAsync<IReadOnlyList<RocketDto>>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull().And.HaveCount(2);
        content.Should().ContainSingle(r => r.Manufacturer == Manufacturer1 && r.ModelName == ModelName1);
        content.Should().ContainSingle(r => r.Manufacturer == Manufacturer2 && r.ModelName == ModelName2);
    }

    [Fact]
    public async ValueTask AddRocket_Success()
    {
        const string Manufacturer = "NASA";
        const string ModelName = "Saturn V";
        const double MaxThrust = 34_000_000D;
        const long PayloadDeltaV = 1_372_000_000;

        var response = await ApiClient.PostAsJsonAsync("api/rockets", new AddRocketRequest
        {
            Manufacturer = Manufacturer,
            ModelName = ModelName,
            MaxThrust = MaxThrust,
            PayloadDeltaV = PayloadDeltaV
        }, JsonOptions, TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var content = await response.Content.ReadFromJsonAsync<RocketDto>(TestCancellationToken);

        content.Should().NotBeNull();

        // the id is assigned by the database
        var expectedRocket = new
        {
            Manufacturer,
            ModelName,
            MaxThrust,
            PayloadDeltaV
        };

        content.Should().BeEquivalentTo(expectedRocket);
        content.Id.Should().BePositive();

        response = await ApiClient.GetAsync("api/rockets", TestCancellationToken);
        IReadOnlyList<RocketDto>? allRocketsContent
            = await response.Content.ReadFromJsonAsync<IReadOnlyList<RocketDto>>(JsonOptions, TestCancellationToken);

        allRocketsContent.Should().ContainSingle()
                         .Which.Should().BeEquivalentTo(expectedRocket);
    }
}
