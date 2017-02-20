// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Runtime.CompilerServices;
using System.Windows;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// Helper class with similar methods than <see cref="Convert"/> but returns the default value of the expected type if value is <see cref="DependencyProperty.UnsetValue"/>.
    /// </summary>
    internal static class ConverterHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToBoolean(object value, IFormatProvider culture)
        {
            return value != DependencyProperty.UnsetValue && Convert.ToBoolean(value, culture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ConvertToChar(object value, IFormatProvider culture)
        {
            return value != DependencyProperty.UnsetValue ? Convert.ToChar(value, culture) : default(char);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ConvertToDouble(object value, IFormatProvider culture)
        {
            return value != DependencyProperty.UnsetValue ? Convert.ToDouble(value, culture) : default(double);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ConvertToInt32(object value, IFormatProvider culture)
        {
            return value != DependencyProperty.UnsetValue ? Convert.ToInt32(value, culture) : default(int);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ConvertToString(object value, IFormatProvider culture)
        {
            return value != DependencyProperty.UnsetValue ? Convert.ToString(value, culture) : default(string);
        }
    }
}
