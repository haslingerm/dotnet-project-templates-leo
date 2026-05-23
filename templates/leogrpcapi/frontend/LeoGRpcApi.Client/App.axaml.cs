using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LeoGRpcApi.Client.Util;
using LeoGRpcApi.Client.ViewModels;
using LeoGRpcApi.Client.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LeoGRpcApi.Client;

public partial class App : Application
{
    private static IServiceProvider? _serviceProvider;

    public App(IServiceProvider services)
    {
        _serviceProvider = services;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = CreateViewModel<MainWindowViewModel>()
            };
            
            _serviceProvider?.GetRequiredService<IToastService>().SetWindow(desktop.MainWindow);
            _serviceProvider?.GetRequiredService<IDialogService>().SetOwner(desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    internal static T CreateViewModel<T>() where T : ViewModelBase
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("Service provider is not initialized");
        }
        
        var viewModel = ActivatorUtilities.CreateInstance<T>(_serviceProvider);

        return viewModel;
    }
}