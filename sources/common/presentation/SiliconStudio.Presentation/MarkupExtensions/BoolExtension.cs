// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows.Markup;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(bool))]
    public class BoolExtension : MarkupExtension
    {
        public bool Value { get; set; }

        public BoolExtension(bool value)
        {
            Value = value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }
    }
}
