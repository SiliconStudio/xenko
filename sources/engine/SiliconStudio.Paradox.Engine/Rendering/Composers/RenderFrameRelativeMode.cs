// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Rendering.Composers
{
    /// <summary>
    /// Specifies which <see cref="RenderFrame"/> to use when creating the output of <see cref="SceneGraphicsLayer"/> and 
    /// the size mode defined by <see cref="RenderFrameDescriptor.Mode"/> is <see cref="RenderFrameSizeMode.Relative"/>.
    /// </summary>
    public enum RenderFrameRelativeMode
    {
        /// <summary>
        /// The size of the <see cref="RenderFrame"/> is calculated relatively to the current frame.
        /// </summary>
        Current,

        /// <summary>
        /// The size of the <see cref="RenderFrame"/> is calculated relatively to the master frame.
        /// </summary>
        Master
    }
}