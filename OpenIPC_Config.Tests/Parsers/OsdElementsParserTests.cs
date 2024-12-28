using System.Collections.ObjectModel;
using OpenIPC_Config.Parsers;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Tests.Parsers;

[TestFixture]
    public class OsdElementsParserTests
    {
        [Test]
        public void Parse_ValidOsdElements_MapsToOverlayItems()
        {
            // Arrange
            var osdElements = "-osd_ele1x 910 -osd_ele1y 350 -osd_ele2x 240 -osd_ele2y 350";
            var overlayItems = new ObservableCollection<OverlayItem>
            {
                new OverlayItem { Name = "ALT", PositionX = 0, PositionY = 0 },
                new OverlayItem { Name = "SPD", PositionX = 0, PositionY = 0 }
            };

            // Act
            var updatedOverlayItems = OsdElementsParser.Parse(osdElements, overlayItems);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.AreEqual(910, updatedOverlayItems[0].PositionX, "ALT PositionX mismatch");
                Assert.AreEqual(350, updatedOverlayItems[0].PositionY, "ALT PositionY mismatch");
                Assert.AreEqual(240, updatedOverlayItems[1].PositionX, "SPD PositionX mismatch");
                Assert.AreEqual(350, updatedOverlayItems[1].PositionY, "SPD PositionY mismatch");
            });
        }

        [Test]
        public void Parse_MissingElements_LeavesDefaultsUntouched()
        {
            // Arrange
            var osdElements = "-osd_ele1x 910 -osd_ele1y 350";
            var overlayItems = new ObservableCollection<OverlayItem>
            {
                new OverlayItem { Name = "ALT", PositionX = 0, PositionY = 0 },
                new OverlayItem { Name = "SPD", PositionX = 100, PositionY = 200 }
            };

            // Act
            var updatedOverlayItems = OsdElementsParser.Parse(osdElements, overlayItems);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.AreEqual(910, updatedOverlayItems[0].PositionX, "ALT PositionX mismatch");
                Assert.AreEqual(350, updatedOverlayItems[0].PositionY, "ALT PositionY mismatch");
                Assert.AreEqual(100, updatedOverlayItems[1].PositionX, "SPD PositionX mismatch");
                Assert.AreEqual(200, updatedOverlayItems[1].PositionY, "SPD PositionY mismatch");
            });
        }

        [Test]
        public void Parse_InvalidOsdElements_ThrowsException()
        {
            // Arrange
            var osdElements = "";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => OsdElementsParser.Parse(osdElements, new ObservableCollection<OverlayItem>()));
            Assert.AreEqual("osd_elements string cannot be empty or null.", ex.Message);
        }

        [Test]
        public void GenerateOsdElements_ValidOverlayItems_GeneratesStringCorrectly()
        {
            // Arrange
            var overlayItems = new ObservableCollection<OverlayItem>
            {
                new OverlayItem { Name = "ALT", PositionX = 910, PositionY = 350 },
                new OverlayItem { Name = "SPD", PositionX = 240, PositionY = 350 }
            };

            // Act
            var osdElements = OsdElementsParser.GenerateOsdElements(overlayItems);

            // Assert
            Assert.AreEqual("-osd_ele1x 910 -osd_ele1y 350 -osd_ele2x 240 -osd_ele2y 350", osdElements);
        }

        [Test]
        public void GenerateOsdElements_EmptyOverlayItems_ReturnsEmptyString()
        {
            // Arrange
            var overlayItems = new ObservableCollection<OverlayItem>();

            // Act
            var osdElements = OsdElementsParser.GenerateOsdElements(overlayItems);

            // Assert
            Assert.AreEqual(string.Empty, osdElements);
        }
    }