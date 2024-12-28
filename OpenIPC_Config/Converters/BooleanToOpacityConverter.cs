using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OpenIPC_Config.Converters;

public class BooleanToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            Console.WriteLine($"BooleanToOpacityConverter.Convert: {b}");
            return b ? 1.0 : 0.0;
        }
        return 0.0; // Default to invisible if value is not boolean
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}