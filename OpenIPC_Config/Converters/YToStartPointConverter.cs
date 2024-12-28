using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace OpenIPC_Config.Converters;

public class YToStartPointConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double y)
        {
            return new Point(0, y); // Start at the left of the canvas
        }
        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}