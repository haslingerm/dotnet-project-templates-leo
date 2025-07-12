using Grpc.Core;
using LeoGRpcApi.Api.TestInt.Util;
using LeoGRpcApi.Shared.ApiContract;

namespace LeoGRpcApi.Api.TestInt;

public sealed class NinjaTests(ApiTestFixture apiFixture)
    : ApiTestBase<NinjaMgmt.NinjaMgmtClient>(apiFixture)
{
    [Fact]
    public async ValueTask GetNinjaById_WithCurrentMission()
    {
        const int NinjaId = 1;

        var response = await GrpcClient.GetNinjaByIdAsync(new GetNinjaByIdRequest { Id = NinjaId },
                                                          cancellationToken: TestCancellationToken);
        response.Should().NotBeNull();
        response.Id.Should().Be(NinjaId);
        response.CodeName.Should().Be("Shadow");
        response.Rank.Should().Be(NinjaRank.Jonin);
        response.WeaponProficiencies.Should().Contain([NinjaWeapon.Sword, NinjaWeapon.Bow]);
        response.SpecialSkills.Should().Contain("invisible");
        response.HasCurrentMission.Should().BeTrue();
        response.CurrentMission.Should().Be(1L, "ID of the assigned mission");
    }

    [Fact]
    public async ValueTask GetNinjaById_NoCurrentMission()
    {
        const int NinjaId = 2;

        var response = await GrpcClient.GetNinjaByIdAsync(new GetNinjaByIdRequest { Id = NinjaId },
                                                          cancellationToken: TestCancellationToken);
        response.Should().NotBeNull();
        response.Id.Should().Be(NinjaId);
        response.CodeName.Should().Be("Storm");
        response.Rank.Should().Be(NinjaRank.Chunin);
        response.WeaponProficiencies.Should().Contain(NinjaWeapon.Shuriken);
        response.SpecialSkills.Should().BeEmpty();
        response.HasCurrentMission.Should().BeFalse();
    }

    [Fact]
    public async ValueTask GetNinjaById_NotFound()
    {
        const int NinjaId = 999;

        await AssertExceptionStatusAsync(async () =>
                                             await GrpcClient
                                                 .GetNinjaByIdAsync(new GetNinjaByIdRequest { Id = NinjaId },
                                                                    cancellationToken: TestCancellationToken),
                                         StatusCode.NotFound);
    }

    [Fact]
    public async ValueTask GetNinjaById_Invalid()
    {
        await AssertExceptionStatusAsync(async () =>
                                             await GrpcClient.GetNinjaByIdAsync(new GetNinjaByIdRequest { Id = -1 },
                                                                                cancellationToken:
                                                                                TestCancellationToken),
                                         StatusCode.InvalidArgument);
    }

    [Fact]
    public async ValueTask GetAllNinjas_Success()
    {
        var response
            = await GrpcClient.GetAllNinjaIdsAsync(new GetAllRequest(), cancellationToken: TestCancellationToken);
        response.NinjaIds.Should().NotBeEmpty()
                .And.HaveCount(2);
    }

    [Fact]
    public async ValueTask CreateNinja_Success()
    {
        const NinjaRank Rank = NinjaRank.Genin;
        const string CodeName = "Teppanyaki";
        const NinjaWeapon Weapon = NinjaWeapon.Staff;
        const string SpecialSkill = "cooking noodles";
        const int ExpectedId = 3;

        var request = new CreateNinjaRequest
        {
            Rank = Rank,
            CodeName = CodeName,
            WeaponProficiencies = { Weapon },
            SpecialSkills = { SpecialSkill }
        };

        var response = await GrpcClient.CreateNinjaAsync(request, cancellationToken: TestCancellationToken);
        response.Id.Should().Be(ExpectedId, "third ninja in the database");

        var ninja = await GrpcClient.GetNinjaByIdAsync(new GetNinjaByIdRequest { Id = response.Id },
                                                       cancellationToken: TestCancellationToken);
        ninja.Should().NotBeNull();
        ninja.Id.Should().Be(ExpectedId);
        ninja.CodeName.Should().Be(CodeName);
        ninja.Rank.Should().Be(Rank);
        ninja.WeaponProficiencies.Should().ContainSingle().Which.Should().Be(Weapon);
        ninja.SpecialSkills.Should().ContainSingle().Which.Should().Be(SpecialSkill);
    }

    [Theory]
    [MemberData(nameof(CreateNinjaInvalidData))]
    public async ValueTask CreateNinja_Invalid(NinjaRank rank, string codeName, IEnumerable<NinjaWeapon> weapons,
                                               IEnumerable<string> specialSkills, string reason)
    {
        var request = new CreateNinjaRequest
        {
            Rank = rank,
            CodeName = codeName,
            WeaponProficiencies = { weapons },
            SpecialSkills = { specialSkills }
        };

        await AssertExceptionStatusAsync(async () =>
                                             await GrpcClient.CreateNinjaAsync(request,
                                                                               cancellationToken:
                                                                               TestCancellationToken),
                                         StatusCode.InvalidArgument, reason);
    }

    public static TheoryData<NinjaRank, string, IEnumerable<NinjaWeapon>, IEnumerable<string>, string>
        CreateNinjaInvalidData()
    {
        const NinjaRank ValidRank = NinjaRank.Genin;
        const string ValidCodeName = "Tonkatsu";
        NinjaWeapon[] validWeapons = [NinjaWeapon.Nunchaku];
        string[] validSkills = ["poison making"];

        return new TheoryData<NinjaRank, string, IEnumerable<NinjaWeapon>, IEnumerable<string>, string>
        {
            { (NinjaRank) 999, ValidCodeName, validWeapons, validSkills, "Invalid rank" },
            { ValidRank, "", validWeapons, validSkills, "CodeName is empty" },
            { ValidRank, ValidCodeName, [(NinjaWeapon) 999], validSkills, "Invalid weapon" },
            { ValidRank, ValidCodeName, validWeapons, [""], "Empty skill" },
            { ValidRank, ValidCodeName, [NinjaWeapon.Bow, NinjaWeapon.Bow], [], "Duplicate weapon" },
            { ValidRank, ValidCodeName, validWeapons, ["skillA", "skillA"], "Duplicate skill provided" }
        };
    }
}
