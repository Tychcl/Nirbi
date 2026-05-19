using System.Globalization;

namespace nearby.Classes.Interface.Converters;

public class DoubleToRectConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double s = 70;
        if (value is int i) s = i;
        else if (value is double d) s = d;
        return new Rect(0, 0, s, s);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}