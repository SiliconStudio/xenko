// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Translation.Presentation.ValueConverters
{
    public class Translate : LocalizableConverter<Translate>
    {
        public string Context { get; set; }

        /// <inheritdoc />
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString();
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return string.IsNullOrEmpty(Context)
                ? TranslationManager.Instance.GetString(text, Assembly)
                : TranslationManager.Instance.GetParticularString(Context, text, Assembly);
        }
    }
}
