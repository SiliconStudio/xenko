// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Assets.Materials.Processor.Visitors;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    /// <summary>
    /// A scalar texture node.
    /// </summary>
    [DataContract("MaterialTextureComputeScalar")]
    [Display("Texture")]
    public class MaterialTextureComputeScalar : MaterialTextureComputeNodeBase, IMaterialComputeScalar
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MaterialTextureComputeScalar()
            : this(null, TextureCoordinate.Texcoord0, Vector2.One, Vector2.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialTextureComputeColor" /> class.
        /// </summary>
        /// <param name="texturePath">Name of the texture.</param>
        /// <param name="texcoordIndex">Index of the texcoord.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="offset">The offset.</param>
        public MaterialTextureComputeScalar(string texturePath, TextureCoordinate texcoordIndex, Vector2 scale, Vector2 offset)
            : base(texturePath, texcoordIndex, scale, offset)
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