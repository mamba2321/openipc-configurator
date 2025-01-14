using Avalonia;
using Moq;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using OpenIPC_Config.ViewModels;
using Serilog;

namespace OpenIPC_Config.Tests.ViewModels;

[TestFixture]
public class TelemetryTabViewModelTests
{
    private Mock<ILogger> _mockLogger;
    private Mock<ISshClientService> _mockSshClientService;
    private Mock<IEventSubscriptionService> _mockEventSubscriptionService;
    private TelemetryTabViewModel _viewModel;
    private Mock<IMessageBoxService> _mockMessageBoxService;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger>();
        _mockSshClientService = new Mock<ISshClientService>();
        _mockEventSubscriptionService = new Mock<IEventSubscriptionService>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();

        _viewModel = new TelemetryTabViewModel(
            _mockLogger.Object,
            _mockSshClientService.Object,
            _mockEventSubscriptionService.Object,
            _mockMessageBoxService.Object
        );
        
        // Initialize default telemetry content
        _viewModel.TelemetryContent = "serial=/dev/ttyS0\nbaud=9600\nrouter=1\nmcs_index=5\naggregate=4\nchannels=3";
        
        
    }
    

    [Test]
    public void Constructor_InitializesWithDefaults()
    {
        Assert.IsNotNull(_viewModel.SerialPorts);
        Assert.IsNotNull(_viewModel.BaudRates);
        Assert.IsNotNull(_viewModel.McsIndex);
        Assert.AreEqual("/dev/ttyS0", _viewModel.SerialPorts[0]);
        Assert.AreEqual("4800", _viewModel.BaudRates[0]);
        Assert.AreEqual("0", _viewModel.McsIndex[0]);
    }

    [Test]
    public void HandleTelemetryContentUpdated_ParsesAndSetsProperties()
    {
        // Arrange
        var telemetryContent = "serial=/dev/ttyS0\nbaud=9600\nrouter=1\nmcs_index=3";
        var message = new TelemetryContentUpdatedMessage( telemetryContent );

        // Act
        _viewModel.HandleTelemetryContentUpdated(message);

        // Assert
        Assert.AreEqual("/dev/ttyS0", _viewModel.SelectedSerialPort);
        Assert.AreEqual("9600", _viewModel.SelectedBaudRate);
        Assert.AreEqual("1", _viewModel.SelectedRouter);
        Assert.AreEqual("3", _viewModel.SelectedMcsIndex);
    }

    [Test]
    public void SaveAndRestartTelemetry_UpdatesAndUploadsTelemetryContent()
    {
        // Arrange
        _viewModel.SelectedSerialPort = "/dev/ttyS0";
        _viewModel.SelectedBaudRate = "9600";
        _viewModel.SelectedRouter = "1";
        _viewModel.SelectedMcsIndex = "3";
        _viewModel.SelectedAggregate = "10";
        _viewModel.SelectedRcChannel = "2";

        // Act
        _viewModel.SaveAndRestartTelemetryCommand.Execute(null);

        // Assert
        _mockSshClientService.Verify(
            service => service.UploadFileStringAsync(
                It.IsAny<DeviceConfig>(),
                Models.OpenIPC.TelemetryConfFileLoc,
                It.Is<string>(content =>
                    content.Contains("serial=/dev/ttyS0") &&
                    content.Contains("baud=9600") &&
                    content.Contains("router=1") &&
                    content.Contains("mcs_index=3") &&
                    content.Contains("aggregate=10") &&
                    content.Contains("channels=2")
                )
            ),
            Times.Once
        );

        _mockSshClientService.Verify(
            service => service.ExecuteCommandAsync(
                It.IsAny<DeviceConfig>(),
                DeviceCommands.TelemetryRestartCommand
            ),
            Times.Once
        );
    }

    [Test]
    public async Task UploadLatestVtxMenu_ExecutesCommandsAndUploadsFiles()
    {
        // Arrange
        var vtxmenuIni = "vtxmenu.ini";

        _mockSshClientService
            .Setup(service => service.UploadBinaryAsync(It.IsAny<DeviceConfig>(), "/etc", vtxmenuIni ))
            .Returns(Task.CompletedTask);

        // Act
        await Task.Run(() => _viewModel.UploadLatestVtxMenuCommand.Execute(null));

        // Assert
        _mockSshClientService.Verify(
            service => service.UploadBinaryAsync(It.IsAny<DeviceConfig>(), "/etc", vtxmenuIni),
            Times.Once
        );
    }

    [Test]
    public void EnableUART0Command_ExecutesUART0OnCommand()
    {
        // Act
        _viewModel.EnableUART0Command.Execute(null);

        // Assert
        _mockSshClientService.Verify(
            service => service.ExecuteCommandAsync(
                It.IsAny<DeviceConfig>(),
                DeviceCommands.UART0OnCommand
            ),
            Times.Once
        );
    }

    [Test]
    public void DisableUART0Command_ExecutesUART0OffCommand()
    {
        // Act
        _viewModel.DisableUART0Command.Execute(null);

        // Assert
        _mockSshClientService.Verify(
            service => service.ExecuteCommandAsync(
                It.IsAny<DeviceConfig>(),
                DeviceCommands.UART0OffCommand
            ),
            Times.Once
        );
    }

    [Test]
    public void OnBoardRecCommand_ExecutesOnAndOffCommands()
    {
        // Arrange
        _viewModel.IsOnboardRecOn = true;

        // Act
        _viewModel.OnBoardRecCommand.Execute(null);

        // Assert
        _mockSshClientService.Verify(
            service => service.ExecuteCommandAsync(
                It.IsAny<DeviceConfig>(),
                "yaml-cli .records.enabled true"
            ),
            Times.Once
        );

        // Arrange
        _viewModel.IsOnboardRecOn = false;
        _viewModel.IsOnboardRecOff = true;

        // Act
        _viewModel.OnBoardRecCommand.Execute(null);

        // Assert
        _mockSshClientService.Verify(
            service => service.ExecuteCommandAsync(
                It.IsAny<DeviceConfig>(),
                "yaml-cli .records.enabled false"
            ),
            Times.Once
        );
    }

    [Test]
    public void AddMavlinkCommand_ExecutesCommands()
    {
        // Act
        _viewModel.AddMavlinkCommand.Execute(null);

        // Assert
        _mockSshClientService.Verify(
            service => service.ExecuteCommandAsync(
                It.IsAny<DeviceConfig>(),
                TelemetryCommands.Extra
            ),
            Times.Once
        );

        _mockSshClientService.Verify(
            service => service.ExecuteCommandAsync(
                It.IsAny<DeviceConfig>(),
                DeviceCommands.RebootCommand
            ),
            Times.Once
        );
    }
    
    

}