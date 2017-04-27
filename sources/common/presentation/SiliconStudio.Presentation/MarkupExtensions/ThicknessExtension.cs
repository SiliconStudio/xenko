// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Windows;
using System.Windows.Markup;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(ThicknessExtension))]
    public class ThicknessExtension : MarkupExtension
    {
        public ThicknessExtension(double uniformLength)
        {
            Value = new Thickness(uniformLength);
        }

        public ThicknessExtension(double horizontal, double vertical)
        {
            Value = new Thickness(horizontal, vertical, horizontal, vertical);
        }

        public ThicknessExtension(double left, double top, double right, double bottom)
        {
            Value = new Thickness(left, top, right, bottom);
        }

        public ThicknessExtension(Thickness value)
        {
            Value = value;
        }

        public Thickness Value { get; set; }

        [NotNull]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }
    }
}
