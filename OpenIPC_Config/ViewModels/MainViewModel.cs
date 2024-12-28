﻿using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    public bool IsMobile { get; }
    
    [ObservableProperty]
    private bool isVRXEnabled;
    
    [ObservableProperty]
    private DeviceConfig _deviceConfig;

    [ObservableProperty]
    private string selectedTab;
    
    public WfbTabViewModel WfbTabViewModel { get; }
    public WfbGSTabViewModel WfbGSTabViewModel { get; }
    public TelemetryTabViewModel TelemetryTabViewModel { get; }
    public CameraSettingsTabViewModel CameraSettingsTabViewModel { get; }
    public VRXTabViewModel VRXTabViewModel { get; }
    public SetupTabViewModel SetupTabViewModel { get; }
    public ConnectControlsViewModel ConnectControlsViewModel { get; }
    public LogViewerViewModel LogViewerViewModel { get; }
    public StatusBarViewModel StatusBarViewModel { get; }

    public MainViewModel(ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        
        IsMobile = App.IsMobile;
        
        // Subscribe to device type change events
        EventSubscriptionService.Subscribe<DeviceTypeChangeEvent, DeviceType>(
            OnDeviceTypeChangeEvent);
        
        IsVRXEnabled = false;

        LoadSettings();

    }

    private void LoadSettings()
    {
        // Load settings via the SettingsManager
        var settings = SettingsManager.LoadSettings();
        _deviceConfig = DeviceConfig.Instance;

        // Publish the initial device type
        EventSubscriptionService.Publish<DeviceTypeChangeEvent, DeviceType>(settings.DeviceType);
        
    }

    private void OnDeviceTypeChangeEvent(DeviceType deviceTypeEvent)
    {
        Log.Debug($"Device type changed to: {deviceTypeEvent}");

        // Update IsVRXEnabled based on the device type
        IsVRXEnabled = deviceTypeEvent == DeviceType.Radxa || deviceTypeEvent == DeviceType.NVR;

        // Update the selected tab based on the device type
        SelectedTab = IsVRXEnabled ? "WFB-GS" : "WFB";

        // Notify the view of tab changes
        EventSubscriptionService.Publish<TabSelectionChangeEvent, string>(SelectedTab);
    }
}
