using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.BuildEngine.Editor.View
{
    [ValueConversion(typeof(IEnumerable), typeof(string))]
    public class EnumerableToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumerable = (IEnumerable<IContent>)value;
            if (!enumerable.Any())
                return "<Empty ruleset>";
            string result = string.Join(Environment.NewLine, enumerable.Select(x => string.IsNullOrEmpty((string)x.Value) ? "<Empty rule>" : x.Value)).Trim();
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
