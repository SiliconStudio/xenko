using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using SiliconStudio.BuildEngine.Editor.ViewModel;

namespace SiliconStudio.BuildEngine.Editor.View
{
    public class BoolToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var propertyName = (string)values[0];
            var value = (bool)values[1];

            switch (propertyName)
            {
                case BuildSettingsPropertiesEnumerator.IsMetadataDatabaseOpenedPropertyName:
                    return value ? "Metadata database found" : "Metadata database not found";
            }

            return value.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
