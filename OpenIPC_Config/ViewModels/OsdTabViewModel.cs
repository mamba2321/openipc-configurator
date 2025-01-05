using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Parsers;
using OpenIPC_Config.Services;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class OsdTabViewModel : ViewModelBase
{
    public ObservableCollection<double> VerticalGridLines { get; } = new();
    public ObservableCollection<double> HorizontalGridLines { get; } = new();

    private VdecConfParser _vdecConfParser;
    
    [ObservableProperty] private bool _canConnect;
    public ICommand SaveOSDCommand { get; private set; }
    
    public OsdTabViewModel(ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        
        EventSubscriptionService.Subscribe<NvrContentUpdateChangeEvent, NvrContentUpdatedMessage>(OnNvrContentUpdatedMessage );
        EventSubscriptionService.Subscribe<AppMessageEvent, AppMessage>(OnAppMessageEvent);
        
        SaveOSDCommand = new RelayCommand(() => SaveOSD());
        
        
        foreach (var item in OverlayItems)
        {
            Log.Logger.Verbose($"Name: {item.Name}, PositionX: {item.PositionX}, PositionY: {item.PositionY}");
        }

        createGridLines();
    }

    private void OnAppMessageEvent(AppMessage appMessage)
    {
        if(appMessage != null)
        {
            // controls buttons
            CanConnect = appMessage.CanConnect;
        }
        
        
    }
    
    private void OnNvrContentUpdatedMessage(NvrContentUpdatedMessage nvrContentUpdatedMessage)
    {
            if (!string.IsNullOrEmpty(nvrContentUpdatedMessage.VdecContent))
            {
                _vdecConfParser = VdecConfParser.ReadConfig(nvrContentUpdatedMessage.VdecContent);
                //vdecConfig.ApplyToOverlayItems(OverlayItems);
                OsdElementsParser.Parse(nvrContentUpdatedMessage.VdecContent, OverlayItems);
                
            }

            
        
    }
    private void createGridLines()
    {
        // Populate grid lines at regular intervals
        for (double i = 0; i <= 480; i += 20) // Horizontal lines every 20 pixels
        {
            HorizontalGridLines.Add(i);
        }

        for (double i = 0; i <= 731; i += 20) // Vertical lines every 20 pixels
        {
            VerticalGridLines.Add(i);
        }
    }

    private async void SaveOSD()
    {
        var overlayElements = OsdElementsParser.GenerateOsdElements(OverlayItems); // Generate osd_elements string()
        
        _vdecConfParser.OsdElements = overlayElements;
        var vdecConfig = _vdecConfParser.GenerateConfig();
        
        SshClientService.UploadFileStringAsync(DeviceConfig.Instance, Models.OpenIPC.VdecConfFileLoc, vdecConfig); // Upload vdec_conf()
        SshClientService.ExecuteCommandAsync(DeviceConfig.Instance, DeviceCommands.RestartVdecCommand); // Reboot()

    }

    public ObservableCollection<OverlayItem> OverlayItems { get; } = new()
    {
        new OverlayItem { Name = "ALT", DisplayValue = "ALT",PositionX = 511, PositionY = 193, IsVisible = true },
        new OverlayItem { Name = "SPD", DisplayValue="SPD",PositionX = 114, PositionY = 195, IsVisible = true },
        new OverlayItem { Name = "VSPD", DisplayValue = "VSPD",PositionX = 511, PositionY = 233, IsVisible = true },
        new OverlayItem { Name = "BAT", DisplayValue="BAT", PositionX = 16, PositionY = 433, IsVisible = true },
        new OverlayItem { Name = "CONS", DisplayValue = "CONS", PositionX = 13, PositionY = 403, IsVisible = true },
        new OverlayItem { Name = "CUR", DisplayValue = "CUR",PositionX = 13, PositionY = 373, IsVisible = true },
        new OverlayItem { Name = "THR", DisplayValue = "THR", PositionX = 318, PositionY = 233, IsVisible = true },
        new OverlayItem { Name = "SATS", DisplayValue = "SATS", PositionX = 643, PositionY = 433, IsVisible = true },
        new OverlayItem { Name = "HDG", DisplayValue = "HDG", PositionX = 643, PositionY = 342, IsVisible = true },
        new OverlayItem { Name = "LAT", DisplayValue = "LAT", PositionX = 581, PositionY = 363, IsVisible = true },
        new OverlayItem { Name = "LON", DisplayValue = "LON",PositionX = 580, PositionY = 403, IsVisible = true },
        new OverlayItem { Name = "DIST", DisplayValue = "DIST",PositionX = 146, PositionY = 433, IsVisible = true },
        new OverlayItem { Name = "RSSI", DisplayValue = "RSSI",PositionX = 340, PositionY = 136, IsVisible = true },
        new OverlayItem { Name = "OpenIPC", DisplayValue = "OpenIPC" ,PositionX = 561, PositionY = 3, IsVisible = true },
        new OverlayItem { Name = "RX Packets", DisplayValue = "RX Packets",PositionX = 337, PositionY = 12, IsVisible = true },
        new OverlayItem { Name = "Bitrate", DisplayValue = "Bitrate",PositionX = 160, PositionY = 8, IsVisible = true },
        
        new OverlayItem { Name = "TIME", DisplayValue = "TIME",PositionX = 643, PositionY = 310, IsVisible = true },
        new OverlayItem { Name = "HORIZON", DisplayValue = "-----------------------HORIZON -----------------------",PositionX = 206, PositionY = 194, IsVisible = true },
        new OverlayItem { Name = "TEMP", DisplayValue = "TEMP",PositionX = 200, PositionY = 21, IsVisible = true },
    };
}