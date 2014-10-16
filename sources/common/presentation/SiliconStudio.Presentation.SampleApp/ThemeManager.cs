// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SiliconStudio.Presentation.SampleApp
{
    public enum AvailableTheme
    {
        Generic,
        ExpressionDark
    }
    
    public static class ThemeManager
    {
        private static readonly Collection<ResourceDictionary> ExpressionDarkDictionaries = new Collection<ResourceDictionary>();

        public static void Initialize()
        {
            AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName("SiliconStudio.Presentation.PropertyGrid.dll"));
            var expressionDarkThemeDictionary = (ResourceDictionary)Application.LoadComponent(new Uri("/SiliconStudio.Presentation;component/Themes/ExpressionDark/Theme.xaml", UriKind.RelativeOrAbsolute));
            var expressionDarkPropertyGridDictionary = (ResourceDictionary)Application.LoadComponent(new Uri("/SiliconStudio.Presentation.PropertyGrid;component/Themes/ExpressionDark/Theme.xaml", UriKind.RelativeOrAbsolute));
            ExpressionDarkDictionaries.Add(expressionDarkThemeDictionary);
            ExpressionDarkDictionaries.Add(expressionDarkPropertyGridDictionary);

            SetThemeCommand = new RoutedCommand("SetThemeCommand", typeof(FrameworkElement));
            CommandManager.RegisterClassCommandBinding(typeof(FrameworkElement), new CommandBinding(SetThemeCommand, OnSetThemeCommand));
        }

        public static RoutedCommand SetThemeCommand { get; private set; }

        public static void SetTheme(AvailableTheme theme)
        {
            switch (theme)
            {
                case AvailableTheme.Generic:
                    foreach (var dictionary in ExpressionDarkDictionaries)
                        Application.Current.Resources.MergedDictionaries.Remove(dictionary);
                    break;
                case AvailableTheme.ExpressionDark:
                    foreach (var dictionary in ExpressionDarkDictionaries)
                        Application.Current.Resources.MergedDictionaries.Add(dictionary);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("theme");
            }
        }

        private static void OnSetThemeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SetTheme((AvailableTheme)e.Parameter);
        }
    }
}