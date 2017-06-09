// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;
using System.Reflection;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Translation
{
    public interface ITranslationManager : ITranslationProvider
    {
        /// <summary>
        /// Gets or sets the current culture used by this Translation Manager to look up culture-specific resources at run time.
        /// </summary>
        [NotNull]
        CultureInfo CurrentLanguage { get; set; }

        event EventHandler LanguageChanged;

        /// <summary>
        /// Gets the translation of <paramref name="text"/> in the current culture.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <param name="assembly">The main assembly to lookup the translation.</param>
        /// <returns>The translation of <paramref name="text"/> in the current culture; or <paramref name="text"/> if no translation is found.</returns>
        [NotNull]
        string GetString([NotNull] string text, Assembly assembly);

        /// <summary>
        /// Gets the translation of <paramref name="text"/> and/or <paramref name="textPlural"/> in the current culture,
        /// choosing the right plural form depending on the <paramref name="count"/>.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <param name="textPlural">The plural version of the text to translate.</param>
        /// <param name="count">The count used to determine the right plural form</param>
        /// <param name="assembly">The main assembly to lookup the translation.</param>
        /// <returns>The translation of <paramref name="text"/> in the current culture; or <paramref name="text"/> if no translation is found.</returns>
        [NotNull]
        string GetPluralString([NotNull] string text, string textPlural, long count, Assembly assembly);

        /// <summary>
        /// Gets the translation of <paramref name="text"/> in the provided <paramref name="context"/> in the current culture.
        /// </summary>
        /// <param name="context">The particular context for the translation.</param>
        /// <param name="text">The text to translate.</param>
        /// <param name="assembly">The main assembly to lookup the translation.</param>
        /// <returns>The translation of <paramref name="text"/> in the current culture; or <paramref name="text"/> if no translation is found.</returns>
        [NotNull]
        string GetParticularString(string context, [NotNull] string text, Assembly assembly);

        /// <summary>
        /// Gets the translation of <paramref name="text"/> and/or <paramref name="textPlural"/> in the provided <paramref name="context"/> in the current culture,
        /// choosing the right plural form depending on the <paramref name="count"/>.
        /// </summary>
        /// <param name="context">The particular context for the translation.</param>
        /// <param name="text">The text to translate.</param>
        /// <param name="textPlural">The plural version of the text to translate.</param>
        /// <param name="count">The count used to determine the right plural form</param>
        /// <param name="assembly">The main assembly to lookup the translation.</param>
        /// <returns>The translation of <paramref name="text"/> in the current culture; or <paramref name="text"/> if no translation is found.</returns>
        [NotNull]
        string GetParticularPluralString(string context, [NotNull] string text, string textPlural, long count, Assembly assembly);

        void RegisterProvider([NotNull] ITranslationProvider provider);
    }
}