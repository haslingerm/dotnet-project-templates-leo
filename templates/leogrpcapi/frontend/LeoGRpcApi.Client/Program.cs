using Avalonia;
using CommunityToolkit.Mvvm.Messaging;
using LeoGRpcApi.Client.Core.Util;
using LeoGRpcApi.Client.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LeoGRpcApi.Client;

internal sealed class Program
{
    private static IServiceProvider _serviceProvider = null!;
#if DEBUG
    private const string EnvironmentName = "Development";
#else
    private const string EnvironmentName = "Production";
#endif

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = EnvironmentName
        });

        AddConfiguration(builder);
        builder.Services.AddCoreServices()
               .AddSingleton<IToastService, ToastService>()
               .AddSingleton<IDialogService, DialogService>()
               // adding a message bus
               .AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);
        ConfigureLogging(builder);
        
        using var host = builder.Build();
        _serviceProvider = host.Services;

        await host.StartAsync();
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        
        await host.StopAsync();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure(() => new App(_serviceProvider))
                  .UsePlatformDetect()
                  .WithInterFont()
                  .LogToTrace();
    
    private static void AddConfiguration(HostApplicationBuilder builder)
    {
        var config = builder.Configuration;
        builder.Services.Configure<Settings>(config.GetSection(Settings.SectionKey));
    }

    private static void ConfigureLogging(HostApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
                     .ReadFrom.Configuration(builder.Configuration)
                     .Enrich.FromLogContext()
                     .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
                     .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(dispose: true);
    }
}
