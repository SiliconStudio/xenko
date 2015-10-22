// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// The size of a <see cref="RenderFrame"/>.
    /// </summary>
    [DataContract("RenderFrameSizeMode")]
    public enum RenderFrameSizeMode
    {
        /// <summary>
        /// The size of the render frame is relative in percentage to the <see cref="GraphicsDevice.BackBuffer"/> size or 
        /// the render target being rendered in a composite rendering.
        /// </summary>
        /// <userdoc>The size of the render frame is relative in percentage to the <see cref="GraphicsDevice.BackBuffer"/> size or 
        /// the render target being rendered in a composite rendering.</userdoc>
        Relative,

        /// <summary>
        /// The size is fixed in pixels.
        /// </summary>
        /// <userdoc>The size of the render frame is fixed in pixels.</userdoc>
        Fixed,
    }
}