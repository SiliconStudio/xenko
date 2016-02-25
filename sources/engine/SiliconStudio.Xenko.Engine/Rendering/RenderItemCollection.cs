// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A collection of <see cref="RenderItem"/>.
    /// </summary>
    [Obsolete]
    public class RenderItemCollection : List<RenderItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderItemCollection" /> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="hasTransparency">if set to <c>true</c> [has transparency].</param>
        public RenderItemCollection(int capacity, bool hasTransparency) : base(capacity)
        {
            HasTransparency = hasTransparency;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has transparency.
        /// </summary>
        /// <value><c>true</c> if this instance has transparency; otherwise, <c>false</c>.</value>
        public bool HasTransparency { get; private set; }
    }
}