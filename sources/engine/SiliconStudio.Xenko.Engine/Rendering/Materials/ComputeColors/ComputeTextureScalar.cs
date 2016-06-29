// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Materials.Processor.Visitors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
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
            Channel = ColorChannel.R;
            FallbackValue = new ComputeFloat(1);
        }

        /// <summary>
        /// Gets or sets the default value used when no texture is set.
        /// </summary>
        /// <userdoc>The fallback value used when no texture is set.</userdoc>
        [NotNull]
        [DataMember(15)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public ComputeFloat FallbackValue { get; set; }

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        /// <value>The channel.</value>
        /// <userdoc>Selects the RGBA channel to sample from the texture.</userdoc>
        [DataMember(20)]
        [DefaultValue(ColorChannel.R)]
        public ColorChannel Channel { get; set; }

        protected override string GetTextureChannelAsString()
        {
            return MaterialUtility.GetAsShaderString(Channel);
        }

        public override ShaderSource GenerateShaderFromFallbackValue(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            return FallbackValue?.GenerateShaderSource(context, baseKeys);
        }



    }
}