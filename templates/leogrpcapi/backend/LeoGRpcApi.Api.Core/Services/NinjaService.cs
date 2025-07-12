using LeoGRpcApi.Api.Persistence.Model;
using LeoGRpcApi.Api.Persistence.Util;
using LeoGRpcApi.Shared.ApiContract;
using OneOf;
using OneOf.Types;

namespace LeoGRpcApi.Api.Core.Services;

/// <summary>
///     Provides methods to manage ninjas
/// </summary>
public interface INinjaService
{
    /// <summary>
    ///     Creates a new ninja with the given rank, code name, weapon proficiencies, and special skills
    /// </summary>
    /// <param name="rank">The rank of the ninja</param>
    /// <param name="codeName">The code name of the ninja</param>
    /// <param name="weaponProficiencies">List of weapons the ninja is proficient with</param>
    /// <param name="specialSkills">List of special skills the ninja has</param>
    /// <returns>The newly created ninja</returns>
    public ValueTask<Ninja> CreateNinjaAsync(NinjaRank rank, string codeName,
                                             List<NinjaWeapon> weaponProficiencies,
                                             List<string> specialSkills);

    /// <summary>
    ///     Retrieves all ninja IDs from the database
    /// </summary>
    /// <returns>A collection of all unique ninja IDs</returns>
    public ValueTask<IReadOnlyCollection<int>> GetAllNinjaIdsAsync();

    /// <summary>
    ///     Attempts to retrieve a ninja by its ID
    /// </summary>
    /// <param name="ninjaId">The unique ID of the ninja</param>
    /// <returns>The ninja, if they have been found</returns>
    public ValueTask<OneOf<Ninja, NotFound>> GetNinjaByIdAsync(int ninjaId);
}

internal sealed class NinjaService(IUnitOfWork uow, ILogger<NinjaService> logger) : INinjaService
{
    public async ValueTask<Ninja> CreateNinjaAsync(NinjaRank rank, string codeName,
                                                   List<NinjaWeapon> weaponProficiencies, List<string> specialSkills)
    {
        var ninja = new Ninja
        {
            Rank = rank,
            CodeName = codeName,
            WeaponProficiencies = weaponProficiencies,
            SpecialSkills = specialSkills
        };

        uow.NinjaRepository.AddNinja(ninja);
        await uow.SaveChangesAsync();

        logger.LogInformation("Created new ninja with id {NinjaId}", ninja.Id);

        return ninja;
    }

    public async ValueTask<IReadOnlyCollection<int>> GetAllNinjaIdsAsync()
    {
        IReadOnlyCollection<int> ninjaIds = await uow.NinjaRepository.GetAllNinjaIdsAsync();

        return ninjaIds;
    }

    public async ValueTask<OneOf<Ninja, NotFound>> GetNinjaByIdAsync(int ninjaId)
    {
        var ninja = await uow.NinjaRepository.GetNinjaByIdAsync(ninjaId);
        if (ninja is null)
        {
            logger.LogWarning("Ninja with id {NinjaId} not found", ninjaId);

            return new NotFound();
        }

        return ninja;
    }
}
