// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine.Processors
{
    /// <summary>
    /// A collection of <see cref="RenderModel"/> for a specific <see cref="EntityGroup"/>.
    /// </summary>
    public class RenderModelCollection : List<RenderModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderModelCollection"/> class.
        /// </summary>
        /// <param name="group">The group.</param>
        public RenderModelCollection(EntityGroup group)
        {
            Group = group;
        }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <value>The group.</value>
        public EntityGroup Group { get; private set; }
    }
}