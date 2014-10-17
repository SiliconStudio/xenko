// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Linq;

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
            var query = Enum.GetValues(enumType).Cast<object>();

            if (enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
            {
                object enumZero = GetEnumZero(enumType);
                if (Enum.IsDefined(enumType, enumZero))
                {
                    query = query.Where(x => Equals(x, enumZero) == false);
                }
            }

            return query.Distinct().ToArray();
        }

        private static object GetEnumZero(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException("enumType");

            Type underlyingType = Enum.GetUnderlyingType(enumType);

            if (underlyingType == null)
                throw new ArgumentNullException("enumType");

            if (underlyingType == typeof(byte))
                return Enum.ToObject(enumType, (byte)0);
            if (underlyingType == typeof(sbyte))
                return Enum.ToObject(enumType, (sbyte)0);
            if (underlyingType == typeof(short))
                return Enum.ToObject(enumType, (short)0);
            if (underlyingType == typeof(ushort))
                return Enum.ToObject(enumType, (ushort)0);
            if (underlyingType == typeof(int))
                return Enum.ToObject(enumType, 0);
            if (underlyingType == typeof(uint))
                return Enum.ToObject(enumType, (uint)0);
            if (underlyingType == typeof(long))
                return Enum.ToObject(enumType, (long)0);
            if (underlyingType == typeof(ulong))
                return Enum.ToObject(enumType, (ulong)0);

            throw new ArgumentException(string.Format("Unknown enum underlying type '{0}'", underlyingType.FullName));
        }
    }
}
