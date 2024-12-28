using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Parsers;

public class OsdElementsParser
{
    private const string ElementPattern = @"-osd_ele(?<index>\d+)(?<axis>[xy])\s+(?<value>\d+)";
    
    public static ObservableCollection<OverlayItem> Parse(string osdElements, ObservableCollection<OverlayItem> overlayItems)
    {
        if (string.IsNullOrWhiteSpace(osdElements))
            throw new ArgumentException("osd_elements string cannot be empty or null.");

        // Dictionary to store parsed coordinates
        var parsedElements = new Dictionary<int, (double? X, double? Y)>();

        // Parse osd_elements using regex
        var matches = Regex.Matches(osdElements, ElementPattern);
        foreach (Match match in matches)
        {
            var index = int.Parse(match.Groups["index"].Value);
            var axis = match.Groups["axis"].Value;
            var value = double.Parse(match.Groups["value"].Value);

            if (!parsedElements.ContainsKey(index))
                parsedElements[index] = (null, null);

            if (axis == "x")
                parsedElements[index] = (value, parsedElements[index].Y);
            else if (axis == "y")
                parsedElements[index] = (parsedElements[index].X, value);
        }

        // Map parsed elements to OverlayItems
        foreach (var kvp in parsedElements)
        {
            var index = kvp.Key - 1; // Convert 1-based index to 0-based index
            if (index >= 0 && index < overlayItems.Count)
            {
                var overlayItem = overlayItems[index];
                if (kvp.Value.X.HasValue) overlayItem.PositionX = kvp.Value.X.Value;
                if (kvp.Value.Y.HasValue) overlayItem.PositionY = kvp.Value.Y.Value;
            }
        }

        return overlayItems;
    }

    public static string GenerateOsdElements(ObservableCollection<OverlayItem> overlayItems)
    {
        var elements = new List<string>();

        for (int i = 0; i < overlayItems.Count; i++)
        {
            var overlayItem = overlayItems[i];
            elements.Add($"-osd_ele{i + 1}x {overlayItem.PositionX}");
            elements.Add($"-osd_ele{i + 1}y {overlayItem.PositionY}");
        }

        return string.Join(" ", elements);
    }
}