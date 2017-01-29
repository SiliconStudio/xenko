// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// A list of <see cref="LightComponent"/> for a specified <see cref="RenderGroupMask"/>.
    /// </summary>
    public class LightComponentCollection : FastList<LightComponent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightComponentCollection"/> class.
        /// </summary>
        public LightComponentCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightComponentCollection"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public LightComponentCollection(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Gets or sets the culling mask.
        /// </summary>
        /// <value>The culling mask.</value>
        public RenderGroupMask CullingMask { get; internal set; }

        /// <summary>
        /// Tags attached.
        /// </summary>
        public PropertyContainer Tags;

    }
}