using Avalonia.Controls;
using LeoGRpcApi.Client.ViewModels;

namespace LeoGRpcApi.Client.Views;

public partial class MissionList : UserControl
{
    public MissionList()
    {
        InitializeComponent();
    }

    private async void HandleCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        try
        {
            if (e.Row.DataContext is not MissionListViewModel.MissionDisplay mission
                || DataContext is not MissionListViewModel viewModel)
            {
                return;
            }

            await viewModel.HandleMissionEdited(mission);
        }
        catch (Exception)
        {
            // We _have_ to handle any exceptions here (somehow) due to async void, otherwise the whole process can crash
            Console.WriteLine("Unhandled exception in async void HandleCellEditEnded");
        }
    }
}
