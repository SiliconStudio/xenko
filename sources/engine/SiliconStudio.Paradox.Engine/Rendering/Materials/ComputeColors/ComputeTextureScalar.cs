// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Rendering.Materials.Processor.Visitors;

namespace SiliconStudio.Paradox.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A scalar texture node.
    /// </summary>
    [DataContract("ComputeTextureScalar")]
    [Display("Texture")]
    public class ComputeTextureScalar : ComputeTextureBase, IComputeScalar
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ComputeTextureScalar()
            : this(null, TextureCoordinate.Texcoord0, Vector2.One, Vector2.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeTextureColor" /> class.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="texcoordIndex">Index of the texcoord.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="offset">The offset.</param>
        public ComputeTextureScalar(Texture texture, TextureCoordinate texcoordIndex, Vector2 scale, Vector2 offset)
            : base(texture, texcoordIndex, scale, offset)
        {
            Channel = TextureChannel.R;
        }

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        /// <value>The channel.</value>
        /// <userdoc>Selects the RGBA channel to sample from the texture.</userdoc>
        [DataMember(20)]
        [DefaultValue(TextureChannel.R)]
        public TextureChannel Channel { get; set; }

        protected override string GetTextureChannelAsString()
        {
            return MaterialUtility.GetAsShaderString(Channel);
        }
    }
}