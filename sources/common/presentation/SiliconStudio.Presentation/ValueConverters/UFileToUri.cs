// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

using SiliconStudio.Core.IO;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert an <see cref="UFile"/> to an instance of the <see cref="Uri"/> class.
    /// </summary>
    public class UFileToUri : OneWayValueConverter<UFileToUri>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            try
            {
                var uri = new Uri((UFile)value);
                return uri;
            }
            catch
            {
                return null;
            }
        }
    }
}