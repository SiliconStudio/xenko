// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Culling mode of a <see cref="SceneCameraRenderer"/>.
    /// </summary>
    public enum CullingMode
    {
        /// <summary>
        /// No culling is applied to meshes.
        /// </summary>
        None,

        /// <summary>
        /// Meshes outside of the camera's view frustum will be culled.
        /// </summary>
        Frustum
    }
}
