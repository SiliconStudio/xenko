// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Translation.Presentation.ValueConverters
{
    public class Translate : MarkupExtension, IValueConverter
    {
        private static readonly Lazy<Translate> Instance = new Lazy<Translate>();

        public string Context { get; set; }

        /// <inheritdoc />
        [NotNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString();
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return string.IsNullOrEmpty(Context)
                ? TranslationManager.Instance.GetString(text)
                : TranslationManager.Instance.GetParticularString(text, Context);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"ConvertBack is not supported by this {nameof(IValueConverter)}.");
        }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance.Value;
        }
    }
}
