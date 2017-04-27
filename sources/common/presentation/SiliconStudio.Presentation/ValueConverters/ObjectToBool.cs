// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;
using SiliconStudio.Presentation.Internal;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert an object to a boolean value, returning <c>false</c> if the object is equal to null, <c>true</c> otherwise.
    /// </summary>
    /// <remarks>Value types are always non-null and therefore always returns true.</remarks>
    public class ObjectToBool : OneWayValueConverter<ObjectToBool>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value != null;
            return result.Box();
        }
    }
}
