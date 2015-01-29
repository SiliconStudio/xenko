// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// A list of <see cref="LightComponent"/>.
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
    }
}