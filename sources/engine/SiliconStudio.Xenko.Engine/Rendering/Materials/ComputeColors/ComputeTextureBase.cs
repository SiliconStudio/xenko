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
    /// Base class for texture nodes
    /// </summary>
    [DataContract(Inherited = true)]
    [Display("Texture")]
    public abstract class ComputeTextureBase : ComputeKeyedBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeTextureColor" /> class.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="texcoordIndex">Index of the texcoord.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="offset">The offset.</param>
        protected ComputeTextureBase(Texture texture, TextureCoordinate texcoordIndex, Vector2 scale, Vector2 offset)
        {
            Enabled = true;
            Texture = texture;
            TexcoordIndex = texcoordIndex;
            Sampler = new ComputeColorParameterSampler();
            Scale = scale;
            Offset = offset;
            Key = null;
        }

        /// <summary>
        /// Gets or sets the enable state of the texture.
        /// </summary>
        /// <userdoc>If unchecked, the texture value is ignored and the fallback value is used instead.</userdoc>
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// The texture Reference.
        /// </summary>
        /// <userdoc>
        /// The reference to the texture asset to use.
        /// </userdoc>
        [DataMember(10)] 
        [DefaultValue(null)]
        [Display("Texture")]
        [InlineProperty]
        public Texture Texture { get; set; }

        /// <summary>
        /// The texture coordinate used to sample the texture.
        /// </summary>
        /// <userdoc>
        /// The set of uv used to sample the texture.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(TextureCoordinate.Texcoord0)]
        public TextureCoordinate TexcoordIndex { get; set; }

        /// <summary>
        /// The sampler of the texture.
        /// </summary>
        /// <userdoc>
        /// The sampler of the texture.
        /// </userdoc>
        [DataMemberIgnore]
        private ComputeColorParameterSampler Sampler { get; set; }

        /// <summary>
        /// The texture filtering mode.
        /// </summary>
        /// <userdoc>
        /// The filtering method to use.
        /// </userdoc>
        [DataMember(41)]
        [DefaultValue(TextureFilter.Linear)]
        public TextureFilter Filtering
        {
            get
            {
                return Sampler.Filtering;
            }
            set
            {
                Sampler.Filtering = value;
            }
        }

        /// <summary>
        /// The texture address mode.
        /// </summary>
        /// <userdoc>
        /// Specify how to wrap the texture along the U axis (horizontal axis).
        /// </userdoc>
        [DataMember(42)]
        [DefaultValue(TextureAddressMode.Wrap)]
        public TextureAddressMode AddressModeU
        {
            get
            {
                return Sampler.AddressModeU;
            }
            set
            {
                Sampler.AddressModeU = value;
            }
        }

        /// <summary>
        /// The texture address mode.
        /// </summary>
        /// <userdoc>
        /// Specify how to wrap the texture along the V axis (vertical axis).
        /// </userdoc>
        [DataMember(43)]
        [DefaultValue(TextureAddressMode.Wrap)]
        public TextureAddressMode AddressModeV
        {
            get
            {
                return Sampler.AddressModeV;
            }
            set
            {
                Sampler.AddressModeV = value;
            }
        }

        /// <summary>
        /// The scale of the texture coordinates.
        /// </summary>
        /// <userdoc>
        /// The scale to apply onto the texture coordinates. This can be used to zoom into texture or tile it (lower than 1 -> zooming, greater than 1 -> tiling).
        /// </userdoc>
        [DataMember(50)]
        public Vector2 Scale { get; set; }

        /// <summary>
        /// The offset in the texture coordinates.
        /// </summary>
        /// <userdoc>
        /// The offsets to apply onto the model's texture coordinates.
        /// </userdoc>
        [DataMember(60)]
        public Vector2 Offset { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Texture";
        }

        protected abstract string GetTextureChannelAsString();

        public abstract ShaderSource GenerateShaderFromFallbackValue(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys);

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            if (!Enabled || Texture == null) // generate shader from default value when the texture is null or disabled
            {
                var fallbackValue = GenerateShaderFromFallbackValue(context, baseKeys);
                if (fallbackValue != null)
                    return fallbackValue;
            }

            // generate shader from the texture
            // TODO: Use a generated UsedTexcoordIndex when backing textures
            var usedTexcoord = "TEXCOORD" + MaterialUtility.GetTextureIndex(TexcoordIndex);

            var textureKey = context.GetTextureKey(this, baseKeys);
            var samplerKey = context.GetSamplerKey(Sampler);
            UsedKey = textureKey;

            var scale = Scale;
            var scaleFactor = context.CurrentOverrides.UVScale;
            if (scaleFactor != Vector2.One)
            {
                scale *= scaleFactor;
            }

            var channelStr = GetTextureChannelAsString();

            // "TTEXTURE", "TStream"
            ShaderClassSource shaderSource;

            // TODO: Workaround bad to have to copy all the new ShaderClassSource(). Check how to improve this
            if (context.OptimizeMaterials)
            {
                var scaleStr = MaterialUtility.GetAsShaderString(scale);
                var offsetStr = MaterialUtility.GetAsShaderString(Offset);

                // If materials are optimized, we precompute best shader combination (note: will generate shader permutations!)
                if (context.IsNotPixelStage)
                {
                    if (Offset != Vector2.Zero)
                        shaderSource = new ShaderClassSource("ComputeColorTextureLodScaledOffsetSampler", textureKey, usedTexcoord, samplerKey, channelStr, scaleStr, offsetStr, 0.0f);
                    else if (scale != Vector2.One)
                        shaderSource = new ShaderClassSource("ComputeColorTextureLodScaledSampler", textureKey, usedTexcoord, samplerKey, channelStr, scaleStr, 0.0f);
                    else
                        shaderSource = new ShaderClassSource("ComputeColorTextureLodSampler", textureKey, usedTexcoord, samplerKey, channelStr, 0.0f);
                }
                else
                {
                    if (Offset != Vector2.Zero)
                        shaderSource = new ShaderClassSource("ComputeColorTextureScaledOffsetSampler", textureKey, usedTexcoord, samplerKey, channelStr, scaleStr, offsetStr);
                    else if (scale != Vector2.One)
                        shaderSource = new ShaderClassSource("ComputeColorTextureScaledSampler", textureKey, usedTexcoord, samplerKey, channelStr, scaleStr);
                    else
                        shaderSource = new ShaderClassSource("ComputeColorTextureSampler", textureKey, usedTexcoord, samplerKey, channelStr);
                }
            }
            else
            {
                // Try to avoid shader permutations, by putting UV scaling/offset in shader parameters
                var textureScale = (ValueParameterKey<Vector2>)context.GetParameterKey(MaterialKeys.TextureScale);
                var textureOffset = (ValueParameterKey<Vector2>)context.GetParameterKey(MaterialKeys.TextureOffset);

                context.Parameters.Set(textureScale, scale);
                context.Parameters.Set(textureOffset, Offset);

                if (context.IsNotPixelStage)
                {
                    shaderSource = new ShaderClassSource("ComputeColorTextureLodScaledOffsetDynamicSampler", textureKey, usedTexcoord, samplerKey, channelStr, textureScale, textureOffset, 0.0f);
                }
                else
                {
                    shaderSource = new ShaderClassSource("ComputeColorTextureScaledOffsetDynamicSampler", textureKey, usedTexcoord, samplerKey, channelStr, textureScale, textureOffset);
                }
            }

            return shaderSource;
        }
    }
}