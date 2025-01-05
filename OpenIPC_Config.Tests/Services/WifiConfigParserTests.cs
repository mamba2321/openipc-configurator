using Moq;
using OpenIPC_Config.Parsers;
using OpenIPC_Config.Services;
using Serilog;

namespace OpenIPC_Config.Tests.Services;

    [TestFixture]
    public class WifiConfigParserTests
    {
        private Mock<ILogger> _mockLogger;
        private WifiConfigParser _wifiConfigParser;

        [SetUp]
        public void SetUp()
        {
            // Mock the logger
            _mockLogger = new Mock<ILogger>();
            Log.Logger = _mockLogger.Object;

            // Initialize WifiConfigParser
            _wifiConfigParser = new WifiConfigParser();
        }

        [Test]
        public void ParseConfigString_ValidConfig_SetsProperties()
        {
            // Arrange
            var configContent = """
                wifi_channel = 6
                wifi_region = 'US'

                [gs_mavlink]
                peer = '192.168.0.2'

                [gs_video]
                peer = '192.168.0.3'
            """;

            // Act
            _wifiConfigParser.ParseConfigString(configContent);

            // Assert
            Assert.AreEqual(6, _wifiConfigParser.WifiChannel);
            Assert.AreEqual("US", _wifiConfigParser.WifiRegion);
            Assert.AreEqual("192.168.0.2", _wifiConfigParser.GsMavlinkPeer);
            Assert.AreEqual("192.168.0.3", _wifiConfigParser.GsVideoPeer);
        }
        
        [Test]
        public void GetUpdatedConfigString_ValidUpdates_ReturnsUpdatedConfig()
        {
            // Arrange
            var configContent = """
                wifi_channel = 6
                wifi_region = 'US'

                [gs_mavlink]
                peer = '192.168.0.2'

                [gs_video]
                peer = '192.168.0.3'
            """;
            _wifiConfigParser.ParseConfigString(configContent);

            // Update properties
            _wifiConfigParser.WifiChannel = 11;
            _wifiConfigParser.WifiRegion = "EU";
            _wifiConfigParser.GsMavlinkPeer = "192.168.1.1";
            _wifiConfigParser.GsVideoPeer = "192.168.1.2";

            // Act
            var updatedConfig = _wifiConfigParser.GetUpdatedConfigString();

            // Assert
            StringAssert.Contains("wifi_channel = 11", updatedConfig);
            StringAssert.Contains("wifi_region = 'EU'", updatedConfig);
            StringAssert.Contains("peer = '192.168.1.1'", updatedConfig);
            StringAssert.Contains("peer = '192.168.1.2'", updatedConfig);
        }
        

        [Test]
        public void ParseConfigString_InvalidLine_IgnoresLine()
        {
            // Arrange
            var configContent = """
                wifi_channel = 6
                invalid_line_without_equals
                wifi_region = 'US'
            """;

            // Act
            _wifiConfigParser.ParseConfigString(configContent);

            // Assert
            Assert.AreEqual(6, _wifiConfigParser.WifiChannel);
            Assert.AreEqual("US", _wifiConfigParser.WifiRegion);
        }
    }