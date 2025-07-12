using LeoGRpcApi.Api.Persistence.Model;
using LeoGRpcApi.Shared.ApiContract;

namespace LeoGRpcApi.Api.Util;

internal static class DtoExtensions
{
    /// <summary>
    ///     Converts a <see cref="Ninja" /> to a <see cref="NinjaDto" />
    /// </summary>
    /// <param name="ninja">The ninja to convert</param>
    /// <returns>The DTO representation of the ninja</returns>
    public static NinjaDto ToDto(this Ninja ninja)
    {
        var dto = new NinjaDto
        {
            Id = ninja.Id,
            CodeName = ninja.CodeName,
            Rank = ninja.Rank,
            WeaponProficiencies = { ninja.WeaponProficiencies },
            SpecialSkills = { ninja.SpecialSkills }
        };

        // Because the DTO property is not nullable, yet optional, we have to jump through an extra hoop
        if (ninja.CurrentMissionId.HasValue)
        {
            dto.CurrentMission = ninja.CurrentMissionId.Value;
        }

        return dto;
    }

    /// <summary>
    ///     Converts a <see cref="Mission" /> to a <see cref="MissionDto" />
    /// </summary>
    /// <param name="mission">The mission to convert</param>
    /// <returns>The DTO representation of the mission</returns>
    public static MissionDto ToDto(this Mission mission)
    {
        var dto = new MissionDto
        {
            Id = mission.Id,
            Title = mission.Title,
            Dangerousness = mission.Dangerousness
        };
        if (mission.Description is not null)
        {
            dto.Description = mission.Description;
        }

        return dto;
    }
}
