// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// An abstract class for implementations of <see cref="IValueConverter"/> that supports markup extensions.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IValueConverter"/> being implemented.</typeparam>
    public abstract class ValueConverterBase<T> : MarkupExtension, IValueConverter where T : class, IValueConverter, new()
    {
        private static T valueConverterInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueConverterBase{T}"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">The generic argument does not match the type of the implementation of this class.</exception>
        protected ValueConverterBase()
        {
            if (GetType() != typeof(T)) throw new InvalidOperationException("The generic argument of this class must be the type being implemented.");
        }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return valueConverterInstance ?? (valueConverterInstance = new T());
        }

        /// <inheritdoc/>
        public abstract object Convert(object value, [NotNull] Type targetType, object parameter, CultureInfo culture);

        /// <inheritdoc/>
        public abstract object ConvertBack(object value, [NotNull] Type targetType, object parameter, CultureInfo culture);
    }
}
