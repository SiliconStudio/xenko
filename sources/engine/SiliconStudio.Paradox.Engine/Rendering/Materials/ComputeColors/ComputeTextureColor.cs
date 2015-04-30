// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A color texture node.
    /// </summary>
    [DataContract("ComputeTextureColor")]
    [Display("Texture")]
    public class ComputeTextureColor : ComputeTextureBase, IComputeColor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ComputeTextureColor()
            : base(null, TextureCoordinate.Texcoord0, Vector2.One, Vector2.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeTextureColor"/> class.
        /// </summary>
        /// <param name="texture">The texture.</param>
        public ComputeTextureColor(Texture texture)
            : base(texture, TextureCoordinate.Texcoord0, Vector2.One, Vector2.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeTextureColor" /> class.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="texcoordIndex">Index of the texcoord.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="offset">The offset.</param>
        public ComputeTextureColor(Texture texture, TextureCoordinate texcoordIndex, Vector2 scale, Vector2 offset)
            : base(texture, texcoordIndex, scale, offset)
        {
        }

        protected override string GetTextureChannelAsString()
        {
            // Use all channels
            return "rgba";
        }
    }
}
