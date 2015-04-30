// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Processors
{
    /// <summary>
    /// Projection of a <see cref="CameraComponent"/>.
    /// </summary>
    [DataContract("CameraProjectionMode")]
    public enum CameraProjectionMode
    {
        /// <summary>
        /// A perspective projection.
        /// </summary>
        Perspective,

        /// <summary>
        /// An orthographic projection.
        /// </summary>
        Orthographic
    }
}