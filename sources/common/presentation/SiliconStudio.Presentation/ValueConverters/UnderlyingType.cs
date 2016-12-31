// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter convert a <see cref="Nullable"/> type to its underlying type. If the type is not nullable, it returns the type itself.
    /// </summary>
    public class UnderlyingType : OneWayValueConverter<UnderlyingType>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = value as Type;
            if (type == null)
            {
                throw new ArgumentException("The object passed to this value converter is not a type.");
            }
            return Nullable.GetUnderlyingType(type) ?? value;
        }
    }
}