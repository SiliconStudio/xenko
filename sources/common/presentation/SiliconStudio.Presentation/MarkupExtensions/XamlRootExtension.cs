// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Xaml;
using System.Windows.Markup;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    /// <summary>
    /// Finds and returns the root object of the current XAML document.
    /// </summary>
    public sealed class XamlRootExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var provider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            return provider?.RootObject;
        }
    }
}
