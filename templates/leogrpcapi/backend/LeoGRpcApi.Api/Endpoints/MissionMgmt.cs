using System.Diagnostics;
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

        // A switch statement, because the success branch also commits. The compiler does not know that it handles
        // every case, so it needs the default guard; that guard is no case: LA0003 still checks that all are handled.
        switch (updateResult)
        {
            case Success:
                await transaction.CommitAsync();

                return new UpdateMissionResponse();
            case NotFound:
                // left uncommitted on purpose
                throw RpcHelper.NotFound();
            default:
                throw new UnreachableException();
        }
    }

    public override async Task<AssignMissionResponse> AssignMission(AssignMissionRequest request,
                                                                    ServerCallContext context)
    {
        RpcHelper.ThrowIfInvalid<AssignMissionRequest, AssignMissionRequestValidator>(request);

        await transaction.BeginTransactionAsync();

        Union<Success, IMissionService.MissionNotFound, IMissionService.NinjaNotFound, IMissionService.NinjaBusy>
            assignMissionResult = await missionService.AssignMissionAsync(request.MissionId, request.NinjaId);

        MissionAssignmentResult result;
        switch (assignMissionResult)
        {
            case Success:
                await transaction.CommitAsync();
                result = MissionAssignmentResult.Success;

                break;
            // the negative outcomes are regular results here, left uncommitted on purpose
            case IMissionService.MissionNotFound:
                result = MissionAssignmentResult.MissionNotFound;

                break;
            case IMissionService.NinjaNotFound:
                result = MissionAssignmentResult.NinjaNotFound;

                break;
            case IMissionService.NinjaBusy:
                result = MissionAssignmentResult.NinjaAlreadyOnMission;

                break;
            default:
                throw new UnreachableException();
        }

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

        Union<Success, NotFound> deleteResult = await missionService.DeleteMissionAsync(request.Id);

        switch (deleteResult)
        {
            case Success:
                await transaction.CommitAsync();

                return new DeleteMissionResponse { Success = true };
            case NotFound:
                return new DeleteMissionResponse { Success = false };
            default:
                throw new UnreachableException();
        }
    }
}
