using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace System.Windows.Core
{
    public class DummyConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, Globalization.CultureInfo culture)
        {
            Debug.WriteLine("DummyConverter->Convert: " + value);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
