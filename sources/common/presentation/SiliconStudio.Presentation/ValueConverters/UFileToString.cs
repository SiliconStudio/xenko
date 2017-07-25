// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;
using System.IO;
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
            return value?.ToString().Replace('/', Path.DirectorySeparatorChar);
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
