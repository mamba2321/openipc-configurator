﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    
    // With this singleton service
    private readonly PingService _pingService;
    
    private DispatcherTimer _pingTimer;
    private readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _pingTimeout = TimeSpan.FromMilliseconds(500);

    
    [ObservableProperty] private string _svgPath;
    private bool _isTabsCollapsed;
<<<<<<< HEAD

    [ObservableProperty] private bool _isMobile;
=======
>>>>>>> parent of 06fa738 (update icons)
    private DeviceType _selectedDeviceType;

    [ObservableProperty] private string _soc;
    [ObservableProperty] private string _sensor;
    [ObservableProperty] private string _networkCardType;

    private readonly IServiceProvider _serviceProvider;
    private readonly GlobalSettingsViewModel _globalSettingsSettingsViewModel;

    
    [ObservableProperty] private bool _isWaiting;
    [ObservableProperty] private bool _isConnected;
    

    public MainViewModel(ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService,
        IServiceProvider serviceProvider,
        GlobalSettingsViewModel globalSettingsSettingsViewModel)
        : base(logger, sshClientService, eventSubscriptionService)
    {
<<<<<<< HEAD
        LoadSettings();
        
        // Initialize the ping service
        _pingService = PingService.Instance(logger);
        CanConnect = false;
        
        IsMobile = false;
=======
>>>>>>> parent of 06fa738 (update icons)
        _serviceProvider = serviceProvider;
        _appVersionText = GetFormattedAppVersion();
        _globalSettingsSettingsViewModel = globalSettingsSettingsViewModel;

        Tabs = new ObservableCollection<TabItemViewModel> { };
        // Subscribe to device type change events
        EventSubscriptionService.Subscribe<DeviceTypeChangeEvent, DeviceType>(
            OnDeviceTypeChangeEvent);


        ToggleTabsCommand = new RelayCommand(() => IsTabsCollapsed = !IsTabsCollapsed);

        EntryBoxBgColor = new SolidColorBrush(Colors.White);

        ConnectCommand = new RelayCommand(() => Connect());

        DeviceTypes = new ObservableCollection<DeviceType>(Enum.GetValues(typeof(DeviceType)).Cast<DeviceType>());

        // Initialize the path
        UpdateSvgPath();

        Soc = "";
        Sensor = "";
        Soc = string.Empty;
        Sensor = string.Empty;
        NetworkCardType = string.Empty;
        IsVRXEnabled = false;
        
        // initialize tabs with Camera
        InitializeTabs(DeviceType.Camera);
    }

    private void InitializeTabs(DeviceType deviceType)
    {
        Tabs.Clear();

        // if Mobile apps default to tabs collapsed
        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
            IsTabsCollapsed = true;
        }

        if (deviceType == DeviceType.Camera)
        {
            Tabs.Add(new TabItemViewModel("Firmware", "avares://OpenIPC_Config/Assets/Icons/iconoir_cube.svg",
                _serviceProvider.GetRequiredService<FirmwareTabViewModel>(), IsTabsCollapsed));
            Tabs.Add(new TabItemViewModel("WFB", "avares://OpenIPC_Config/Assets/Icons/iconoir_wifi.svg",
                _serviceProvider.GetRequiredService<WfbTabViewModel>(), IsTabsCollapsed));
            Tabs.Add(new TabItemViewModel("Camera", "avares://OpenIPC_Config/Assets/Icons/iconoir_camera.svg",
                _serviceProvider.GetRequiredService<CameraSettingsTabViewModel>(), IsTabsCollapsed));
            Tabs.Add(new TabItemViewModel("Telemetry", "avares://OpenIPC_Config/Assets/Icons/iconoir_drag.svg",
                _serviceProvider.GetRequiredService<TelemetryTabViewModel>(), IsTabsCollapsed));
            // Tabs.Add(new TabItemViewModel("Presets", "avares://OpenIPC_Config/Assets/Icons/iconoir_presets_dark.svg",
            //     _serviceProvider.GetRequiredService<PresetsTabViewModel>(), IsTabsCollapsed));
            Tabs.Add(new TabItemViewModel("Setup", "avares://OpenIPC_Config/Assets/Icons/iconoir_settings.svg",
                _serviceProvider.GetRequiredService<SetupTabViewModel>(), IsTabsCollapsed));
        }
        else if (deviceType == DeviceType.Radxa)
        {
            // Need these spaces for some reason
            Tabs.Add(new TabItemViewModel("WFB         ", "avares://OpenIPC_Config/Assets/Icons/iconoir_drag.svg",
                _serviceProvider.GetRequiredService<WfbGSTabViewModel>(), IsTabsCollapsed));
            Tabs.Add(new TabItemViewModel("Setup", "avares://OpenIPC_Config/Assets/Icons/iconoir_settings.svg",
                _serviceProvider.GetRequiredService<SetupTabViewModel>(), IsTabsCollapsed));
        }
    }

    public bool IsTabsCollapsed
    {
        get => _isTabsCollapsed;
        set
        {
            if (SetProperty(ref _isTabsCollapsed, value))
            {
                UpdateSvgPath();
            }
        }
    }

    public ObservableCollection<TabItemViewModel> Tabs { get; set; }
    public ObservableCollection<DeviceType> DeviceTypes { get; set; }

    [ObservableProperty] private SolidColorBrush _entryBoxBgColor;
    public int SelectedTabIndex { get; set; }

    public ICommand ConnectCommand { get; private set; }
    public ICommand ToggleTabsCommand { get; }

    public WfbTabViewModel WfbTabViewModel { get; }
    public WfbGSTabViewModel WfbGSTabViewModel { get; }
    public TelemetryTabViewModel TelemetryTabViewModel { get; }
    public CameraSettingsTabViewModel CameraSettingsTabViewModel { get; }
    public VRXTabViewModel VRXTabViewModel { get; }
    public SetupTabViewModel SetupTabViewModel { get; }
    public ConnectControlsViewModel ConnectControlsViewModel { get; }
    public LogViewerViewModel LogViewerViewModel { get; }
    public StatusBarViewModel StatusBarViewModel { get; }

    private void UpdateSvgPath()
    {
        SvgPath = IsTabsCollapsed
            ? "/Assets/Icons/drawer-open.svg"
            : "/Assets/Icons/drawer-close.svg";
    }

    public DeviceType SelectedDeviceType
    {
        get => _selectedDeviceType;
        set
        {
            // Ignore setting to None if it's due to a binding update
            if (value == DeviceType.None) return;

            if (_selectedDeviceType != value)
            {
                _selectedDeviceType = value;

                // Now only send the message with the selected device type
                SendDeviceTypeMessage(_selectedDeviceType);

                // Trigger any other actions, like OnPropertyChanged if needed
                OnPropertyChanged();
                CheckIfCanConnect();
            }
        }
    }

    private string GetFormattedAppVersion()
    {
        var fullVersion = VersionHelper.GetAppVersion();

        // Extract the first part of the version (e.g., "1.0.0")
        var truncatedVersion = fullVersion.Split('+')[0]; // Handles semantic versions like "1.0.0+buildinfo"
        return truncatedVersion.Length > 10 ? truncatedVersion.Substring(0, 10) : truncatedVersion;
    }

    private void CheckIfCanConnect()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var isValidIp = Utilities.IsValidIpAddress(IpAddress);
            CanConnect = !string.IsNullOrWhiteSpace(Password)
                         && isValidIp
                         && !SelectedDeviceType.Equals(DeviceType.None)
                         && IsConnected;

        });
    }

    partial void OnIpAddressChanged(string value)
    {
        Logger.Verbose($"IP Address changed to: {value}");
        if (!string.IsNullOrEmpty(value))
        {
            if (Utilities.IsValidIpAddress(value))
            {
                Logger.Verbose($"Starting ping timer for valid IP: {value}");
                StartPingTimer();
            }
            else
            {
                Logger.Verbose($"Stopping ping timer for invalid IP: {value}");
                StopPingTimer();
            }
        }
        else
        {
            Logger.Verbose("Stopping ping timer for empty IP");
            StopPingTimer();
        }
    }
    partial void OnPortChanged(int value)
    {
        CheckIfCanConnect();
    }

    partial void OnPasswordChanged(string value)
    {
        CheckIfCanConnect();
    }

    private void SendDeviceTypeMessage(DeviceType deviceType)
    {
        // Insert logic to send a message based on the selected device type
        // For example, use an event aggregator, messenger, or direct call
        Log.Debug($"Device type selected: {deviceType}");
        //Console.WriteLine($"Device type selected: {deviceType}");

        EventSubscriptionService.Publish<DeviceTypeChangeEvent, DeviceType>(deviceType);
    }

    #region Ping Logic


    private void StopPingTimer()
    {
        if (_pingTimer != null)
        {
            CanConnect = false;
            Logger.Debug($"Stopping ping timer for IP: {IpAddress}");

            _pingTimer.Tick -= PingTimer_Tick; // Remove the event handler
            _pingTimer.Stop();
            _pingTimer.IsEnabled = false;
            _pingTimer = null; // Set to null to ensure it's recreated
            
            IsWaiting = true;
        }
    }
    private void StartPingTimer()
    {
        CanConnect = false;
        
        // First, stop any existing timer to avoid duplicates
        StopPingTimer();
    
        _pingTimer = new DispatcherTimer
        {
            Interval = _pingInterval
        };

         _pingTimer.Tick += PingTimer_Tick;
         
         Logger.Verbose($"Starting ping timer for IP: {IpAddress}");
         _pingTimer.Start();
    }
    
    // PingTimer_Tick method
    private async void PingTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            string currentIpAddress = IpAddress; // Assuming this is the property holding the IP
            
            // Check if can connect
            CheckIfCanConnect();
            
            // Important: Store the IP address that was used for this ping operation
            string ipBeingPinged = currentIpAddress;
            
            Logger.Verbose($"PingTimer_Tick executing ping to: {ipBeingPinged}");
            
            if (string.IsNullOrEmpty(ipBeingPinged))
            {
                Logger.Verbose("Empty IP - Setting IsConnected=false, IsWaiting=true");
                IsConnected = false;
                IsWaiting = true;
                return;
            }
        
            try
            {
                var reply = await _pingService.SendPingAsync(ipBeingPinged, (int)_pingTimeout.TotalMilliseconds);
            
                // Only update the UI state if the IP we pinged is still the current IP
                if (ipBeingPinged == IpAddress)
                {
                    if (reply.Status == IPStatus.Success)
                    {
                        Logger.Verbose("Ping successful - Setting IsConnected=true, IsWaiting=false");
                        
                        // used for status color changes
                        IsConnected = true;
                        IsWaiting = false;
                        
                        // enable connect button
                        CanConnect = true;
                    }
                    else
                    {
                        Logger.Verbose("Ping failed - Setting IsConnected=false, IsWaiting=true");
                        
                        // used for status color changes
                        IsConnected = false;
                        IsWaiting = true;
                        
                        // disable connect button, device not ready
                        CanConnect = false;
                    }
                }
                else
                {
                    Logger.Debug($"IP changed during ping from {ipBeingPinged} to {IpAddress} - ignoring result");
                }
            }
            catch (Exception pingEx)
            {
                CanConnect = false;
                // Only update the UI state if the IP we pinged is still the current IP
                if (ipBeingPinged == IpAddress)
                {
                    Logger.Error(pingEx, $"Error occurred during ping to {ipBeingPinged}");
                    IsConnected = false;
                    IsWaiting = true;
                    Logger.Verbose("Current state after exception: IsConnected=False, IsWaiting=True");
                }
                else
                {
                    Logger.Verbose($"IP changed during ping from {ipBeingPinged} to {IpAddress} - ignoring error");
                }
            }
        
            Logger.Verbose($"After ping to {ipBeingPinged}: IsConnected={IsConnected}, IsWaiting={IsWaiting}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unhandled error in PingTimer_Tick");
            IsConnected = false;
            IsWaiting = true;
        }
    }
    
    #endregion

    private async void Connect()
    {
        var appMessage = new AppMessage();
        //DeviceConfig deviceConfig = new DeviceConfig();
        _deviceConfig.Username = "root";
        _deviceConfig.IpAddress = IpAddress;
        _deviceConfig.Password = Password;
        _deviceConfig.Port = Port;
        _deviceConfig.DeviceType = SelectedDeviceType;

        UpdateUIMessage("Getting hostname");
        await getHostname(_deviceConfig);
        if (_deviceConfig.Hostname == string.Empty)
        {
            Log.Error("Failed to get hostname, stopping");
            return;
        }

        var validator = App.ServiceProvider.GetRequiredService<DeviceConfigValidator>();
        if (!validator.IsDeviceConfigValid(_deviceConfig))
        {
            UpdateUIMessage("Hostname Error!");
            var msBox = MessageBoxManager.GetMessageBoxStandard("Hostname Error!",
                $"Hostname does not match device type! \nHostname: {_deviceConfig.Hostname} Device Type: {_selectedDeviceType}.\nPlease check device..\nOk to continue anyway\nCancel to quit",
                ButtonEnum.OkCancel);

            var result = await msBox.ShowAsync();
            if (result == ButtonResult.Cancel)
            {
                Log.Debug("Device selection and hostname mismatch, stopping");
                return;
            }
        }

        await getSensorType(_deviceConfig);
        Sensor = _deviceConfig.SensorType;

        await getNetworkCardType(_deviceConfig);
        NetworkCardType = _deviceConfig.NetworkCardType;

        await getChipType(_deviceConfig);
        Soc = _deviceConfig.ChipType;
        // Save the config to app settings
        SaveConfig();

        // Publish the event
        EventSubscriptionService.Publish<AppMessageEvent, AppMessage>(new AppMessage { DeviceConfig = _deviceConfig });

        // set the background to gray
        EntryBoxBgColor = new SolidColorBrush(Colors.Gray);

        appMessage.DeviceConfig = _deviceConfig;

        if (_deviceConfig != null)
        {
            if (_deviceConfig.DeviceType == DeviceType.Camera)
            {
                UpdateUIMessage("Processing Camera...");
                processCameraFiles();
                UpdateUIMessage("Processing Camera...done");
            }
            else if (_deviceConfig.DeviceType == DeviceType.Radxa)
            {
                UpdateUIMessage("Processing Radxa...");
                processRadxaFiles();
                UpdateUIMessage("Processing Radxa...done");
            }
        }

        UpdateUIMessage("Connected");
    }

    
    
    private void SaveConfig()
    {
        _deviceConfig.DeviceType = SelectedDeviceType;
        _deviceConfig.IpAddress = IpAddress;
        _deviceConfig.Port = Port;
        _deviceConfig.Password = Password;

        // save config to file
        SettingsManager.SaveSettings(_deviceConfig);
    }

    /// <summary>
    ///     Retrieves the hostname of the device asynchronously using SSH.
    ///     <para>
    ///         The command execution is cancelled after 10 seconds if no response is received.
    ///         If the command execution times out, a message box is displayed with an error message.
    ///     </para>
    /// </summary>
    /// <param name="deviceConfig">The device configuration to use for the SSH connection.</param>
    private async Task getHostname(DeviceConfig deviceConfig)
    {
        deviceConfig.Hostname = string.Empty;

        var cts = new CancellationTokenSource(10000); // 10 seconds
        var cancellationToken = cts.Token;

        var cmdResult =
            await SshClientService.ExecuteCommandWithResponseAsync(deviceConfig, DeviceCommands.GetHostname,
                cancellationToken);

        // If the command execution takes longer than 10 seconds, the task will be cancelled
        if (cmdResult == null)
        {
            // Handle the timeout
            // .
            var resp = MessageBoxManager.GetMessageBoxStandard("Timeout Error!",
                "The command took too long to execute. Please check device..");
            await resp.ShowAsync();
            return;
        }

        var hostName = Utilities.RemoveSpecialCharacters(cmdResult.Result);
        deviceConfig.Hostname = hostName;
        //_deviceConfig.Hostname = hostName;
        //Hostname = hostName;

        // Cleanup
        cts.Dispose();
    }

    /// <summary>
    ///     Retrieves the chip type or SOC of the device asynchronously using SSH.
    ///     <para>
    ///         The command execution is cancelled after 10 seconds if no response is received.
    ///         If the command execution times out, a message box is displayed with an error message.
    ///     </para>
    /// </summary>
    /// <param name="deviceConfig">The device configuration to use for the SSH connection.</param>
    private async Task getChipType(DeviceConfig deviceConfig)
    {
        deviceConfig.ChipType = string.Empty;

        var cts = new CancellationTokenSource(10000); // 10 seconds
        var cancellationToken = cts.Token;

        var cmdResult =
            await SshClientService.ExecuteCommandWithResponseAsync(deviceConfig, DeviceCommands.GetChipType,
                cancellationToken);

        // If the command execution takes longer than 10 seconds, the task will be cancelled
        if (cmdResult == null)
        {
            // Handle the timeout
            // .
            var resp = MessageBoxManager.GetMessageBoxStandard("Timeout Error!",
                "The command took too long to execute. Please check device..");
            await resp.ShowAsync();
            return;
        }

        var chipType = Utilities.RemoveSpecialCharacters(cmdResult.Result);
        deviceConfig.ChipType = chipType;


        // Cleanup
        cts.Dispose();
    }

    /// <summary>
    ///     Retrieves the snesor type of the device asynchronously using SSH.
    ///     <para>
    ///         The command execution is cancelled after 10 seconds if no response is received.
    ///         If the command execution times out, a message box is displayed with an error message.
    ///     </para>
    /// </summary>
    /// <param name="deviceConfig">The device configuration to use for the SSH connection.</param>
    private async Task getSensorType(DeviceConfig deviceConfig)
    {
        deviceConfig.SensorType = string.Empty;

        var cts = new CancellationTokenSource(10000); // 10 seconds
        var cancellationToken = cts.Token;

        var cmdResult =
            await SshClientService.ExecuteCommandWithResponseAsync(deviceConfig, DeviceCommands.GetSensorType,
                cancellationToken);

        // If the command execution takes longer than 10 seconds, the task will be cancelled
        if (cmdResult == null)
        {
            // Handle the timeout
            // .
            var resp = MessageBoxManager.GetMessageBoxStandard("Timeout Error!",
                "The command took too long to execute. Please check device..");
            await resp.ShowAsync();
            return;
        }

        var sensorType = Utilities.RemoveSpecialCharacters(cmdResult.Result);
        deviceConfig.SensorType = sensorType;


        // Cleanup
        cts.Dispose();
    }

    /// <summary>
    ///     Retrieves the network card type of the device asynchronously using SSH.
    ///     <para>
    ///         The command execution is cancelled after 10 seconds if no response is received.
    ///         If the command execution times out, a message box is displayed with an error message.
    ///     </para>
    /// </summary>
    /// <param name="deviceConfig">The device configuration to use for the SSH connection.</param>
    private async Task getNetworkCardType(DeviceConfig deviceConfig)
    {
        deviceConfig.NetworkCardType = string.Empty;

        var cts = new CancellationTokenSource(10000); // 10 seconds
        var cancellationToken = cts.Token;

        var cmdResult =
            await SshClientService.ExecuteCommandWithResponseAsync(deviceConfig, DeviceCommands.GetNetworkCardType,
                cancellationToken);

        // If the command execution takes longer than 10 seconds, the task will be cancelled
        if (cmdResult == null)
        {
            // Handle the timeout
            // .
            var resp = MessageBoxManager.GetMessageBoxStandard("Timeout Error!",
                "The command took too long to execute. Please check device..");
            await resp.ShowAsync();
            return;
        }

        var lsusbResults = Utilities.RemoveLastChar(cmdResult.Result);
        var networkCardType = WifiCardDetector.DetectWifiCard(lsusbResults);
        deviceConfig.NetworkCardType = networkCardType;


        // Cleanup
        cts.Dispose();
    }

    private async void processCameraFiles()
    {
        // read device to determine configurations
        await _globalSettingsSettingsViewModel.ReadDevice();
        Logger.Debug($"IsWfbYamlEnabled = {_globalSettingsSettingsViewModel.IsWfbYamlEnabled}");

        // remove when ready
        _globalSettingsSettingsViewModel.IsWfbYamlEnabled = false;
        
        if (_globalSettingsSettingsViewModel.IsWfbYamlEnabled)
        {
            try
            {
                // download file wfb.yaml
                Logger.Debug($"Reading wfb.yaml");

                var wfbContent = await SshClientService.DownloadFileAsync(_deviceConfig, OpenIPC.WfbYamlFileLoc);

                // if (wfbContent != null)
                //     EventSubscriptionService.Publish<WfbYamlContentUpdatedEvent,
                //         WfbYamlContentUpdatedMessage>(new WfbYamlContentUpdatedMessage(wfbContent));
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }
        else
        {
            Logger.Debug($"Reading legacy settings");
            // download file wfb.conf
            var wfbConfContent = await SshClientService.DownloadFileAsync(_deviceConfig, OpenIPC.WfbConfFileLoc);


            if (wfbConfContent != null)
                EventSubscriptionService.Publish<WfbConfContentUpdatedEvent,
                    WfbConfContentUpdatedMessage>(new WfbConfContentUpdatedMessage(wfbConfContent));

            try
            {
                var majesticContent =
                    await SshClientService.DownloadFileAsync(_deviceConfig, OpenIPC.MajesticFileLoc);
                // Publish a message to WfbSettingsTabViewModel
                EventSubscriptionService.Publish<MajesticContentUpdatedEvent,
                    MajesticContentUpdatedMessage>(new MajesticContentUpdatedMessage(majesticContent));
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            try
            {
                var telemetryContent =
                    await SshClientService.DownloadFileAsync(_deviceConfig, OpenIPC.TelemetryConfFileLoc);
                // Publish a message to WfbSettingsTabViewModel

                EventSubscriptionService.Publish<TelemetryContentUpdatedEvent,
                    TelemetryContentUpdatedMessage>(new TelemetryContentUpdatedMessage(telemetryContent));
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }

            try
            {
                // get /home/radxa/scripts/screen-mode
                var droneKeyContent =
                    await SshClientService.DownloadFileBytesAsync(_deviceConfig, OpenIPC.RemoteDroneKeyPath);


                if (droneKeyContent != null)
                {
                    //byte[] fileBytes = Encoding.UTF8.GetBytes(droneKeyContent);

                    var droneKey = Utilities.ComputeMd5Hash(droneKeyContent);

                    var deviceContentUpdatedMessage = new DeviceContentUpdatedMessage();
                    _deviceConfig = DeviceConfig.Instance;
                    _deviceConfig.KeyChksum = droneKey;
                    deviceContentUpdatedMessage.DeviceConfig = _deviceConfig;

                    EventSubscriptionService.Publish<DeviceContentUpdateEvent,
                        DeviceContentUpdatedMessage>(deviceContentUpdatedMessage);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }

        EventSubscriptionService.Publish<AppMessageEvent,
            AppMessage>(new AppMessage { CanConnect = DeviceConfig.Instance.CanConnect, DeviceConfig = _deviceConfig });

        Log.Information("Done reading files from device.");
    }

    private async void processRadxaFiles()
    {
        try
        {
            UpdateUIMessage("Downloading wifibroadcast.cfg");

            // get /etc/wifibroadcast.cfg
            var wifibroadcastContent =
                await SshClientService.DownloadFileAsync(_deviceConfig, OpenIPC.WifiBroadcastFileLoc);

            if (!string.IsNullOrEmpty(wifibroadcastContent))
            {
                var radxaContentUpdatedMessage = new RadxaContentUpdatedMessage();
                radxaContentUpdatedMessage.WifiBroadcastContent = wifibroadcastContent;

                EventSubscriptionService.Publish<RadxaContentUpdateChangeEvent,
                    RadxaContentUpdatedMessage>(new RadxaContentUpdatedMessage
                    { WifiBroadcastContent = wifibroadcastContent });
            }
            else
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", "Failed to download /etc/wifibroadcast.cfg")
                    .ShowAsync();
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            throw;
        }


        try
        {
            UpdateUIMessage("Downloading modprod.d/wfb.conf");
            // get /etc/modprobe.d/wfb.conf
            var wfbModProbeContent =
                await SshClientService.DownloadFileAsync(_deviceConfig, OpenIPC.WifiBroadcastModProbeFileLoc);

            if (wfbModProbeContent != null)
            {
                var radxaContentUpdatedMessage = new RadxaContentUpdatedMessage();
                radxaContentUpdatedMessage.WfbConfContent = wfbModProbeContent;

                EventSubscriptionService.Publish<RadxaContentUpdateChangeEvent,
                    RadxaContentUpdatedMessage>(new RadxaContentUpdatedMessage { WfbConfContent = wfbModProbeContent });
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            throw;
        }


        try
        {
            UpdateUIMessage("Downloading screen-mode");
            // get /home/radxa/scripts/screen-mode
            var screenModeContent =
                await SshClientService.DownloadFileAsync(_deviceConfig, OpenIPC.ScreenModeFileLoc);

            if (screenModeContent != null)
            {
                var radxaContentUpdatedMessage = new RadxaContentUpdatedMessage();
                radxaContentUpdatedMessage.ScreenModeContent = screenModeContent;

                EventSubscriptionService.Publish<RadxaContentUpdateChangeEvent,
                    RadxaContentUpdatedMessage>(
                    new RadxaContentUpdatedMessage { ScreenModeContent = screenModeContent });
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            throw;
        }

        try
        {
            UpdateUIMessage("Downloading gskey");

            var gsKeyContent =
                await SshClientService.DownloadFileBytesAsync(_deviceConfig, OpenIPC.RemoteGsKeyPath);

            if (gsKeyContent != null)
            {
                var droneKey = Utilities.ComputeMd5Hash(gsKeyContent);
                if (droneKey != OpenIPC.KeyMD5Sum)
                    Log.Warning("GS key MD5 checksum mismatch");
                else
                    Log.Information("GS key MD5 checksum matched default key");

                EventSubscriptionService.Publish<RadxaContentUpdateChangeEvent,
                    RadxaContentUpdatedMessage>(new RadxaContentUpdatedMessage { DroneKeyContent = droneKey });


                UpdateUIMessage("Downloading gskey...done");
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            throw;
        }

        EventSubscriptionService.Publish<AppMessageEvent, AppMessage>(new AppMessage
            { CanConnect = DeviceConfig.Instance.CanConnect, DeviceConfig = _deviceConfig });

        Log.Information("Done reading files from device.");
    }

    private void LoadSettings()
    {
        IsWaiting = true;
        // Load settings via the SettingsManager
        var settings = SettingsManager.LoadSettings();
        _deviceConfig = DeviceConfig.Instance;
        IpAddress = settings.IpAddress;
        Password = settings.Password;
        Port = settings.Port == 0 ? 22 : settings.Port;
        SelectedDeviceType = settings.DeviceType;

        // Publish the initial device type
        EventSubscriptionService.Publish<DeviceTypeChangeEvent, DeviceType>(settings.DeviceType);
    }

    private void OnDeviceTypeChangeEvent(DeviceType deviceTypeEvent)
    {
        Log.Debug($"Device type changed to: {deviceTypeEvent}");

        InitializeTabs(deviceTypeEvent);

        // Update IsVRXEnabled based on the device type
        //IsVRXEnabled = deviceTypeEvent == DeviceType.Radxa || deviceTypeEvent == DeviceType.NVR;

        // Update the selected tab based on the device type
        //SelectedTab = IsVRXEnabled ? "WFB-GS" : "WFB";

        // Notify the view of tab changes
        //EventSubscriptionService.Publish<TabSelectionChangeEvent, string>(SelectedTab);
    }

    #region Observable Properties

    [ObservableProperty] private bool _canConnect;
    [ObservableProperty] private string _appVersionText;
    [ObservableProperty] private string _ipAddress;
    [ObservableProperty] private int _port;
    [ObservableProperty] private string _password;
    [ObservableProperty] private string _device;
    [ObservableProperty] private bool isVRXEnabled;
    [ObservableProperty] private DeviceConfig _deviceConfig;
    [ObservableProperty] private TabItemViewModel _selectedTab;

    #endregion
}