// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows.Markup;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(double))]
    public class DoubleExtension : MarkupExtension
    {
        public double Value { get; set; }

        public DoubleExtension(object value)
        {
            Value = Convert.ToDouble(value);
        }

        [NotNull]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }
    }
}