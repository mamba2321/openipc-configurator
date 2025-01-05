using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using MsBox.Avalonia;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Parsers;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class WfbGSTabViewModel : ViewModelBase
{
    private readonly Dictionary<int, string> _24FrequencyMapping = FrequencyMappings.Frequency24GHz;
    private readonly Dictionary<int, string> _58FrequencyMapping = FrequencyMappings.Frequency58GHz;

    
    private readonly Dictionary<int, string> _frequencyMapping = new()
    {
        { 1, "2412 MHz [1]" },
        { 2, "2417 MHz [2]" },
        { 3, "2422 MHz [3]" },
        { 4, "2427 MHz [4]" },
        { 5, "2432 MHz [5]" },
        { 6, "2437 MHz [6]" },
        { 7, "2442 MHz [7]" },
        { 8, "2447 MHz [8]" },
        { 9, "2452 MHz [9]" },
        { 10, "2457 MHz [10]" },
        { 11, "2462 MHz [11]" },
        { 12, "2467 MHz [12]" },
        { 13, "2472 MHz [13]" },
        { 14, "2484 MHz [14]" },
        { 36, "5180 MHz [36]" },
        { 40, "5200 MHz [40]" },
        { 44, "5220 MHz [44]" },
        { 48, "5240 MHz [48]" },
        { 52, "5260 MHz [52]" },
        { 56, "5280 MHz [56]" },
        { 60, "5300 MHz [60]" },
        { 64, "5320 MHz [64]" },
        { 100, "5500 MHz [100]" },
        { 104, "5520 MHz [104]" },
        { 108, "5540 MHz [108]" },
        { 112, "5560 MHz [112]" },
        { 116, "5580 MHz [116]" },
        { 120, "5600 MHz [120]" },
        { 124, "5620 MHz [124]" },
        { 128, "5640 MHz [128]" },
        { 132, "5660 MHz [132]" },
        { 136, "5680 MHz [136]" },
        { 140, "5700 MHz [140]" },
        { 144, "5720 MHz [144]" },
        { 149, "5745 MHz [149]" },
        { 153, "5765 MHz [153]" },
        { 157, "5785 MHz [157]" },
        { 161, "5805 MHz [161]" },
        { 165, "5825 MHz [165]" },
        { 169, "5845 MHz [169]" },
        { 173, "5865 MHz [173]" },
        { 177, "5885 MHz [177]" }
    };

    #region Properies

    [ObservableProperty] private int _selectedChannel;

    [ObservableProperty] private int _selectedPower24GHz;
    
    [ObservableProperty] private int _selectedBandwidth;

    [ObservableProperty] private int _selectedPower;

    
    [ObservableProperty] private ObservableCollection<string> _frequencies58GHz;

    [ObservableProperty] private ObservableCollection<string> _frequencies24GHz;

    [ObservableProperty] private ObservableCollection<int> _power58GHz;

    [ObservableProperty] private ObservableCollection<int> _power24GHz;
    
    [ObservableProperty] private DeviceConfig _deviceConfig;
    
    [ObservableProperty] private bool _canConnect;

    [ObservableProperty] private ObservableCollection<string> _frequencies;
    [ObservableProperty] private string _gsMavlink;
    [ObservableProperty] private string _gsVideo;
    [ObservableProperty] private ObservableCollection<int> _power;
    [ObservableProperty] private string _wifiRegion;
    [ObservableProperty] private bool _isNvrDevice;
    [ObservableProperty] private bool _isRadxaDevice;
    [ObservableProperty] private string _selectedFrequency24String;
    [ObservableProperty] private string _selectedFrequency58String;
    
    #endregion 
    
    partial void OnSelectedFrequency24StringChanged(string value)
    {
        if (!string.IsNullOrEmpty(value)) HandleFrequencyChange(value, _24FrequencyMapping);
    }

    partial void OnSelectedFrequency58StringChanged(string value)
    {
        if (!string.IsNullOrEmpty(value)) HandleFrequencyChange(value, _58FrequencyMapping);
    }

    private readonly WfbGsConfigParser _wfbGsConfigParser;
    private readonly WifiConfigParser _wifiConfigParser;
    private WfbConfParser _wfbConfParser;

    
    public WfbGSTabViewModel(ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        _wfbGsConfigParser = new WfbGsConfigParser();
        _wifiConfigParser = new WifiConfigParser();

        //DeviceConfig = DeviceConfig.Instance;
        InitializeCollections();
        
        EventSubscriptionService.Subscribe<DeviceTypeChangeEvent, DeviceType>(OnDeviceTypeChangeEvent);
        EventSubscriptionService.Subscribe<NvrContentUpdateChangeEvent, NvrContentUpdatedMessage>(OnNvrContentUpdatedMessage);
        EventSubscriptionService.Subscribe<RadxaContentUpdateChangeEvent, RadxaContentUpdatedMessage>(OnRadxaContentUpdateChange);
        EventSubscriptionService.Subscribe<AppMessageEvent, AppMessage>(OnAppMessage);
        
    }


    private void OnAppMessage(AppMessage appMessage)
    {
        if (appMessage.CanConnect) CanConnect = appMessage.CanConnect;
        //Log.Information($"CanConnect {CanConnect.ToString()}");

        DeviceConfig = appMessage.DeviceConfig;

    }

    /// <summary>
    /// //frequency 
    //power
    //region
    /// </summary>
    [RelayCommand]
    private async Task RestartWfb()
    {
        //await MessageBoxManager.GetMessageBoxStandard("Warning", "Not implemented yet", ButtonEnum.Ok).ShowAsync();
        Log.Information("Restart WFB button clicked");

        var newFrequency58 = SelectedFrequency58String;
        var newFrequency24 = SelectedFrequency24String;

        var newPower58 = SelectedPower;
        var newPower24 = SelectedPower24GHz;
        var newBandwidth = SelectedBandwidth;
        var newChannel = SelectedChannel;
        var newWifiRegion = WifiRegion;
         
        
        if (IsRadxaDevice)
        {
            // /etc/wifibroadcast.cfg
            await UpdateWifiBroadcastCfg();

            // /etc/modprobe.d/wfb.conf
            await UpdateModprobeWfbConf();
    
        }

        if (IsNvrDevice)
        {
            _wfbConfParser.Channel = newChannel;
            _wfbConfParser.Region = newWifiRegion;
            var updatedWfbString = _wfbConfParser.GenerateConfig();
            SshClientService.UploadFileStringAsync(DeviceConfig.Instance, Models.OpenIPC.WfbConfFileLoc, updatedWfbString); // Upload wfb_conf()


            // Update WfbConfContent with the new values

        }
        
        await MessageBoxManager.GetMessageBoxStandard("Success", "Saved!").ShowAsync();
    }

    
    
    private async Task UpdateModprobeWfbConf()
    {
        try
        {
            _wfbGsConfigParser.TxPower = SelectedPower.ToString();
            // Update the parser's properties based on user input
            var updatedConfigString = _wfbGsConfigParser.GetUpdatedConfigString();

            if (string.IsNullOrEmpty(updatedConfigString))
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", "Updated configuration is empty").ShowAsync();
                return;
            }

            // Upload the updated configuration file
            SshClientService.UploadFileStringAsync(DeviceConfig.Instance, Models.OpenIPC.WifiBroadcastModProbeFileLoc,
                updatedConfigString);
            Log.Information("Configuration file updated and uploaded successfully.");
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            throw;
        }
    }
    
    

    private void HandleFrequencyChange(string newValue, Dictionary<int, string> frequencyMapping)
    {
        // Reset the other frequency collection to its first value
        if (frequencyMapping == _24FrequencyMapping)
        {
            SelectedFrequency58String = Frequencies58GHz.FirstOrDefault();
            SelectedPower = Power58GHz.FirstOrDefault();
        }
        else if (frequencyMapping == _58FrequencyMapping)
        {
            SelectedFrequency24String = Frequencies24GHz.FirstOrDefault();
            SelectedPower24GHz = Power24GHz.FirstOrDefault();
        }

        // Extract the channel number using a regular expression
        var match = Regex.Match(newValue, @"\[(\d+)\]");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var channel))
            SelectedChannel = channel;
        else
            SelectedChannel = -1; // Default value if parsing fails
    }
    
    private async Task UpdateWifiBroadcastCfg()
    {
        // update /etc/wifibroadcast.cfg
        try
        {
            // Update the parser's properties based on user input

            _wifiConfigParser.WifiChannel = SelectedChannel;
            _wifiConfigParser.WifiRegion = WifiRegion;
            _wifiConfigParser.GsMavlinkPeer = GsMavlink;
            _wifiConfigParser.GsVideoPeer = GsVideo;

            // Get the updated configuration string
            // Generate the updated configuration string
            var updatedConfigContent = _wifiConfigParser.GetUpdatedConfigString();

            if (string.IsNullOrEmpty(updatedConfigContent))
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", "Updated configuration is empty").ShowAsync();
                return;
            }

            // Upload the updated configuration file
            SshClientService.UploadFileStringAsync(DeviceConfig.Instance, Models.OpenIPC.WifiBroadcastFileLoc,
                updatedConfigContent);
            Log.Information("Configuration file updated and uploaded successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update configuration file.");
            await MessageBoxManager.GetMessageBoxStandard("Error", "Failed to update configuration.").ShowAsync();
        }
    }

    private void InitializeCollections()
    {
        Frequencies58GHz = new ObservableCollectionExtended<string>(_58FrequencyMapping.Values);
        Frequencies24GHz = new ObservableCollectionExtended<string>(_24FrequencyMapping.Values);
        Power58GHz = new ObservableCollectionExtended<int> { 1, 5, 10, 15, 20, 25, 30 };
        Power24GHz = new ObservableCollectionExtended<int> { 1, 20, 25, 30, 35, 40 };
    }

    private  void OnDeviceTypeChangeEvent(DeviceType deviceType)
    {
        IsRadxaDevice = (deviceType == DeviceType.Radxa);
        IsNvrDevice = (deviceType == DeviceType.NVR);
    }
    
    /// <summary>
    /// Handles the frequency key input event.
    /// </summary>
    /// <param name="value">The input value representing the frequency key.</param>
    /// <remarks>
    /// This method is called when the frequency key input event is triggered, and it is responsible for parsing the input value and updating the corresponding frequency settings.
    /// </remarks>
    private void HandleFrequencyKey(string value)
    {
        if (int.TryParse(value, out var frequency))
        {
            SelectedFrequency58String = _58FrequencyMapping.ContainsKey(frequency)
                ? _58FrequencyMapping[frequency]
                : SelectedFrequency58String;

            SelectedFrequency24String = _24FrequencyMapping.ContainsKey(frequency)
                ? _24FrequencyMapping[frequency]
                : SelectedFrequency24String;
        }
    }
    
    
    /// <summary>
    /// Handles the NVR content updated message event.
    /// </summary>
    /// <param name="nvrContentUpdatedMessage">The NVR content updated message containing the updated configuration data.</param>
    /// <remarks>
    /// This method is called when the NVR content is updated, and it is responsible for parsing the updated configuration data and updating the corresponding properties in the view model.
    /// </remarks>
    private void OnNvrContentUpdatedMessage(NvrContentUpdatedMessage nvrContentUpdatedMessage)
    {
        var wfbConfContent = nvrContentUpdatedMessage.WfbConfContent;
        if (!string.IsNullOrEmpty(wfbConfContent))
        {
            _wfbConfParser = WfbConfParser.ReadConfig(wfbConfContent); 

            var channel = _wfbConfParser.Channel;
            
            //if (_frequencyMapping.TryGetValue(channel, out frequencyString)) SelectedFrequencyString = frequencyString;
            HandleFrequencyKey(channel.ToString());
            
            var power = _wfbConfParser.DriverTxPowerOverride;
            SelectedPower = power;
            
            var region = _wfbConfParser.Region;
            if (!string.IsNullOrEmpty(region)) WifiRegion = region;


        }
    }
    
    private void OnRadxaContentUpdateChange(RadxaContentUpdatedMessage radxaContentUpdatedMessage)
    {
        var wifiBroadcastContent = radxaContentUpdatedMessage.WifiBroadcastContent;
        if (!string.IsNullOrEmpty(wifiBroadcastContent))
        {
            var configContent = radxaContentUpdatedMessage.WifiBroadcastContent;

            _wifiConfigParser.ParseConfigString(configContent);

            var channel = _wifiConfigParser.WifiChannel;

            // string frequencyString;
            // if (_frequencyMapping.TryGetValue(channel, out frequencyString)) SelectedFrequencyString = frequencyString;

            var wifiRegion = _wifiConfigParser.WifiRegion;
            if (!string.IsNullOrEmpty(wifiRegion)) WifiRegion = _wifiConfigParser.WifiRegion;

            var gsMavlink = _wifiConfigParser.GsMavlinkPeer;
            if (!string.IsNullOrEmpty(gsMavlink)) GsMavlink = _wifiConfigParser.GsMavlinkPeer;

            var gsVideo = _wifiConfigParser.GsVideoPeer;
            if (!string.IsNullOrEmpty(gsVideo)) GsVideo = _wifiConfigParser.GsVideoPeer;
        }

        var wfbConfContent = radxaContentUpdatedMessage.WfbConfContent;
        if (!string.IsNullOrEmpty(wfbConfContent))
        {
            _wfbGsConfigParser.ParseConfigString(wfbConfContent);
            var power = _wfbGsConfigParser.TxPower;
            if (int.TryParse(power, out var parsedPower)) SelectedPower = parsedPower;
        }
    }
}