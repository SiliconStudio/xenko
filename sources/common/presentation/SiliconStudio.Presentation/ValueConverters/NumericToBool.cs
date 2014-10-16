// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert a numerical value to a boolean. The result will be <c>false</c> if the given value is equal to zero, <c>true</c> otherwise.
    /// </summary>
    /// <remarks>Supported types are: <see cref="SByte"/>, <see cref="Int16"/>, <see cref="Int32"/>, <see cref="Int64"/>, <see cref="Byte"/>, <see cref="UInt16"/>, <see cref="UInt32"/>, <see cref="UInt64"/></remarks>
    public class NumericToBool : OneWayValueConverter<NumericToBool>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is sbyte) return ((sbyte)value) != 0;
            if (value is short) return ((short)value) != 0;
            if (value is int) return ((int)value) != 0;
            if (value is long) return ((long)value) != 0;
            if (value is byte) return ((byte)value) != 0;
            if (value is ushort) return ((ushort)value) != 0;
            if (value is uint) return ((uint)value) != 0;
            if (value is ulong) return ((ulong)value) != 0;
            throw new ArgumentException("value is not a numeric type");
        }
    }
}
