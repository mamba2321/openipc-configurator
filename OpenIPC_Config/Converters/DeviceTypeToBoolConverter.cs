using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Serilog;

namespace OpenIPC_Config.Converters;

public class DeviceTypeToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //throw new NotImplementedException();
        Log.Error("Not implemented yet!");
        return null;
    }
}