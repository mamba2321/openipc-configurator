using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace OpenIPC_Config.Converters;

public class XToStartPointConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double x)
        {
            return new Point(x, 0); // Start at the top of the canvas
        }
        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}