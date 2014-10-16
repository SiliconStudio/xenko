// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    /// <summary>
    /// A converter that returns value[0] when value[1], ..., value[n] equals each other, and null otherwise
    /// </summary>
    class FirstValueIfNextValueEqualsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = values[0];
            for (int i = 1; i < values.Length - 1; ++i)
            {
                if (!values[i].Equals(values[i+1]))
                    return null;
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
