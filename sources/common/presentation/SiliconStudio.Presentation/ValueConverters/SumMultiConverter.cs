using System;
using System.Globalization;
using System.Linq;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class SumMultiConverter : OneWayMultiValueConverter<SumMultiConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                throw new InvalidOperationException("This multi converter must be invoked with at least two elements");

            var result = 1.0;
            try
            {
                result = values.Select(x => System.Convert.ToDouble(x, culture)).Aggregate(result, (current, next) => current + next);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("The values of this converter must be convertible to a double.", exception);
            }

            return result;
        }
    }
}
