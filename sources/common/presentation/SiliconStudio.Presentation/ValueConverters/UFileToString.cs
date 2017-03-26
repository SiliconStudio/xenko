// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert an <see cref="UFile"/> object to its string representation. <see cref="ConvertBack"/> is supported.
    /// </summary>
    /// <seealso cref="UDirectoryToString"/>
    public class UFileToString : ValueConverterBase<UFileToString>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().Replace("/", "\\");
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            try
            {
                return new UFile((string)value);
            }
            catch
            {
                return new UFile("");
            }
        }
    }
}
