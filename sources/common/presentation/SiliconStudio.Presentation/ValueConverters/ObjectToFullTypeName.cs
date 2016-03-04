// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter converts any object to a string representing the full name of its type. It accepts null and will convert it to a string representation of null.
    /// <see cref="ConvertBack"/>is supported, and will return the type corresponding to the given type name, or null if the string representation of a null object is passed.
    /// </summary>
    /// <seealso cref="ObjectToTypeName"/>
    /// <seealso cref="ObjectToType"/>
    public class ObjectToFullTypeName : ValueConverterBase<ObjectToFullTypeName>
    {
        /// <summary>
        /// The string representation of the type of a null object
        /// </summary>
        public const string NullObjectType = "(None)";

        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? NullObjectType : value.GetType().FullName;
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var typeName = value.ToString();
            return typeName == NullObjectType ? null : AssemblyRegistry.GetType(typeName);
        }
    }
}
