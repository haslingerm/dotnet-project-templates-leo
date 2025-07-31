using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Grpc.Core;
using LeoGRpcApi.Client.Core.Services;
using LeoGRpcApi.Client.Model;
using LeoGRpcApi.Client.Util;
using LeoGRpcApi.Shared.ApiContract;

namespace LeoGRpcApi.Client.ViewModels;

public partial class NinjaListViewModel(
    INinjaService ninjaService,
    IMissionService missionService,
    ILogger<NinjaListViewModel> logger,
    IToastService toast,
    IMessenger messenger) : ViewModelBase, IRecipient<AssignNinjaRequestMessage>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PickMode))]
    private partial long? PickForMissionId { get; set; }

    [ObservableProperty]
    public partial bool Loading { get; set; }

    [MemberNotNullWhen(true, nameof(PickForMissionId))]
    public bool PickMode => PickForMissionId is not null;

    [ObservableProperty]
    public partial NinjaDisplay? SelectedNinja { get; set; }

    public ObservableCollection<NinjaDisplay> Ninjas { get; } = [];

    public void Receive(AssignNinjaRequestMessage message)
    {
        SelectedNinja = null;
        PickForMissionId = message.MissionId;
    }

    public async Task InitializeAsync()
    {
        messenger.Register(this);
        PickForMissionId = null;
        Loading = true;
        try
        {
            IReadOnlyCollection<int> allNinjas = await ninjaService.GetAllNinjaIdsAsync();
            foreach (int ninjaId in allNinjas)
            {
                await LoadNinjaAsync(ninjaId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load ninjas");
            toast.ShowError("Failed to load ninjas");
        }
        finally
        {
            Loading = false;
        }

        return;

        async ValueTask LoadNinjaAsync(int id)
        {
            try
            {
                var ninja = await ninjaService.GetNinjaByIdAsync(id);
                Ninjas.Add(NinjaDisplay.FromDto(ninja));
            }
            // reacting only to an expected NotFound, other RPC exception will bubble up
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.NotFound)
            {
                toast.ShowWarning($"Ninja #{id} not found");
            }
        }
    }

    public async ValueTask HandleNinjaSelectedAsync()
    {
        if (!PickMode)
        {
            return;
        }

        var ninja = SelectedNinja;
        var succeeded = false;
        long missionId = PickForMissionId.Value;

        // Cancel if clicked outside a valid row
        if (ninja is null)
        {
            EndSelectionMode();

            return;
        }

        if (ninja.IsDeployed)
        {
            toast.ShowWarning("Selected ninja is already deployed on a mission");

            return;
        }

        try
        {
            var result = await missionService.AssignNinjaToMissionAsync(missionId, ninja.Id);

            switch (result)
            {
                case MissionAssignmentResult.Success:
                {
                    succeeded = true;

                    // set null here to avoid cyclic event handling
                    PickForMissionId = null;

                    UpdateDisplay();
                    toast.ShowSuccess("Ninja assigned to mission");

                    break;
                }
                case MissionAssignmentResult.NinjaNotFound:
                {
                    toast.ShowWarning("Selected ninja was not found at the backend");

                    break;
                }
                case MissionAssignmentResult.MissionNotFound:
                {
                    toast.ShowWarning("Selected mission was not found at the backend");

                    break;
                }
                case MissionAssignmentResult.NinjaAlreadyOnMission:
                {
                    toast.ShowWarning("Selected ninja is already on a mission");

                    break;
                }
                case MissionAssignmentResult.UnknownAssignmentResult:
                default:
                {
                    toast.ShowError("Failed to assign ninja to mission");

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign ninja to mission");
            toast.ShowError("Failed to assign ninja to mission");
        }
        finally
        {
            EndSelectionMode();
        }

        return;

        void EndSelectionMode()
        {
            messenger.Send(new AssignNinjaCompletedMessage(succeeded, succeeded && ninja is not null
                                                               ? ninja.Id
                                                               : null,
                                                           missionId));
            SelectedNinja = null;
            PickForMissionId = null;
        }

        void UpdateDisplay()
        {
            Ninjas.Remove(ninja);
            var updatedNinja = ninja with { IsDeployed = true };
            Ninjas.Add(updatedNinja);
        }
    }

    public sealed record NinjaDisplay(
        int Id,
        string CodeName,
        string Rank,
        string WeaponProficiencies,
        string SpecialSkills,
        bool IsDeployed)
    {
        public static NinjaDisplay FromDto(NinjaDto dto)
        {
            const string Separator = ", ";
            bool isDeployed = dto is { HasCurrentMission: true, CurrentMission: > 0L };

            return new NinjaDisplay(dto.Id, dto.CodeName, dto.Rank.ToString(),
                                    string.Join(Separator, dto.WeaponProficiencies),
                                    string.Join(Separator, dto.SpecialSkills), isDeployed);
        }
    }
}

public sealed class DesignNinjaListViewModel() : NinjaListViewModel(new DesignFacades.DummyNinjaService(),
                                                                    new DesignFacades.DummyMissionService(),
                                                                    new DesignFacades.DummyLogger<NinjaListViewModel>(),
                                                                    new DesignFacades.DummyToastService(),
                                                                    new DesignFacades.DummyMessenger());
