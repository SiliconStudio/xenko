// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.Effects.Skyboxes
{
    /// <summary>
    /// A list of <see cref="SkyboxComponent"/>.
    /// </summary>
    public class SkyboxComponentCollection : FastList<SkyboxComponent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxComponentCollection"/> class.
        /// </summary>
        public SkyboxComponentCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxComponentCollection"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public SkyboxComponentCollection(int capacity)
            : base(capacity)
        {
        }
    }
}