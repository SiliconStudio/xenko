// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// A collection of UIElements.
    /// </summary>
    public class UIElementCollection : TrackingCollection<UIElement>
    {
        /// <summary>
        /// Gets the underlying list if not possible. 
        /// Returns null if the underlying collection is not a list.
        /// </summary>
        public List<UIElement> UnderlyingList
        {
            get { return Items is List<UIElement>? (List<UIElement>)Items: null; }
        }
    }
}