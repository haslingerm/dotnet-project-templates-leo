using Avalonia.Controls;
using LeoGRpcApi.Client.ViewModels;

namespace LeoGRpcApi.Client.Views;

public partial class NinjaList : UserControl
{
    public NinjaList()
    {
        InitializeComponent();
    }

    private async void HandleSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (DataContext is not NinjaListViewModel viewModel)
            {
                return;
            }

            await viewModel.HandleNinjaSelectedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled exception in HandleSelectionChanged: {ex.Message}");
        }
    }
}
