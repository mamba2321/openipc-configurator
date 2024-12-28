using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace OpenIPC_Config.Converters;

public class YToEndPointConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double y)
        {
            return new Point(731, y); // End at the right of the canvas
        }
        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}