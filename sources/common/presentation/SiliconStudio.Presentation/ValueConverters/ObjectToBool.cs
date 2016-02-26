// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

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
            return value != null;
        }
    }
}
