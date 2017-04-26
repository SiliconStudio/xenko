// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;
using System.Windows;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// A converter that resolve the specified value from the resources from the current application
    /// </summary>
    public class StaticResourceConverter : OneWayValueConverter<StaticResourceConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Application.Current.TryFindResource(value) ?? DependencyProperty.UnsetValue;
        }
    }
}
