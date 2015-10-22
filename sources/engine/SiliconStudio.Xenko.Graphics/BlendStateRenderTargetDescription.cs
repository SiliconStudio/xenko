// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Describes the blend state for a render target.
    /// </summary>
    [DataContract]
    public struct BlendStateRenderTargetDescription
    {
        /// <summary>
        /// Enable (or disable) blending. 
        /// </summary>
        public bool BlendEnable;

        /// <summary>
        /// This <see cref="Blend"/> specifies the first RGB data source and includes an optional pre-blend operation. 
        /// </summary>
        public Blend ColorSourceBlend;

        /// <summary>
        /// This <see cref="Blend"/> specifies the second RGB data source and includes an optional pre-blend operation. 
        /// </summary>
        public Blend ColorDestinationBlend;

        /// <summary>
        /// This <see cref="BlendFunction"/> defines how to combine the RGB data sources. 
        /// </summary>
        public BlendFunction ColorBlendFunction;

        /// <summary>
        /// This <see cref="Blend"/> specifies the first alpha data source and includes an optional pre-blend operation. Blend options that end in _COLOR are not allowed. 
        /// </summary>
        public Blend AlphaSourceBlend;

        /// <summary>
        /// This <see cref="Blend"/> specifies the second alpha data source and includes an optional pre-blend operation. Blend options that end in _COLOR are not allowed. 
        /// </summary>
        public Blend AlphaDestinationBlend;

        /// <summary>
        /// This <see cref="BlendFunction"/> defines how to combine the alpha data sources. 
        /// </summary>
        public BlendFunction AlphaBlendFunction;

        /// <summary>
        /// A write mask. 
        /// </summary>
        public ColorWriteChannels ColorWriteChannels;
    }
}