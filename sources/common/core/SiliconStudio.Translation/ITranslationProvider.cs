// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Translation
{
    public interface ITranslationProvider
    {
        [NotNull]
        string BaseName { get; }

        /// <summary>
        /// Gets the translation of <paramref name="text"/> in the current culture.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <returns>The translation of <paramref name="text"/> in the current culture; or <paramref name="text"/> if no translation is found.</returns>
        [NotNull]
        string GetString([NotNull] string text);

        /// <summary>
        /// Gets the translation of <paramref name="text"/> and/or <paramref name="textPlural"/> in the current culture,
        /// choosing the right plural form depending on the <paramref name="count"/>.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <param name="textPlural">The plural version of the text to translate.</param>
        /// <param name="count">The count used to determine the right plural form</param>
        /// <returns>The translation of <paramref name="text"/> in the current culture; or <paramref name="text"/> if no translation is found.</returns>
        [NotNull]
        string GetPluralString([NotNull] string text, string textPlural, long count);

        /// <summary>
        /// Gets the translation of <paramref name="text"/> in the provided <paramref name="context"/> in the current culture.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <param name="context">The particular context for the translation.</param>
        /// <returns>The translation of <paramref name="text"/> in the current culture; or <paramref name="text"/> if no translation is found.</returns>
        [NotNull]
        string GetParticularString([NotNull] string text, string context);

        /// <summary>
        /// Gets the translation of <paramref name="text"/> and/or <paramref name="textPlural"/> in the provided <paramref name="context"/> in the current culture,
        /// choosing the right plural form depending on the <paramref name="count"/>.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <param name="textPlural">The plural version of the text to translate.</param>
        /// <param name="count">The count used to determine the right plural form</param>
        /// <param name="context">The particular context for the translation.</param>
        /// <returns>The translation of <paramref name="text"/> in the current culture; or <paramref name="text"/> if no translation is found.</returns>
        [NotNull]
        string GetParticularPluralString([NotNull] string text, string textPlural, long count, string context);
    }
}
