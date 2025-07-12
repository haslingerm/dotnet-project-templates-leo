using Grpc.Core;
using LeoGRpcApi.Api.Persistence.Model;
using LeoGRpcApi.Api.Persistence.Util;
using LeoGRpcApi.Api.TestInt.Util;
using LeoGRpcApi.Shared.ApiContract;

namespace LeoGRpcApi.Api.TestInt;

public sealed class MissionTests(ApiTestFixture apiFixture)
    : ApiTestBase<MissionMgmt.MissionMgmtClient>(apiFixture)
{
    private NinjaMgmt.NinjaMgmtClient NinjaGrpcClient => CreateGrpcClient<NinjaMgmt.NinjaMgmtClient>();

    [Fact]
    public async ValueTask GetAllMissions_Success()
    {
        var response = await GrpcClient.GetAllMissionsAsync(new GetAllRequest(),
                                                            cancellationToken: TestCancellationToken);
        response.Should().NotBeNull();
        response.Missions.Should().NotBeEmpty()
                .And.HaveCount(2);

        var firstMission = response.Missions.First(m => m.Id == 1L);
        firstMission.Dangerousness.Should().BeApproximately(0.62D, double.Epsilon);
        firstMission.Title.Should().Be("Shogun");
        firstMission.HasDescription.Should().BeTrue();
        firstMission.Description.Should().Be("Infiltrate the Shogun's palace and gather intelligence");

        var secondMission = response.Missions.First(m => m.Id == 2L);
        secondMission.Dangerousness.Should().BeApproximately(0.999D, double.Epsilon);
        secondMission.Title.Should().Be("Black Lotus");
        secondMission.HasDescription.Should().BeFalse();
    }

    [Fact]
    public async ValueTask CreateMission_Success()
    {
        const string Title = "Test Mission";
        const string Description = "This is a test mission";
        const double Dangerousness = 0.33D;
        const long ExpectedId = 3L;

        var request = new CreateMissionRequest
        {
            Title = Title,
            Description = Description,
            Dangerousness = Dangerousness
        };

        var response = await GrpcClient.CreateMissionAsync(request, cancellationToken: TestCancellationToken);
        response.Should().NotBeNull();
        response.Id.Should().Be(ExpectedId, "third mission");
        response.Title.Should().Be(request.Title);
        response.Description.Should().Be(request.Description);
        response.Dangerousness.Should().BeApproximately(request.Dangerousness, double.Epsilon);

        await AssertMissionValues(ExpectedId, Description, Dangerousness, Title);
    }

    [Theory]
    [InlineData("", 0.55D, "Title cannot be empty")]
    [InlineData("Valid Title", -0.1D, "Dangerousness cannot be lower than 0")]
    [InlineData("Valid Title", 1.1D, "Dangerousness cannot be higher than 1")]
    public async ValueTask CreateMission_Invalid(string title, double dangerousness, string reason)
    {
        var request = new CreateMissionRequest
        {
            Title = title,
            Dangerousness = dangerousness
            // description is optional, not checked
        };

        await AssertExceptionStatusAsync(async () =>
                                             await GrpcClient.CreateMissionAsync(request,
                                                  cancellationToken: TestCancellationToken),
                                         StatusCode.InvalidArgument, reason);
    }

    [Fact]
    public async ValueTask UpdateMission_Success()
    {
        const long MissionId = 1L;
        const string NewDesc = "Updated Mission Description";
        const double NewDangerousness = 0.1112D;

        var request = new UpdateMissionRequest
        {
            Id = MissionId,
            Dangerousness = NewDangerousness,
            Description = NewDesc
        };
        var response = await GrpcClient.UpdateMissionAsync(request, cancellationToken: TestCancellationToken);

        // no exception means success
        response.Should().NotBeNull();

        await AssertMissionValues(MissionId, NewDesc, NewDangerousness);
    }

    [Theory]
    [InlineData(-1L, 0.5D, "Mission ID must be positive")]
    [InlineData(0L, 0.5D, "Mission ID must be greater than 0")]
    [InlineData(1L, -0.5D, "Dangerousness must be >= 0")]
    [InlineData(1L, 1.5D, "Dangerousness must be <= 1")]
    public async ValueTask UpdateMission_Invalid(long id, double dangerousness, string reason)
    {
        var request = new UpdateMissionRequest
        {
            Id = id,
            Dangerousness = dangerousness
            // description is optional and not validated, not checked
        };

        await AssertExceptionStatusAsync(async () =>
                                             await GrpcClient.UpdateMissionAsync(request,
                                                  cancellationToken: TestCancellationToken),
                                         StatusCode.InvalidArgument, reason);
    }

    [Fact]
    public async ValueTask UpdateMission_NotFound()
    {
        var request = new UpdateMissionRequest
        {
            Id = 999L,
            Dangerousness = 0.5D,
            Description = "This mission does not exist"
        };

        await AssertExceptionStatusAsync(async () =>
                                             await GrpcClient.UpdateMissionAsync(request,
                                                  cancellationToken: TestCancellationToken),
                                         StatusCode.NotFound);
    }

    [Fact]
    public async ValueTask AssignMission_Success()
    {
        const long MissionId = 2L;
        const int NinjaId = 3;

        var request = new AssignMissionRequest
        {
            MissionId = MissionId,
            NinjaId = NinjaId
        };

        var response = await GrpcClient.AssignMissionAsync(request, cancellationToken: TestCancellationToken);
        response.Should().NotBeNull();
        response.Result.Should().Be(MissionAssignmentResult.Success);

        var ninja = await NinjaGrpcClient
            .GetNinjaByIdAsync(new GetNinjaByIdRequest { Id = NinjaId }, cancellationToken: TestCancellationToken);
        ninja.Should().NotBeNull();
        ninja.HasCurrentMission.Should().BeTrue();
        ninja.CurrentMission.Should().Be(MissionId, "Ninja should now be assigned to the mission");
    }

    [Theory]
    [InlineData(-1L, 3, "Mission ID must be positive")]
    [InlineData(0L, 3, "Mission ID must be > 0")]
    [InlineData(2L, -1, "Ninja ID must be positive")]
    [InlineData(2L, 0, "Ninja ID must be greater than 0")]
    public async ValueTask AssignMission_Invalid(long missionId, int ninjaId, string reason)
    {
        var request = new AssignMissionRequest
        {
            MissionId = missionId,
            NinjaId = ninjaId
        };

        await AssertExceptionStatusAsync(async () =>
                                             await GrpcClient.AssignMissionAsync(request,
                                                  cancellationToken: TestCancellationToken),
                                         StatusCode.InvalidArgument, reason);
    }

    [Theory]
    [InlineData(999L, 3, MissionAssignmentResult.MissionNotFound)]
    [InlineData(2L, 999, MissionAssignmentResult.NinjaNotFound)]
    public async ValueTask AssignMission_NotFound(long missionId, int ninjaId, MissionAssignmentResult expectedResult)
    {
        var request = new AssignMissionRequest
        {
            MissionId = missionId,
            NinjaId = ninjaId
        };

        var response = await GrpcClient.AssignMissionAsync(request, cancellationToken: TestCancellationToken);
        response.Should().NotBeNull();
        response.Result.Should().Be(expectedResult);
    }

    [Fact]
    public async ValueTask AssignMission_NinjaBusy()
    {
        const long MissionId = 2L;
        const int NinjaId = 1;

        var request = new AssignMissionRequest
        {
            MissionId = MissionId,
            NinjaId = NinjaId
        };

        var response = await GrpcClient.AssignMissionAsync(request, cancellationToken: TestCancellationToken);
        response.Should().NotBeNull();
        response.Result.Should().Be(MissionAssignmentResult.NinjaAlreadyOnMission);
    }

    [Fact]
    public async ValueTask DeleteMission_Success()
    {
        const long MissionId = 1L;

        var request = new DeleteMissionRequest { Id = MissionId };
        var response = await GrpcClient.DeleteMissionAsync(request, cancellationToken: TestCancellationToken);
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();

        var missions = await GrpcClient.GetAllMissionsAsync(new GetAllRequest(),
                                                            cancellationToken: TestCancellationToken);
        missions.Missions.Should().NotContain(m => m.Id == MissionId);

        var ninja = await NinjaGrpcClient
            .GetNinjaByIdAsync(new GetNinjaByIdRequest { Id = 1 }, cancellationToken: TestCancellationToken);
        ninja.Should().NotBeNull("Ninja still exists, even after mission deletion");
        ninja.HasCurrentMission.Should().BeFalse("Ninja should no longer be assigned to mission after deletion");
    }

    [Fact]
    public async ValueTask DeleteMission_NotFound()
    {
        var request = new DeleteMissionRequest { Id = 999L };

        var response = await GrpcClient.DeleteMissionAsync(request, cancellationToken: TestCancellationToken);
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
    }

    [Fact]
    public async ValueTask DeleteMission_Invalid()
    {
        var request = new DeleteMissionRequest { Id = -1L };

        await AssertExceptionStatusAsync(async () =>
                                             await GrpcClient.DeleteMissionAsync(request,
                                                  cancellationToken: TestCancellationToken),
                                         StatusCode.InvalidArgument, "Mission ID must be > 0");
    }

    protected override async ValueTask ImportSeedDataAsync(DatabaseContext context)
    {
        await base.ImportSeedDataAsync(context);

        context.Ninjas.Add(new Ninja
        {
            CodeName = "Hans",
            SpecialSkills = [],
            WeaponProficiencies = [NinjaWeapon.Sword],
            Rank = NinjaRank.Kage
        });

        await context.SaveChangesAsync();
    }

    private async ValueTask AssertMissionValues(long missionId, string? description, double dangerousness,
                                                string? title = null)
    {
        var missions = await GrpcClient.GetAllMissionsAsync(new GetAllRequest(),
                                                            cancellationToken: TestCancellationToken);
        var mission = missions.Missions.First(m => m.Id == missionId);
        if (title is not null)
        {
            mission.Title.Should().Be(title);
        }

        if (description is not null)
        {
            mission.HasDescription.Should().BeTrue();
            mission.Description.Should().Be(description);
        }
        else
        {
            mission.HasDescription.Should().BeFalse();
        }

        mission.Dangerousness.Should().BeApproximately(dangerousness, double.Epsilon);
    }
}
