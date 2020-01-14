using System;
using System.Globalization;
using System.Windows.Data;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class EnumComboBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // select button if the property value matches the static parameter
            // Debugger.Break();
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // send the static parameter if the button is selected
            // Debugger.Break();
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
