// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert an <see cref="UFile"/> to its string representation.
    /// </summary>
    public class UFileToString : ValueConverterBase<UFileToString>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ufile = (UFile)value;
            return ufile.ToString().Replace('/', '\\');
        }

        [NotNull]
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (string)value;
            return new UFile(str);
        }
    }
}