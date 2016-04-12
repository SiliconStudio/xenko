// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
        [DataMember]
        public Color BarColor
        {
            get { return BarColorInternal; }
            set { BarColorInternal = value; }
        }
    }
}
