using System.Linq.Expressions;
using FluentValidation;
using Google.Protobuf.Collections;

namespace LeoGRpcApi.Shared.ApiContract.Validation;

// many validators can be shared between client and server
public sealed class CreateNinjaRequestValidator : AbstractValidator<CreateNinjaRequest>
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

public sealed class GetNinjaByIdRequestValidator : AbstractValidator<GetNinjaByIdRequest>
{
    public GetNinjaByIdRequestValidator()
    {
        RuleFor(r => r.Id).GreaterThan(0);
    }
}
