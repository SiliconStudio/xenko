// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter convert any object to its type. It accepts null and will return null in this case.
    /// </summary>
    /// <seealso cref="ObjectToFullTypeName"/>
    /// <seealso cref="ObjectToTypeName"/>
    public class ObjectToType : OneWayValueConverter<ObjectToType>
    {
        /// <summary>
        /// The string representation of the type of a null object
        /// </summary>
        public const string NullObjectType = "(None)";

        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType();
        }
    }
}
