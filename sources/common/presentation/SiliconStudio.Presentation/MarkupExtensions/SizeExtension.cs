// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Windows;
using System.Windows.Markup;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(SizeExtension))]
    public class SizeExtension : MarkupExtension
    {
        public SizeExtension(double uniformLength)
        {
            Value = new Size(uniformLength, uniformLength);
        }

        public SizeExtension(double width, double height)
        {
            Value = new Size(width, height);
        }

        public SizeExtension(Size value)
        {
            Value = value;
        }

        public Size Value { get; set; }

        [NotNull]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }
    }
}
