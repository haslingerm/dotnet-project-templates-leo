using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LeoGRpcApi.Client.Model;
using LeoGRpcApi.Client.Util;

namespace LeoGRpcApi.Client.ViewModels;

public partial class MainWindowViewModel(IMessenger messenger) : ViewModelBase,
                                                                 IRecipient<AssignNinjaRequestMessage>,
                                                                 IRecipient<AssignNinjaCompletedMessage>
{
    [ObservableProperty]
    public partial bool Loading { get; private set; }

    [ObservableProperty]
    public partial NinjaListViewModel? NinjaListViewModel { get; private set; }

    [ObservableProperty]
    public partial MissionListViewModel? MissionListViewModel { get; private set; }

    [ObservableProperty]
    public partial string InfoText { get; set; } = string.Empty;

    public void Receive(AssignNinjaCompletedMessage message)
    {
        InfoText = string.Empty;
    }

    public void Receive(AssignNinjaRequestMessage message)
    {
        InfoText = $"Please select a currently not deployed ninja, to assign them to mission #{message.MissionId}";
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        messenger.Register<AssignNinjaRequestMessage>(this);
        messenger.Register<AssignNinjaCompletedMessage>(this);

        Loading = true;
        try
        {
            await Task.Delay(800);
            NinjaListViewModel = App.CreateViewModel<NinjaListViewModel>();
            MissionListViewModel = App.CreateViewModel<MissionListViewModel>();
            await Task.WhenAll(NinjaListViewModel.InitializeAsync(),
                               MissionListViewModel.InitializeAsync());
        }
        finally
        {
            Loading = false;
        }
    }
}

public sealed class DesignMainWindowViewModel() : MainWindowViewModel(new DesignFacades.DummyMessenger());
