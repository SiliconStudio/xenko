// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Copyright (c) 2009 Svetoslav Savov
// http://svetoslavsavov.blogspot.jp/2009/07/switching-wpf-interface-themes-at.html

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Presentation.Legacy
{
    public class ThemeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the ResourceDictionaries removed from the application resources.
        /// </summary>
        public ThemeResourceDictionary[] OldResources { get; private set; }

        /// <summary>
        /// Gets the ResourceDictionaries added to the application resources.
        /// </summary>
        public ThemeResourceDictionary[] NewResources { get; private set; }

        public ThemeChangedEventArgs(ThemeResourceDictionary[] oldResources, ThemeResourceDictionary[] newResources)
        {
            OldResources = oldResources;
            NewResources = newResources;
        }
    }

    /// <summary>
    /// Helper class that replaces ResourceDictionary for theme switching.
    /// </summary>
    public static class ThemeSelector
    {
        /// <summary>
        /// Raised when the theme changes.
        /// </summary>
        public static event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        /// <summary>
        /// Builds a XAML resource Uri.
        /// </summary>
        /// <param name="assembly">The assembly containing the theme. (ex: 'SiliconStudio.Presentation')</param>
        /// <param name="themeName">The name of the theme. (ex: 'ExpressionDark/Theme.xaml')</param>
        /// <returns>Returns a XAML resource Uri.</returns>
        public static Uri ToUri(string assembly, string themeName)
        {
            var ext = Path.GetExtension(assembly);

            if (ext == ".dll" || ext == ".exe")
                assembly = Path.GetFileNameWithoutExtension(assembly);

            ext = Path.GetExtension(themeName);
            if (ext != ".xaml")
                themeName += ".xaml";

            return new Uri(string.Format("/{0};component/Themes/{1}", assembly, themeName), UriKind.Relative);
        }

        /// <summary>
        /// Replaces the ThemeResourceDictionary instances by new ones referenced by there Uri.
        /// </summary>
        /// <param name="element">The element to replace its ThemeResourceDictionary.</param>
        /// <param name="themeUris">The reference Uris of the new ThemeResourceDictionary instances.</param>
        public static void SetTheme(FrameworkElement element, params Uri[] themeUris)
        {
            if (element == null)
                return;

            ApplyTheme(element.Resources, themeUris);
        }

        /// <summary>
        /// Replaces the ThemeResourceDictionary instances by new ones referenced by there Uri.
        /// </summary>
        /// <param name="app">The application to replace its ThemeResourceDictionary.</param>
        /// <param name="themeUris">The reference Uris of the new ThemeResourceDictionary instances.</param>
        public static void SetTheme(this Application app, params Uri[] themeUris)
        {
            if (app == null)
                return;

            ApplyTheme(app.Resources, themeUris);
        }

        /// <summary>
        /// Cleanups the existing ThemeResourceDictionary instances from within the
        /// given ResourceDictionary and add new ones referenced by the given Uris.
        /// </summary>
        /// <param name="resources">The ResourceDictionary to update.</param>
        /// <param name="themeUris">The Uri reference of the ResourceDictionary.</param>
        private static void ApplyTheme(ResourceDictionary resources, params Uri[] themeUris)
        {
            if (resources == null)
                return;

            // cleanup the existing ThemeResourceDictionary
            var oldResources = resources.MergedDictionaries
                .OfType<ThemeResourceDictionary>()
                .ToArray();

            oldResources.ForEach(x => resources.MergedDictionaries.Remove(x));

            var newResources = themeUris.Select(uri => new ThemeResourceDictionary { Source = uri }).ToArray();

            // add new ThemeResourceDictionary instances
            // Reverse is used because newly created dictionaries are inserted at the position zero
            foreach (var res in newResources.Reverse())
            {
                try
                {
                    resources.MergedDictionaries.Insert(0, res);
                }
                catch
                {
                    Debug.WriteLine("Invalid or not found ResourceDictionary at URI {0}", res.Source);
                }
            }

            var handler = ThemeChanged;

            if (handler != null)
                handler(null, new ThemeChangedEventArgs(oldResources, newResources));
        }
    }
}
