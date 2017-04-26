// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Windows.Input;
using System.Windows.Markup;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    /// <summary>
    /// This markup extension allows to create a <see cref="Key"/> instance from a string representing the key.
    /// </summary>
    public class KeyExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public Key Key { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExtension"/> class with a string representing the key.
        /// </summary>
        /// <param name="key">A string representing the key.</param>
        public KeyExtension([NotNull] string key)
        {
            Key = (Key)Enum.Parse(typeof(Key), key, true);
        }

        /// <inheritdoc/>
        [NotNull]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Key;
        }
    }
}
