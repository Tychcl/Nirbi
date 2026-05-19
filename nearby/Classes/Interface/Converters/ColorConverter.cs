using System.Globalization;
using nearby.ViewModels;

namespace nearby.Classes.Interface.Converters;

public class ColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TaskCategory vtc && parameter is TaskCategory ptc && vtc == ptc)
        {
            return ResourceManager.Get("CPrimary");
        }
        else
        {
            return ResourceManager.Get("CBorder");
        }
    }
        

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}