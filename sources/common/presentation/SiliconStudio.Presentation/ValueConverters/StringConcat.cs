// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Globalization;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class StringConcat : OneWayValueConverter<StringConcat>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() + parameter.ToString();
        }
    }
}
