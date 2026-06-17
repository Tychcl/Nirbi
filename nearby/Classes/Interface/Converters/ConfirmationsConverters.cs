using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using nearby.ViewModels;
namespace nearby.Classes.Interface.Converters
{
    public class TabBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && parameter is string param)
            {
                bool expected = param == "true";
                return isSelected == expected ? ResourceManager.Get("CPrimary") : Colors.Transparent;
            }
            return Colors.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class TabTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && parameter is string param)
            {
                bool expected = param == "true";
                return isSelected == expected ? Colors.White : Application.Current!.Resources["CTextSecondary"];
            }
            return Application.Current!.Resources["CTextSecondary"];
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BoolToCountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is bool isReceived && values[1] is int count)
            {
                return isReceived ? $"Получено: {count}" : $"Отправлено: {count}";
            }
            return "0";
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ConfirmationListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ConfirmationsViewModel vm)
                return vm.IsReceivedSelected ? vm.ReceivedConfirmations : vm.SentConfirmations;
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
