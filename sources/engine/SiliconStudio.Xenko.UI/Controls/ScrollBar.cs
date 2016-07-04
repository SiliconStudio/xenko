// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// Represents a scroll bar. 
    /// </summary>
    [DataContract(nameof(ScrollBar))]
    [DebuggerDisplay("ScrollBar - Name={Name}")]
    public class ScrollBar : UIElement
    {
        public ScrollBar()
        {
            BarColorInternal = new Color(0, 0, 0, 0);
        }

        internal Color BarColorInternal;

        /// <summary>
        /// The color of the bar.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        public Color BarColor
        {
            get { return BarColorInternal; }
            set { BarColorInternal = value; }
        }

        /// <inheritdoc/>
        protected override IEnumerable<IUIElementChildren> EnumerateChildren() => Enumerable.Empty<IUIElementChildren>();
    }
}
