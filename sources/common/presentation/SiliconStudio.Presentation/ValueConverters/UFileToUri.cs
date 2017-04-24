// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
