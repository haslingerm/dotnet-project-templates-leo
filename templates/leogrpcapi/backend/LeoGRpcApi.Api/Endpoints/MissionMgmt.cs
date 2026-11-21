using Grpc.Core;
using LeoGRpcApi.Api.Persistence.Model;
using LeoGRpcApi.Api.Persistence.Util;
using LeoGRpcApi.Api.Services;
using LeoGRpcApi.Api.Util;
using LeoGRpcApi.Shared.ApiContract;
using LeoGRpcApi.Shared.ApiContract.Validation;
using LeoUnion;
using LeoUnion.Markers;

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
        Union<Success, NotFound> updateResult
            = await missionService.UpdateMissionAsync(request.Id, desc, request.Dangerousness);

        return updateResult switch
        {
            Success => await transaction.CommitAndReturnAsync(new UpdateMissionResponse()),
            NotFound => throw RpcHelper.NotFound()
        };
    }

    public override async Task<AssignMissionResponse> AssignMission(AssignMissionRequest request,
                                                                    ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<AssignMissionRequest, AssignMissionRequestValidator>(request);

        await transaction.BeginTransactionAsync();

        Union<Success, IMissionService.MissionNotFound, IMissionService.NinjaNotFound, IMissionService.NinjaBusy>
            assignMissionResult = await missionService.AssignMissionAsync(request.MissionId, request.NinjaId);

        return assignMissionResult switch
        {
            Success => await transaction.CommitAndReturnAsync(new AssignMissionResponse
            {
                Result = MissionAssignmentResult.Success
            }),
            IMissionService.MissionNotFound => new AssignMissionResponse
            {
                Result = MissionAssignmentResult.MissionNotFound
            },
            IMissionService.NinjaNotFound => new AssignMissionResponse
            {
                Result = MissionAssignmentResult.NinjaNotFound
            },
            IMissionService.NinjaBusy => new AssignMissionResponse
            {
                Result = MissionAssignmentResult.NinjaAlreadyOnMission
            }
        };
    }

    public override async Task<DeleteMissionResponse> DeleteMission(DeleteMissionRequest request,
                                                                    ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<DeleteMissionRequest, DeleteMissionRequestValidator>(request);

        await transaction.BeginTransactionAsync();

        Union<Success, NotFound> deleteResult = await missionService.DeleteMissionAsync(request.Id);

        return deleteResult switch
        {
            Success => await transaction.CommitAndReturnAsync(new DeleteMissionResponse { Success = true }),
            NotFound => new DeleteMissionResponse { Success = false }
        };
    }
}
