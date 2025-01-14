using System.Reflection;
using Moq;
using OpenIPC_Config.Services;

namespace OpenIPC_Config.Tests.Services;


[TestFixture]
public class VersionHelperTests
{
    private Mock<IFileSystem> _mockFileSystem;

    [SetUp]
    public void SetUp()
    {
        _mockFileSystem = new Mock<IFileSystem>();
        VersionHelper.SetFileSystem(_mockFileSystem.Object);
    }

    [TearDown]
    public void TearDown()
    {
        // Reset the file system to the default implementation
        VersionHelper.SetFileSystem(new FileSystem());
    }

    [Test]
    public void GetAppVersion_ReturnsVersionFromFile_InDevelopmentEnvironment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var expectedVersion = "1.0.0-test";

        _mockFileSystem.Setup(fs => fs.Exists(It.IsAny<string>())).Returns(true);
        _mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Returns(expectedVersion);

        // Act
        var version = VersionHelper.GetAppVersion();

        // Assert
        Assert.AreEqual(expectedVersion, version);
    }

    

    [Test]
    public void GetAppVersion_ReturnsUnknownVersion_OnException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        _mockFileSystem.Setup(fs => fs.Exists(It.IsAny<string>())).Throws(new Exception("Test exception"));

        // Act
        var version = VersionHelper.GetAppVersion();

        // Assert
        Assert.AreEqual("Unknown Version", version);
    }
}