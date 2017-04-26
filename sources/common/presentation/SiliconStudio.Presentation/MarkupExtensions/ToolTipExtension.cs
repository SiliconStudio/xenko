// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Windows.Markup;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    /// <summary>
    /// This markup extension allows to format the text of a tooltip a text and a gesture.
    /// </summary>
    public class ToolTipExtension : MarkupExtension
    {
        private readonly string content;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolTipExtension"/> class.
        /// </summary>
        /// <param name="text">A string representing the tooltip text</param>
        /// <param name="gesture">A string representing the gesture.</param>
        public ToolTipExtension(string text, string gesture)
        {
            content = !string.IsNullOrEmpty(gesture) ? $"{text} ({gesture})" : text;
        }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return content;
        }
    }
}
