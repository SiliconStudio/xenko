// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert an <see cref="UPath"/> object to its string representation. <see cref="ConvertBack"/> is supported, the correct
    /// target type (UFile or UDirectory) must be passed.
    /// </summary>
    public class UPathToString : ValueConverterBase<UPathToString>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().Replace("/", "\\");
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, [NotNull] Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (targetType == typeof(UFile))
                    return value != null ? new UFile((string)value) : null;

                if (targetType == typeof(UDirectory))
                    return value != null ? new UDirectory((string)value) : null;
            }
            catch
            {
                if (targetType == typeof(UFile))
                    return value != null ? new UFile("") : null;

                if (targetType == typeof(UDirectory))
                    return value != null ? new UDirectory("") : null;
            }

            throw new ArgumentException(@"target type must be either UFile or UDirectory", nameof(targetType));
        }
    }
}
