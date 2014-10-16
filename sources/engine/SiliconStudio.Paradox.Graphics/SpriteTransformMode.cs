// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Describe what a provided matrix represent.
    /// </summary>
    public enum SpriteTransformMode
    {
        /// <summary>
        /// The world matrix.
        /// </summary>
        WorldTransform,

        /// <summary>
        /// The composition of the world and projection matrix.
        /// </summary>
        WorldProjectionTransform,
    }
}