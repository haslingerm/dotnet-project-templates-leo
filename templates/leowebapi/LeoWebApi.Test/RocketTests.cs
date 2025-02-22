using LeoWebApi.Core.Logic;
using LeoWebApi.Persistence.Model;

namespace LeoWebApi.Test;

public sealed class RocketTests
{
    [Fact]
    public void CalcThrustToWeightRatio_Success()
    {
        var rocket = new Rocket
        {
            Id = 1,
            ModelName = "H-IIA",
            Manufacturer = "JAXA",
            MaxThrust = 1_500_000,
            PayloadDeltaV = 36_000_000
        };

        (bool takeoffPossible, double ratio) = rocket.CalcThrustToWeightRatio(800_000);

        ratio.Should().Be(1.875D);
        takeoffPossible.Should().BeTrue();
    }
}
