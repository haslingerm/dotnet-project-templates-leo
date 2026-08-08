using Grpc.Core;
using LeoGRpcApi.Api.Persistence.Model;
using LeoGRpcApi.Api.Persistence.Util;
using LeoGRpcApi.Api.Services;
using LeoGRpcApi.Api.Util;
using LeoGRpcApi.Shared.ApiContract;
using LeoGRpcApi.Shared.ApiContract.Validation;
using OneOf;
using OneOf.Types;

namespace LeoGRpcApi.Api.Endpoints;

public sealed class MissionMgmtEndpoint(
    IMissionService missionService,
    ITransactionProvider transaction)
    : MissionMgmt.MissionMgmtBase
{
    public override async Task<MissionDto> CreateMission(CreateMissionRequest request,
                                                         ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<CreateMissionRequest, CreateMissionRequestValidator>(request);

        await transaction.BeginTransactionAsync();

        string? desc = request.HasDescription ? request.Description : null;
        var mission = await missionService.CreateMissionAsync(request.Title, desc, request.Dangerousness);

        // committing is explicit in the success case, rolling back is the default
        await transaction.CommitAsync();

        return mission.ToDto();
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

        await transaction.BeginTransactionAsync();

        string? desc = request.HasDescription ? request.Description : null;
        OneOf<Success, NotFound> updateResult
            = await missionService.UpdateMissionAsync(request.Id, desc, request.Dangerousness);

        return await updateResult.Match(async success =>
                                        {
                                            await transaction.CommitAsync();

                                            return new UpdateMissionResponse();
                                        },
                                        // left uncommitted on purpose
                                        notFound => throw RpcHelper.NotFound());
    }

    public override async Task<AssignMissionResponse> AssignMission(AssignMissionRequest request,
                                                                    ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<AssignMissionRequest, AssignMissionRequestValidator>(request);

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

    public override async Task<DeleteMissionResponse> DeleteMission(DeleteMissionRequest request,
                                                                    ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<DeleteMissionRequest, DeleteMissionRequestValidator>(request);

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
}
