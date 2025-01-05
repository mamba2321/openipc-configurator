using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OpenIPC_Config.Logging;
using OpenIPC_Config.Services;
using OpenIPC_Config.ViewModels;
using OpenIPC_Config.Views;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config;

public class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }
    
    public static bool IsMobile { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeLogger(string configPath)
    {
        
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: false, reloadOnChange: true)
            .Build();

        // Initialize logger with EventAggregatorSink
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Sink(new EventAggregatorSink(ServiceProvider.GetRequiredService<IEventAggregator>()))
            .CreateLogger();

        Log.Information("**********************************************************************************************");
        Log.Information($"Starting up log for OpenIPC Configurator v{VersionHelper.GetAppVersion()}");
        Log.Information($"Using appsettings.json from {configPath}");
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        // Determine if the app is running on a mobile platform 
        IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();
        
        var appSetting = CreateAppSettings();
        
        // Configure and build the DI container
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();
        
        // Initialize logger after ServiceProvider is available
        InitializeLogger(appSetting);
        
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Remove Avalonia's default data validation plugin to avoid conflicts
            BindingPlugins.DataValidators.RemoveAt(0);

            // Resolve MainWindow and its DataContext from DI container
            desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // Resolve MainView and its DataContext from DI container
            singleViewPlatform.MainView = ServiceProvider.GetRequiredService<MainView>();
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private string GetConfigPath()
    {
        var appName = Assembly.GetExecutingAssembly().GetName().Name;
        string configPath;

        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() || OperatingSystem.IsMacOS())
        {
            var configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            if (!Directory.Exists(configDirectory))
                Directory.CreateDirectory(configDirectory);

            configPath = Path.Combine(configDirectory, "appsettings.json");
        }
        else if (OperatingSystem.IsWindows())
        {
            var configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            if (!Directory.Exists(configDirectory))
                Directory.CreateDirectory(configDirectory);

            configPath = Path.Combine(configDirectory, "appsettings.json");
        }
        else // Assume Linux
        {
            var configDirectory = Path.Combine($"./config/{appName}");
            if (!Directory.Exists(configDirectory))
                Directory.CreateDirectory(configDirectory);

            configPath = Path.Combine(configDirectory, "appsettings.json");
        }

        return configPath;
    }

    private string CreateAppSettings()
    {
        var configPath = GetConfigPath();

        // Create default settings if not present
        if (!File.Exists(configPath))
        {
            var defaultSettings = createDefaultAppSettings();
            File.WriteAllText(configPath, defaultSettings.ToString());
            Log.Information($"Default appsettings.json created at {configPath}");
        }

        return configPath;

    }


    private void ConfigureServices(IServiceCollection services)
    {
        // Register IEventAggregator as a singleton
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IEventSubscriptionService, EventSubscriptionService>();
        services.AddSingleton<ISshClientService, SshClientService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();
        services.AddSingleton<IYamlConfigService, YamlConfigService>();
        services.AddSingleton<ILogger>(sp => Log.Logger);
        
        // Load the configuration
        var configPath = GetConfigPath();
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: false, reloadOnChange: true)
            .Build();
        
        Log.Information(
            "**********************************************************************************************");
        Log.Information($"Starting up log for OpenIPC Configurator v{VersionHelper.GetAppVersion()}");
        Log.Information($"Using appsettings.json from {configPath}");

        // Register IConfiguration
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient<DeviceConfigValidator>();
        
        RegisterViewModels(services);

        RegisterViews(services);
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        // Register ViewModels
        services.AddTransient<MainViewModel>();

        services.AddTransient<CameraSettingsTabViewModel>();
        services.AddTransient<ConnectControlsViewModel>();
        services.AddTransient<LogViewerViewModel>();
        services.AddTransient<SetupTabViewModel>();
        services.AddTransient<StatusBarViewModel>();
        services.AddTransient<TelemetryTabViewModel>();
        services.AddTransient<VRXTabViewModel>();
        services.AddTransient<WfbGSTabViewModel>();
        services.AddTransient<WfbTabViewModel>();
        services.AddTransient<OsdTabViewModel>();
    }

    private static void RegisterViews(IServiceCollection services)
    {
        // Register Views
        services.AddTransient<MainWindow>();
        services.AddTransient<MainView>();
        services.AddTransient<CameraSettingsView>();
        services.AddTransient<ConnectControlsView>();
        services.AddTransient<LogViewer>();
        services.AddTransient<SetupTabView>();
        services.AddTransient<StatusBarView>();
        services.AddTransient<TelemetryTabView>();
        services.AddTransient<VRXTabView>();
        services.AddTransient<WfbGSTabView>();
        services.AddTransient<WfbTabView>();
        services.AddTransient<OsdTabView>();
    }


    private JObject createDefaultAppSettings()
    {
        // Create default settings
        var defaultSettings = new JObject(
            new JProperty("UpdateChecker",
                new JObject(
                    new JProperty("LatestJsonUrl", "https://github.com/OpenIPC/openipc-configurator/releases/latest/download/latest.json")
                )
            ),
            new JProperty("Serilog",
                new JObject(
                    new JProperty("Using", new JArray("Serilog.Sinks.Console", "Serilog.Sinks.RollingFile")),
                    new JProperty("MinimumLevel", "Verbose"),
                    new JProperty("WriteTo",
                        new JArray(
                            new JObject(
                                new JProperty("Name", "Console")
                            ),
                            new JObject(
                                new JProperty("Name", "RollingFile"),
                                new JProperty("Args",
                                    new JObject(
                                        new JProperty("pathFormat",
                                            $"{Models.OpenIPC.AppDataConfigDirectory}/Logs/configurator-{{Date}}.log")
                                    )
                                )
                            )
                        )
                    ),
                    new JProperty("Properties",
                        new JObject(
                            new JProperty("Application", "OpenIPC_Config")
                        )
                    )
                )
            ),
            new JProperty("DeviceHostnameMapping",
                new JObject(
                    new JProperty("Camera", new JArray("openipc-ssc338q","openipc-ssc330")),
                    new JProperty("Radxa", new JArray("radxa", "raspberrypi")),
                    new JProperty("NVR", new JArray("openipc-hi3536dv100"))
                )
            )
        );

        return defaultSettings;
    }

}
