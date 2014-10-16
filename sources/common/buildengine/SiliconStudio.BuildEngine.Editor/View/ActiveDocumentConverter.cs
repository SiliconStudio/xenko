using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using SiliconStudio.BuildEngine.Editor.ViewModel;

namespace SiliconStudio.BuildEngine.Editor.View
{
    public class ActiveDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is BuildSessionViewModel)
                return value;

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is BuildSessionViewModel)
                return value;

            return Binding.DoNothing;
        }
    }
}
