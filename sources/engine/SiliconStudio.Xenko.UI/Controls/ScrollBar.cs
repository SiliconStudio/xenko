// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Diagnostics;
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
        /// <userdoc>The color of the bar.</userdoc>
        [DataMember]
        [Display(category: AppearanceCategory)]
        public Color BarColor
        {
            get { return BarColorInternal; }
            set { BarColorInternal = value; }
        }
    }
}
