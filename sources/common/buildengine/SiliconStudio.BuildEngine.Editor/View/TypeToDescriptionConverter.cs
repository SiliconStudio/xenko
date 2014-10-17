using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SiliconStudio.BuildEngine.Editor.View
{
    public class TypeToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (Type)value;
            var description = (DescriptionAttribute)type.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
            return description != null ? description.Description : type.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
