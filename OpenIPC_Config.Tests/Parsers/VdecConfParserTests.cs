using OpenIPC_Config.Parsers;

namespace OpenIPC_Config.Tests.Parsers;

[TestFixture]
public class VdecConfParserTests
{
    [Test]
    public void ReadConfig_ValidInput_ParsesCorrectly()
    {
        // Arrange
        string fileContents = @"
        ### Port for video rx (default: 5000)
        port=5600

        ### Codec: h264 or h265 (default: h264)
        codec=h265

        ### Incoming data format: stream or frame (default: stream)
        format=stream

        ### Screen output mode:
        mode=1080p60

        ### Mavlink port for telemetry (default: 14750)
        mavlink_port=16000

        ### Osd: none, simple or custom
        osd=none

        ### Extra param
        extra=""--bg-r 10 --bg-g 20 --bg-b 30""
        osd_elements=""-osd_ele1x 900 -osd_ele1y 350""
        ";

        // Act
        var config = VdecConfParser.ReadConfig(fileContents);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(5600, config.Port, "Port value mismatch.");
            Assert.AreEqual("h265", config.Codec, "Codec value mismatch.");
            Assert.AreEqual("stream", config.Format, "Format value mismatch.");
            Assert.AreEqual("1080p60", config.Mode, "Mode value mismatch.");
            Assert.AreEqual(16000, config.MavlinkPort, "MavlinkPort value mismatch.");
            Assert.AreEqual("none", config.Osd, "OSD value mismatch.");
            Assert.AreEqual("--bg-r 10 --bg-g 20 --bg-b 30", config.Extra, "Extra value mismatch.");
            Assert.AreEqual("-osd_ele1x 900 -osd_ele1y 350", config.OsdElements, "OSD Elements value mismatch.");
        });
    }

    [Test]
    public void ReadConfig_EmptyInput_ThrowsException()
    {
        // Arrange
        string fileContents = "";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => VdecConfParser.ReadConfig(fileContents));
        Assert.AreEqual("Input configuration content is empty.", ex.Message);
    }

    [Test]
    public void ReadConfig_InvalidLines_SkipsGracefully()
    {
        // Arrange
        string fileContents = @"
        ### Invalid lines and comments
        invalid-line-no-separator
        # Another comment
        port=5600
        ";

        // Act
        var config = VdecConfParser.ReadConfig(fileContents);

        // Assert
        Assert.AreEqual(5600, config.Port, "Port value mismatch.");
    }

    [Test]
    public void ReadConfig_MissingFields_UsesDefaultValues()
    {
        // Arrange
        string fileContents = @"
        ### Only port is specified
        port=5700
        ";

        // Act
        var config = VdecConfParser.ReadConfig(fileContents);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(5700, config.Port, "Port value mismatch.");
            Assert.AreEqual("h264", config.Codec, "Default codec value mismatch.");
            Assert.AreEqual("stream", config.Format, "Default format value mismatch.");
            Assert.AreEqual("720p60", config.Mode, "Default mode value mismatch.");
            Assert.AreEqual(14750, config.MavlinkPort, "Default mavlink port value mismatch.");
            Assert.AreEqual("simple", config.Osd, "Default OSD value mismatch.");
            Assert.AreEqual(string.Empty, config.Records, "Default records value mismatch.");
            Assert.AreEqual(string.Empty, config.Extra, "Default extra value mismatch.");
            Assert.AreEqual(string.Empty, config.OsdElements, "Default OSD elements value mismatch.");
        });
    }

    [Test]
    public void GenerateConfig_ReturnsCorrectString()
    {
        // Arrange
        var config = new VdecConfParser
        {
            Port = 5600,
            Codec = "h265",
            Format = "stream",
            Mode = "1080p60",
            MavlinkPort = 16000,
            Osd = "none",
            Extra = "--bg-r 10 --bg-g 20 --bg-b 30",
            OsdElements = "-osd_ele1x 900 -osd_ele1y 350"
        };

        // Act
        var generatedConfig = config.GenerateConfig();

        // Assert
        StringAssert.Contains("port=5600", generatedConfig);
        StringAssert.Contains("codec=h265", generatedConfig);
        StringAssert.Contains("format=stream", generatedConfig);
        StringAssert.Contains("mode=1080p60", generatedConfig);
        StringAssert.Contains("mavlink_port=16000", generatedConfig);
        StringAssert.Contains("osd=none", generatedConfig);
        StringAssert.Contains("extra=\"--bg-r 10 --bg-g 20 --bg-b 30\"", generatedConfig);
        StringAssert.Contains("osd_elements=\"-osd_ele1x 900 -osd_ele1y 350\"", generatedConfig);
    }
}