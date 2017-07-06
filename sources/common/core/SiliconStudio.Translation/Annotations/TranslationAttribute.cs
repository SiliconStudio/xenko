// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Translation.Annotations
{
    /// <summary>
    /// Specifies a translatable name, with support for context and plural.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum |
                    AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public class TranslationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationAttribute"/> class.
        /// </summary>
        public TranslationAttribute([NotNull] string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationAttribute"/> class.
        /// </summary>
        public TranslationAttribute([NotNull] string text, [NotNull] string textPlural)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            TextPlural = textPlural ?? throw new ArgumentNullException(nameof(textPlural));
        }

        public string Context { get; set; }

        [NotNull]
        public string Text { get; }

        public string TextPlural { get; }
    }
}
