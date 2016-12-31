// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows.Markup;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(double))]
    public sealed class MaxDoubleExtension : MarkupExtension
    {
        [NotNull]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return double.MaxValue;
        }
    }
}