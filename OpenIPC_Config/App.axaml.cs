using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
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
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
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
    
    public static string OSType { get; private set; }

    private void DetectOsType()
    {
        // Detect OS Type
        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
            OSType = "Mobile";
        }
        else if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            OSType = "Desktop";
        }
        else
        {
            OSType = "Unknown";
        }
    }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        DetectOsType();

    }

    private IConfigurationRoot LoadConfiguration()
    {
        var configPath = GetConfigPath();

        // Create default settings if not present
        if (!File.Exists(configPath))
        {
            var defaultSettings = createDefaultAppSettings();
            File.WriteAllText(configPath, defaultSettings.ToString());
            Log.Information($"Default appsettings.json created at {configPath}");
        }

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        return configuration;
    }
    
    private void InitializeBasicLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Log.Information("Basic logger initialized for early startup.");
    }
    
    private void ReconfigureLogger(IConfiguration configuration)
    {
        var eventAggregator = ServiceProvider.GetRequiredService<IEventAggregator>();

        //Log.Logger = null;
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            //.WriteTo.Console() // Keep console logging
            .WriteTo.Sink(new EventAggregatorSink(eventAggregator)) // Add EventAggregatorSink
            .CreateLogger();

        Log.Information(
            "**********************************************************************************************");
        Log.Information($"Starting up log for OpenIPC Configurator v{VersionHelper.GetAppVersion()}");
        Log.Information("Logger initialized successfully.");
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        // Step 1: Initialize basic logger 
        // not sure if this is needed
        //InitializeBasicLogger();

        // Step 2: Load configuration
        var configuration = LoadConfiguration();
        
        // Configure DI container
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection, configuration);
        ServiceProvider = serviceCollection.BuildServiceProvider();
        
        // Step 4: Reconfigure logger with DI services
        ReconfigureLogger(configuration);
        
        // check for updates
        CheckForUpdatesAsync();

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
    
    private void CreateAppSettings()
    {
        var configPath = GetConfigPath();

        // Create default settings if not present
        if (!File.Exists(configPath))
        {
            var defaultSettings = createDefaultAppSettings();
            File.WriteAllText(configPath, defaultSettings.ToString());
            Log.Information($"Default appsettings.json created at {configPath}");
        }
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, false, true)
            .AddJsonFile("appsettings.json", true, true)
            // .AddJsonFile("appsettings.Development.json", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            // .AddJsonFile(
            //     $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
            //     true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Sink(new EventAggregatorSink(ServiceProvider.GetRequiredService<IEventAggregator>()))
            .CreateLogger();

        Log.Information(
            "**********************************************************************************************");
        Log.Information($"Starting up log for OpenIPC Configurator v{VersionHelper.GetAppVersion()}");
        Log.Information($"Using appsettings.json from {configPath}");
    }
    
    public virtual async Task ShowUpdateDialogAsync(string releaseNotes, string downloadUrl, string newVersion)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard("Update Available",
            $"New version available: {newVersion}\n\n{releaseNotes}\n\nDo you want to download the update?", ButtonEnum.YesNo);

        var result = await msgBox.ShowAsync();

        if (result == ButtonResult.Yes)
        {
            OpenBrowser(downloadUrl);
        }
    }

    private void OpenBrowser(string url)
    {
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private async Task CheckForUpdatesAsync()
    {
        // Set up the necessary dependencies
        var httpClient = new HttpClient();

        var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Assembly.GetExecutingAssembly().GetName().Name, "appsettings.json");

        // Create an IConfiguration instance
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: false, reloadOnChange: true)
            .Build();

        // Pass the dependencies to the constructor
        var updateChecker = new UpdateChecker(httpClient, configuration);

        try
        {
            string currentVersion;
#if DEBUG
            // In debug mode, read the version from VERSION.txt
            string versionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VERSION");
            if (File.Exists(versionFilePath))
            {
                currentVersion = File.ReadAllText(versionFilePath).Trim();
            }
            else
            {
                currentVersion = "0.0.0.0"; // Default version for debugging
            }
#else
            currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
#endif

            var result = await updateChecker.CheckForUpdateAsync(currentVersion);

            if (result.HasUpdate)
            {
                await ShowUpdateDialogAsync(result.ReleaseNotes, result.DownloadUrl, result.NewVersion);
                Log.Information($"Update Available! Version: {result.NewVersion}, {result.ReleaseNotes}");
            }
            else
            {
                Log.Information("No updates found.");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while checking for updates: {ex.Message}");
        }
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register IEventAggregator as a singleton
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IEventSubscriptionService, EventSubscriptionService>();
        services.AddSingleton<ISshClientService, SshClientService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();
        
        services.AddSingleton<IYamlConfigService, YamlConfigService>();
        services.AddSingleton<ILogger>(sp => Log.Logger);

        // Register configuration
        services.AddSingleton<IConfiguration>(configuration);

        // Register IConfiguration
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient<DeviceConfigValidator>();
        
        // Register IConfiguration
        services.AddTransient<DeviceConfigValidator>();
        
        // Register ViewModels
        RegisterViewModels(services);
        
        // Register Views
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
