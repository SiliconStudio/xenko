// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

using SiliconStudio.Core.IO;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert an <see cref="UFile"/> to a string representing the file name with its extension.
    /// </summary>
    public class UFileToFileNameWithExt : OneWayValueConverter<UFileToFileNameWithExt>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var ufile = (UFile)value;
            return ufile.GetFileName();
        }
    }
}