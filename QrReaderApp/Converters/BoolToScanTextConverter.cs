using System.Globalization;

namespace QrReaderApp.Converters;

public class BoolToScanTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isScanning)
        {
            return isScanning ? "Pause" : "Resume";
        }
        return "Scan";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
