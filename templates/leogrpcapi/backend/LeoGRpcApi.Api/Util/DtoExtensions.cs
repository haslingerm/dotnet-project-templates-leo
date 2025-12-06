using LeoGRpcApi.Api.Persistence.Model;
using LeoGRpcApi.Shared.ApiContract;

namespace LeoGRpcApi.Api.Util;

internal static class DtoExtensions
{
    extension(Ninja ninja)
    {
        /// <summary>
        ///     Converts a <see cref="Ninja" /> to a <see cref="NinjaDto" />
        /// </summary>
        /// <returns>The DTO representation of the ninja</returns>
        public NinjaDto ToDto()
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
    }

    extension(Mission mission)
    {
        /// <summary>
        ///     Converts a <see cref="Mission" /> to a <see cref="MissionDto" />
        /// </summary>
        /// <returns>The DTO representation of the mission</returns>
        public MissionDto ToDto()
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
}
