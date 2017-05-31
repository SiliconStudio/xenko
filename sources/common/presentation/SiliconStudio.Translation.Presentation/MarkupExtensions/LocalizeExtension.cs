// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Windows.Markup;
using System.Xaml;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Translation.Presentation.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(string))]
    public class LocalizeExtension : MarkupExtension
    {
        public LocalizeExtension(object text)
        {
            Text = text?.ToString();
        }

        public string Context { get; set; }

        /// <seealso cref="Plural"/>
        public long Count { get; set; }

        /// <seealso cref="Count"/>
        public string Plural { get; set; }

        [ConstructorArgument("text")]
        public string Text { get; set; }

        /// <inheritdoc />
        [NotNull]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Text))
                return string.Empty;

            var rootProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));
            var assembly = rootProvider.RootObject.GetType().Assembly;
            return string.IsNullOrEmpty(Context)
                ? (string.IsNullOrEmpty(Plural) ? TranslationManager.Instance.GetString(Text, assembly) : TranslationManager.Instance.GetPluralString(Text, Plural, Count, assembly))
                : (string.IsNullOrEmpty(Plural) ? TranslationManager.Instance.GetParticularString(Text, Context, assembly) : TranslationManager.Instance.GetParticularPluralString(Text, Plural, Count, Context, assembly));
        }
    }
}
