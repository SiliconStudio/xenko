// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SiliconStudio.Presentation.Core;

namespace SiliconStudio.Presentation.Extensions
{
    public static class FrameworkElementExtensions
    {
        //public static async Task AwaitLoadedAsync(this FrameworkElement source)
        //{
        //    if (source.IsLoaded == false)
        //        await new LoadedEventAwaiter(source);
        //}

        // this is not an extension method because it is supposed to "extend" a method that is protected, so it has been made an helper method instead.
        public static T CheckTemplatePart<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
                return null;

            if ((dependencyObject is T) == false)
            {
                throw new ArgumentException(string.Format("Invalid '{0}' TemplatePart type. '{1}' expected.",
                    dependencyObject.GetType().FullName, typeof(T).FullName));
            }

            return (T)dependencyObject;
        }
    }
}
