using FluentValidation;
using Grpc.Core;
using LeoGRpcApi.Api.Core.Services;
using LeoGRpcApi.Api.Persistence.Model;
using LeoGRpcApi.Api.Persistence.Util;
using LeoGRpcApi.Api.Util;
using LeoGRpcApi.Shared.ApiContract;
using OneOf;
using OneOf.Types;

namespace LeoGRpcApi.Api.Endpoints;

public sealed class MissionMgmtEndpoint(
    IMissionService missionService,
    ITransactionProvider transaction,
    ILogger<MissionMgmtEndpoint> logger)
    : MissionMgmt.MissionMgmtBase
{
    public override async Task<MissionDto> CreateMission(CreateMissionRequest request,
                                                                    ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<CreateMissionRequest, CreateMissionRequestValidator>(request);

        try
        {
            await transaction.BeginTransactionAsync();
            string? desc = request.HasDescription ? request.Description : null;
            var mission = await missionService.CreateMissionAsync(request.Title, desc, request.Dangerousness);

            await transaction.CommitAsync();

            return mission.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create mission");

            throw RpcHelper.InternalError();
        }
    }

    public override async Task<GetAllMissionsResponse> GetAllMissions(GetAllRequest request, ServerCallContext context)
    {
        IReadOnlyCollection<Mission> missions = await missionService.GetAllMissionsAsync();

        return new GetAllMissionsResponse
        {
            Missions = { missions.Select(m => m.ToDto()) }
        };
    }

    public override async Task<UpdateMissionResponse> UpdateMission(UpdateMissionRequest request,
                                                                      ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<UpdateMissionRequest, UpdateMissionRequestValidator>(request);

        try
        {
            await transaction.BeginTransactionAsync();
            string? desc = request.HasDescription ? request.Description : null;
            OneOf<Success, NotFound> updateResult
                = await missionService.UpdateMissionAsync(request.Id, desc, request.Dangerousness);

            return await updateResult.Match(async success =>
                                            {
                                                await transaction.CommitAsync();

                                                return new UpdateMissionResponse();
                                            },
                                            notFound => throw RpcHelper.NotFound());
        }
        // We might throw a NotFound RpcException here as a normal code path, so catching that right away again does not make sense
        catch (Exception ex) when (ex is not RpcException)
        {
            logger.LogError(ex, "Failed to update mission");

            throw RpcHelper.InternalError();
        }
    }

    public override async Task<AssignMissionResponse> AssignMission(AssignMissionRequest request,
                                                                    ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<AssignMissionRequest, AssignMissionRequestValidator>(request);

        try
        {
            await transaction.BeginTransactionAsync();

            OneOf<Success, IMissionService.MissionNotFound, IMissionService.NinjaNotFound, IMissionService.NinjaBusy>
                assignMissionResult = await missionService.AssignMissionAsync(request.MissionId, request.NinjaId);
            var result = await assignMissionResult.Match<ValueTask<MissionAssignmentResult>>(async success =>
             {
                 await transaction.CommitAsync();

                 return MissionAssignmentResult.Success;
             },
             missionNotFound => ValueTask.FromResult(MissionAssignmentResult.MissionNotFound),
             ninjaNotFound => ValueTask.FromResult(MissionAssignmentResult.NinjaNotFound),
             ninjaBusy => ValueTask.FromResult(MissionAssignmentResult.NinjaAlreadyOnMission));

            return new AssignMissionResponse
            {
                Result = result
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign mission");

            throw RpcHelper.InternalError();
        }
    }

    public override async Task<DeleteMissionResponse> DeleteMission(DeleteMissionRequest request,
                                                                      ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<DeleteMissionRequest, DeleteMissionRequestValidator>(request);

        try
        {
            await transaction.BeginTransactionAsync();
            OneOf<Success, NotFound> deleteResult = await missionService.DeleteMissionAsync(request.Id);

            bool result = await deleteResult.Match<ValueTask<bool>>(async success =>
                                                                    {
                                                                        await transaction.CommitAsync();

                                                                        return true;
                                                                    },
                                                                    notFound => ValueTask.FromResult(false));

            return new DeleteMissionResponse
            {
                Success = result
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete mission");

            throw RpcHelper.InternalError();
        }
    }

    private sealed class CreateMissionRequestValidator : AbstractValidator<CreateMissionRequest>
    {
        public const double MinDangerousness = 0D;
        public const double MaxDangerousness = 1D;

        public CreateMissionRequestValidator()
        {
            RuleFor(r => r.Title).NotEmpty();
            RuleFor(r => r.Dangerousness).InclusiveBetween(MinDangerousness, MaxDangerousness);
        }
    }

    private sealed class UpdateMissionRequestValidator : AbstractValidator<UpdateMissionRequest>
    {
        public UpdateMissionRequestValidator()
        {
            RuleFor(r => r.Id).GreaterThan(0L);
            RuleFor(r => r.Dangerousness).InclusiveBetween(CreateMissionRequestValidator.MinDangerousness,
                                                           CreateMissionRequestValidator.MaxDangerousness);
        }
    }

    private sealed class AssignMissionRequestValidator : AbstractValidator<AssignMissionRequest>
    {
        public AssignMissionRequestValidator()
        {
            RuleFor(r => r.MissionId).GreaterThan(0L);
            RuleFor(r => r.NinjaId).GreaterThan(0);
        }
    }

    private sealed class DeleteMissionRequestValidator : AbstractValidator<DeleteMissionRequest>
    {
        public DeleteMissionRequestValidator()
        {
            RuleFor(r => r.Id).GreaterThan(0L);
        }
    }
}
