using System.Linq.Expressions;
using FluentValidation;
using Google.Protobuf.Collections;
using Grpc.Core;
using LeoGRpcApi.Api.Core.Services;
using LeoGRpcApi.Api.Persistence.Model;
using LeoGRpcApi.Api.Persistence.Util;
using LeoGRpcApi.Api.Util;
using LeoGRpcApi.Shared.ApiContract;
using OneOf;
using OneOf.Types;

namespace LeoGRpcApi.Api.Endpoints;

// Code generation requires a reference to the .proto file in the .csproj of the project as well as the Grpc.Tools package.
// Here this is done in the Ninjas.Shared.ApiContract project and referenced by both backend and frontend.
// Mind, that some types (e.g. enums) are needed not only in the API layer but also in persistence if you do not want to add a mapping step.
public sealed class NinjaMgmtEndpoint(
    INinjaService ninjaService,
    ITransactionProvider transaction,
    ILogger<NinjaMgmtEndpoint> logger)
    : NinjaMgmt.NinjaMgmtBase
{
    public override async Task<CreateNinjaResponse>
        CreateNinja(CreateNinjaRequest request, ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<CreateNinjaRequest, CreateNinjaRequestValidator>(request);
        try
        {
            await transaction.BeginTransactionAsync();
            var ninja = await ninjaService.CreateNinjaAsync(request.Rank,
                                                            request.CodeName,
                                                            request.WeaponProficiencies.ToList(),
                                                            request.SpecialSkills.ToList());

            await transaction.CommitAsync();

            return new CreateNinjaResponse
            {
                Id = ninja.Id
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create ninja");

            throw RpcHelper.InternalError();
        }
    }

    public override async Task<GetAllNinjaIdsResponse> GetAllNinjaIds(GetAllRequest request, ServerCallContext context)
    {
        IReadOnlyCollection<int> ninjaIds = await ninjaService.GetAllNinjaIdsAsync();

        var response = new GetAllNinjaIdsResponse
        {
            NinjaIds = { ninjaIds }
        };

        return response;
    }

    public override async Task<NinjaDto> GetNinjaById(GetNinjaByIdRequest request, ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<GetNinjaByIdRequest, GetNinjaByIdRequestValidator>(request);

        OneOf<Ninja, NotFound> ninjaResult = await ninjaService.GetNinjaByIdAsync(request.Id);

        return ninjaResult.Match(ninja => ninja.ToDto(),
                                 notFound => throw RpcHelper.NotFound());
    }

    private sealed class CreateNinjaRequestValidator : AbstractValidator<CreateNinjaRequest>
    {
        public CreateNinjaRequestValidator()
        {
            RuleFor(r => r.Rank).IsInEnum();
            RuleFor(r => r.CodeName).NotEmpty();

            NoDuplicatesRuleFor<RepeatedField<NinjaWeapon>, NinjaWeapon>(r => r.WeaponProficiencies);
            NoDuplicatesRuleFor<RepeatedField<string>, string>(r => r.SpecialSkills);

            RuleForEach(r => r.WeaponProficiencies)
                .IsInEnum()
                .When(r => r.WeaponProficiencies.Count > 0);
            RuleForEach(r => r.SpecialSkills)
                .NotEmpty()
                .When(r => r.SpecialSkills.Count > 0);
        }

        private void NoDuplicatesRuleFor<TProperty, TElement>(
            Expression<Func<CreateNinjaRequest, TProperty>> expression)
            where TProperty : IEnumerable<TElement>
        {
            RuleFor(expression).Must(collection => collection.Count() == collection.Distinct().Count())
                               .WithMessage("Cannot contain duplicate values");
        }
    }

    private sealed class GetNinjaByIdRequestValidator : AbstractValidator<GetNinjaByIdRequest>
    {
        public GetNinjaByIdRequestValidator()
        {
            RuleFor(r => r.Id).GreaterThan(0);
        }
    }
}
