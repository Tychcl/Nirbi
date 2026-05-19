using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;

namespace nearby.Classes.Interface.Converters;

public class MessageCornerConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool v )
        {
            return new RoundRectangle { CornerRadius = v ? new CornerRadius(15, 15, 15, 0) : new CornerRadius(15, 15, 0, 15) };
        }
        return new RoundRectangle { CornerRadius = new CornerRadius(15, 15, 0, 15) };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}