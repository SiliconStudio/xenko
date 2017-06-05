// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Translation
{
    public static class TranslationManager
    {
        private static readonly Lazy<ITranslationManager> Lazy = new Lazy<ITranslationManager>(() => new TranslationManagerImpl());

        /// <summary>
        /// Gets the instance of the <see cref="ITranslationManager"/>.
        /// </summary>
        public static ITranslationManager Instance => Lazy.Value;

        /// <summary>
        /// Implementation of <see cref="ITranslationManager"/>.
        /// </summary>
        private sealed class TranslationManagerImpl : ITranslationManager
        {
            private readonly Dictionary<string, ITranslationProvider> translationProviders = new Dictionary<string, ITranslationProvider>();

            /// <inheritdoc />
            public CultureInfo CurrentLanguage
            {
                get => CultureInfo.CurrentUICulture;
                set
                {
                    if (Equals(CultureInfo.CurrentUICulture, value))
                        return;

                    CultureInfo.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture = value;
                    OnLanguageChanged();
                }
            }

            /// <inheritdoc />
            public event EventHandler LanguageChanged;

            /// <inheritdoc />
            string ITranslationProvider.BaseName => nameof(TranslationManager);

            /// <inheritdoc />
            public string GetString(string text)
            {
                return GetString(text, Assembly.GetCallingAssembly());
            }

            /// <inheritdoc />
            public string GetString(string text, [NotNull] Assembly assembly)
            {
                if (assembly == null) throw new ArgumentNullException(nameof(assembly));
                return GetProvider(assembly)?.GetString(text) ?? text;
            }

            /// <inheritdoc />
            public string GetPluralString(string text, string textPlural, long count)
            {
                return GetPluralString(text, textPlural, count, Assembly.GetCallingAssembly());
            }

            /// <inheritdoc />
            public string GetPluralString(string text, string textPlural, long count, [NotNull] Assembly assembly)
            {
                if (assembly == null) throw new ArgumentNullException(nameof(assembly));
                return GetProvider(assembly)?.GetPluralString(text, textPlural, count) ?? text;
            }

            /// <inheritdoc />
            public string GetParticularString(string text, string context)
            {
                return GetParticularString(text, context, Assembly.GetCallingAssembly());
            }

            /// <inheritdoc />
            public string GetParticularString(string text, string context, [NotNull] Assembly assembly)
            {
                if (assembly == null) throw new ArgumentNullException(nameof(assembly));
                return GetProvider(assembly)?.GetParticularString(text, context) ?? text;
            }

            /// <inheritdoc />
            public string GetParticularPluralString(string text, string context, string textPlural, long count)
            {
                return GetParticularPluralString(text, context, textPlural, count, Assembly.GetCallingAssembly());
            }

            /// <inheritdoc />
            public string GetParticularPluralString(string text, string context, string textPlural, long count, [NotNull] Assembly assembly)
            {
                if (assembly == null) throw new ArgumentNullException(nameof(assembly));
                return GetProvider(assembly)?.GetParticularPluralString(text, context, textPlural, count) ?? text;
            }

            /// <inheritdoc />
            public void RegisterProvider(ITranslationProvider provider)
            {
                if (provider == null) throw new ArgumentNullException(nameof(provider));
                translationProviders.Add(provider.BaseName, provider);
            }

            [CanBeNull]
            private ITranslationProvider GetProvider([NotNull] Assembly assembly)
            {
                translationProviders.TryGetValue(assembly.GetName().Name, out var provider);
                return provider;
            }

            private void OnLanguageChanged()
            {
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}