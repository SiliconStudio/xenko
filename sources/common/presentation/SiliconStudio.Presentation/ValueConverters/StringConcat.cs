// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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