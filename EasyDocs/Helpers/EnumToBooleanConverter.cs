using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace EasyDocs.Helpers
{
    /// <summary>
    /// The Enum To Boolean Converter allows enum types to be converted to boolean types through the explicit parameters.
    /// For an example have a look in SettingsWindowView.axaml at the radio buttons, and how whether they are checked is converted to a Settings Tab Enum.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null && value.Equals(parameter);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value == null || parameter == null) { return BindingOperations.DoNothing; }
            return (bool)value ? parameter : BindingOperations.DoNothing;
        }
    }
}