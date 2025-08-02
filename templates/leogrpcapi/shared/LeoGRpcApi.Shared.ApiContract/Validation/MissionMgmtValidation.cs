using FluentValidation;

namespace LeoGRpcApi.Shared.ApiContract.Validation;

public sealed class CreateMissionRequestValidator : AbstractValidator<CreateMissionRequest>
{
    public const double MinDangerousness = 0D;
    public const double MaxDangerousness = 1D;

    public CreateMissionRequestValidator()
    {
        RuleFor(r => r.Title).NotEmpty();
        RuleFor(r => r.Dangerousness).InclusiveBetween(MinDangerousness, MaxDangerousness);
    }
}

public sealed class UpdateMissionRequestValidator : AbstractValidator<UpdateMissionRequest>
{
    public UpdateMissionRequestValidator()
    {
        RuleFor(r => r.Id).GreaterThan(0L);
        RuleFor(r => r.Dangerousness).InclusiveBetween(CreateMissionRequestValidator.MinDangerousness,
                                                       CreateMissionRequestValidator.MaxDangerousness);
    }
}

public sealed class AssignMissionRequestValidator : AbstractValidator<AssignMissionRequest>
{
    public AssignMissionRequestValidator()
    {
        RuleFor(r => r.MissionId).GreaterThan(0L);
        RuleFor(r => r.NinjaId).GreaterThan(0);
    }
}

public sealed class DeleteMissionRequestValidator : AbstractValidator<DeleteMissionRequest>
{
    public DeleteMissionRequestValidator()
    {
        RuleFor(r => r.Id).GreaterThan(0L);
    }
}
