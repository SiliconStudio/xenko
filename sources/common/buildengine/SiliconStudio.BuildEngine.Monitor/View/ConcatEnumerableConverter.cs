// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    class ConcatEnumerableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<object> result = Enumerable.Empty<object>();
            foreach (var value in values)
            {
                var enumerable = value as IEnumerable;
                if (enumerable != null)
                    result = result.Concat(enumerable.Cast<object>());
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
