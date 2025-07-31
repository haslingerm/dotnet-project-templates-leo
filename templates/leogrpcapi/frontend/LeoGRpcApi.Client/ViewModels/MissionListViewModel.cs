using System.Collections.ObjectModel;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LeoGRpcApi.Client.Core.Services;
using LeoGRpcApi.Client.Model;
using LeoGRpcApi.Client.Util;
using LeoGRpcApi.Shared.ApiContract;

namespace LeoGRpcApi.Client.ViewModels;

public partial class MissionListViewModel(
    IMissionService missionService,
    ILogger<MissionListViewModel> logger,
    IToastService toast,
    IMessenger messenger) : ViewModelBase, IRecipient<AssignNinjaCompletedMessage>
{
    private readonly Dictionary<long, (double Dangerousness, string? Description)> _originalValues = new();

    [ObservableProperty]
    public partial bool Loading { get; private set; }

    [ObservableProperty]
    public partial bool Processing { get; private set; }

    public ObservableCollection<MissionDisplay> Missions { get; } = [];

    public async Task InitializeAsync()
    {
        messenger.Register(this);
        Loading = true;
        try
        {
            IReadOnlyCollection<MissionDto> allMissions = await missionService.GetAllMissionsAsync();
            foreach (var mission in allMissions)
            {
                Missions.Add(MissionDisplay.FromDto(mission));
                _originalValues[mission.Id] = (mission.Dangerousness, mission.Description);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load missions");
            toast.ShowError("Failed to load missions");
        }
        finally
        {
            Loading = false;
        }
    }

    [RelayCommand]
    private void AssignNinja(MissionDisplay? mission)
    {
        if (mission is null || Processing)
        {
            return;
        }

        messenger.Send(new AssignNinjaRequestMessage(mission.Id));
        
        // enabled here and only disabled after completed message is received
        Processing = true;
    }

    [RelayCommand]
    private async Task DeleteMissionAsync(MissionDisplay mission)
    {
        const string ErrorMessage = "Failed to delete mission";
        Processing = true;
        try
        {
            await Task.Delay(500);
            bool result = await missionService.DeleteMissionAsync(mission.Id);
            if (!result)
            {
                toast.ShowWarning(ErrorMessage);

                return;
            }

            Missions.Remove(mission);
            _originalValues.Remove(mission.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessage);
            toast.ShowError(ErrorMessage);
        }
        finally
        {
            Processing = false;
        }
    }

    public async ValueTask HandleMissionEdited(MissionDisplay mission)
    {
        Processing = true;
        try
        {
            await Task.Delay(500);
            await missionService.UpdateMissionAsync(mission.Id, mission.Dangerousness, mission.Description);
            _originalValues[mission.Id] = (mission.Dangerousness, mission.Description);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to edit mission");
            toast.ShowError("Failed to edit mission");

            var original = _originalValues[mission.Id];
            mission.Dangerousness = original.Dangerousness;
            mission.Description = original.Description ?? string.Empty;
        }
        finally
        {
            Processing = false;
        }
    }
    
    public void Receive(AssignNinjaCompletedMessage message)
    {
        Processing = false;
    }

    public sealed partial class MissionDisplay : ObservableObject
    {
        public long Id { get; init; }
        public required string Title { get; init; }

        [ObservableProperty]
        public required partial string Description { get; set; }

        [ObservableProperty]
        public partial double Dangerousness { get; set; }

        public static MissionDisplay FromDto(MissionDto dto) =>
            new()
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                Dangerousness = dto.Dangerousness
            };
    }
}

public sealed class DesignMissionListViewModel() : MissionListViewModel(new DesignFacades.DummyMissionService(),
                                                                        new DesignFacades.DummyLogger<
                                                                            MissionListViewModel>(),
                                                                        new DesignFacades.DummyToastService(),
                                                                        new DesignFacades.DummyMessenger());
