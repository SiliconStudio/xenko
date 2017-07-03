// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert a <see cref="Type"/> to an enumerable of <see cref="Enum"/> values, assuming the given type represents an enum or
    /// a nullable enum. Enums with <see cref="FlagsAttribute"/> are supported as well.
    /// </summary>
    public class EnumValues : OneWayValueConverter<EnumValues>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumType = value as Type;
            if (enumType == null)
                return null;

            if (!enumType.IsEnum)
            {
                enumType = Nullable.GetUnderlyingType(enumType);
                if (enumType == null || !enumType.IsEnum)
                    return null;
            }

            if (enumType.GetCustomAttribute<FlagsAttribute>(false) != null)
            {
                var query = EnumExtensions.GetIndividualFlags(enumType);
                return query;
            }
            else
            {
                var query = Enum.GetValues(enumType).Cast<object>().Distinct().ToList();
                return query;
            }
        }
    }
}
