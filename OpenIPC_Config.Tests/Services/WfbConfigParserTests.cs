using Moq;
using OpenIPC_Config.Parsers;
using OpenIPC_Config.Services;
using Serilog;

namespace OpenIPC_Config.Tests.Services;

[TestFixture]
public class WfbConfigParserTests
{
    private Mock<ILogger> _mockLogger;
    private WfbConfigParser _wfbConfigParserH;

    [SetUp]
    public void SetUp()
    {
        // Mock the logger
        _mockLogger = new Mock<ILogger>();
        Log.Logger = _mockLogger.Object;

        // Initialize WfbConfigParser
        _wfbConfigParserH = new WfbConfigParser();
    }

    [Test]
    public void ParseConfigString_ValidConfig_SetsProperties()
    {
        // Arrange
        var configContent = """
            unit = 'test_unit'
            wlan = 'wlan0'
            region = 'US'
            channel = '6'
            txpower = 30
            driver_txpower_override = 1
            bandwidth = 20
            stbc = 1
            ldpc = 1
            mcs_index = 7
            stream = 2
            link_id = 12345
            udp_port = 14550
            rcv_buf = 1048576
            frame_type = 'data'
            fec_k = 10
            fec_n = 20
            pool_timeout = 100
            guard_interval = 'long'
        """;

        // Act
        _wfbConfigParserH.ParseConfigString(configContent);

        // Assert
        Assert.AreEqual("test_unit", _wfbConfigParserH.Unit);
        Assert.AreEqual("wlan0", _wfbConfigParserH.Wlan);
        Assert.AreEqual("US", _wfbConfigParserH.Region);
        Assert.AreEqual("6", _wfbConfigParserH.Channel);
        Assert.AreEqual(30, _wfbConfigParserH.TxPower);
        Assert.AreEqual(1, _wfbConfigParserH.DriverTxPowerOverride);
        Assert.AreEqual(20, _wfbConfigParserH.Bandwidth);
        Assert.AreEqual(1, _wfbConfigParserH.Stbc);
        Assert.AreEqual(1, _wfbConfigParserH.Ldpc);
        Assert.AreEqual(7, _wfbConfigParserH.McsIndex);
        Assert.AreEqual(2, _wfbConfigParserH.Stream);
        Assert.AreEqual(12345, _wfbConfigParserH.LinkId);
        Assert.AreEqual(14550, _wfbConfigParserH.UdpPort);
        Assert.AreEqual(1048576, _wfbConfigParserH.RcvBuf);
        Assert.AreEqual("data", _wfbConfigParserH.FrameType);
        Assert.AreEqual(10, _wfbConfigParserH.FecK);
        Assert.AreEqual(20, _wfbConfigParserH.FecN);
        Assert.AreEqual(100, _wfbConfigParserH.PoolTimeout);
        Assert.AreEqual("long", _wfbConfigParserH.GuardInterval);
    }
    
    [Test]
    public void ParseConfigString_InvalidLine_IgnoresLine()
    {
        // Arrange
        var configContent = """
            unit = 'test_unit'
            invalid_line_without_equals
            channel = '6'
        """;

        // Act
        _wfbConfigParserH.ParseConfigString(configContent);

        // Assert
        Assert.AreEqual("test_unit", _wfbConfigParserH.Unit);
        Assert.AreEqual("6", _wfbConfigParserH.Channel);
        
    }
}