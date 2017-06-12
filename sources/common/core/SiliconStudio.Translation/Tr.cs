// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Reflection;
using System.Runtime.CompilerServices;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Translation
{
    // ReSharper disable InconsistentNaming
    public static class Tr
    {
        /// <inheritdoc cref="ITranslationProvider.GetString"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NotNull]
        public static string _([NotNull] string text)
        {
            return TranslationManager.Instance.GetString(text, Assembly.GetCallingAssembly());
        }

        /// <inheritdoc cref="ITranslationProvider.GetPluralString"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NotNull]
        public static string _n([NotNull] string text, string pluralText, long n)
        {
            return TranslationManager.Instance.GetPluralString(text, pluralText, n, Assembly.GetCallingAssembly());
        }

        /// <inheritdoc cref="ITranslationProvider.GetParticularString"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NotNull]
        public static string _p(string context, [NotNull] string text)
        {
            return TranslationManager.Instance.GetParticularString(context, text, Assembly.GetCallingAssembly());
        }

        /// <inheritdoc cref="ITranslationProvider.GetParticularPluralString"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NotNull]
        public static string _pn(string context, [NotNull] string text, string textPlural, long n)
        {
            return TranslationManager.Instance.GetParticularPluralString(context, text, textPlural, n, Assembly.GetCallingAssembly());
        }
    }
}
