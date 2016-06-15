// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Text.RegularExpressions;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Particles.Materials;

namespace ParticleMaterialShader.Materials
{
    [DataContract("ParticleCustomMaterial")]
    [Display("ParticleCustomMaterial")]
    public class ParticleCustomMaterial : ParticleMaterialSimple
    {
        private ShaderSource shaderBaseColor;
        private ShaderSource shaderBaseScalar;

        [DataMemberIgnore]
        public override string EffectName { get; protected set; } = "ParticleCustomEffect";

        /// <summary>
        /// <see cref="IComputeColor"/> allows several channels to be blended together, including textures, vertex streams and fixed values.
        /// </summary>
        /// <userdoc>
        /// Emissive component ignores light and defines a fixed color this particle should use (emit) when rendered.
        /// </userdoc>
        [DataMember(100)]
        [Display("Emissive")]
        public IComputeColor ComputeColor { get; set; } = new ComputeTextureColor();

        /// <summary>
        /// <see cref="UVBuilder"/> defines how the base coordinates of the particle shape should be modified for texture scrolling, animation, etc.
        /// </summary>
        /// <userdoc>
        /// If left blank, the texture coordinates will be the original ones from the shape builder, usually (0, 0, 1, 1). Or you can define a custom texture coordinate builder which modifies the original coordinates for the sprite.
        /// </userdoc>
        [DataMember(200)]
        [Display("TexCoord0")]
        public UVBuilder UVBuilder0;
        private AttributeDescription texCoord0 = new AttributeDescription("TEXCOORD");

        /// <summary>
        /// <see cref="IComputeColor"/> allows several channels to be blended together, including textures, vertex streams and fixed values.
        /// </summary>
        /// <userdoc>
        /// Alpha component which defines how opaque (1) or transparent (0) the color will be
        /// </userdoc>
        [DataMember(300)]
        [Display("Alpha")]
        public IComputeScalar ComputeScalar { get; set; } = new ComputeTextureScalar();

        /// <summary>
        /// <see cref="UVBuilder"/> defines how the base coordinates of the particle shape should be modified for texture scrolling, animation, etc.
        /// </summary>
        /// <userdoc>
        /// If left blank, the texture coordinates will be the original ones from the shape builder, usually (0, 0, 1, 1). Or you can define a custom texture coordinate builder which modifies the original coordinates for the sprite.
        /// </userdoc>
        [DataMember(400)]
        [Display("TexCoord1")]
        public UVBuilder UVBuilder1;
        private AttributeDescription texCoord1 = new AttributeDescription("TEXCOORD1");

        protected override void InitializeCore(RenderContext context)
        {
            base.InitializeCore(context);

            UpdateShaders(context.GraphicsDevice);
        }


        /// <inheritdoc />
        public override void Setup(RenderContext context)
        {
            base.Setup(context);

            UpdateShaders(context.GraphicsDevice);
        }

        public override void ValidateEffect(RenderContext context, ref EffectValidator effectValidator)
        {
            effectValidator.ValidateParameter(ParticleCustomShaderKeys.BaseColor, shaderBaseColor);
            effectValidator.ValidateParameter(ParticleCustomShaderKeys.BaseIntensity, shaderBaseScalar);
        }

        private void UpdateShaders(GraphicsDevice graphicsDevice)
        {
            if (ComputeColor != null && ComputeScalar != null)
            {
                var shaderGeneratorContext = new ShaderGeneratorContext(graphicsDevice)
                {
                    Parameters = Parameters,
                    ColorSpace = graphicsDevice.ColorSpace
                };

                // Don't forget to set the proper color space!
                shaderGeneratorContext.ColorSpace = graphicsDevice.ColorSpace;

                var newShaderBaseColor = ComputeColor.GenerateShaderSource(shaderGeneratorContext, new MaterialComputeColorKeys(ParticleCustomShaderKeys.EmissiveMap, ParticleCustomShaderKeys.EmissiveValue, Color.White));
                var newShaderBaseScalar = ComputeScalar.GenerateShaderSource(shaderGeneratorContext, new MaterialComputeColorKeys(ParticleCustomShaderKeys.IntensityMap, ParticleCustomShaderKeys.IntensityValue, Color.White));

                // Check if shader code has changed
                if (!newShaderBaseColor.Equals(shaderBaseColor) || !newShaderBaseScalar.Equals(shaderBaseScalar))
                {
                    shaderBaseColor = newShaderBaseColor;
                    shaderBaseScalar = newShaderBaseScalar;
                    Parameters.Set(ParticleCustomShaderKeys.BaseColor, shaderBaseColor);
                    Parameters.Set(ParticleCustomShaderKeys.BaseIntensity, shaderBaseScalar);

                    // TODO: Is this necessary?
                    HasVertexLayoutChanged = true;
                }
            }
        }

        public override void UpdateVertexBuilder(ParticleVertexBuilder vertexBuilder)
        {
            base.UpdateVertexBuilder(vertexBuilder);

            var code = shaderBaseColor?.ToString();

            if (code?.Contains("COLOR0") ?? false)
            {
                vertexBuilder.AddVertexElement(ParticleVertexElements.Color);
            }

            //  There are two UV builders, building texCoord0 and texCoord1
            //  Which set is referenced can be set by the user in the IComputeColor tree
            vertexBuilder.AddVertexElement(ParticleVertexElements.TexCoord[0]);

            vertexBuilder.AddVertexElement(ParticleVertexElements.TexCoord[1]);
        }

        public unsafe override void PatchVertexBuffer(ParticleVertexBuilder vertexBuilder, Vector3 invViewX, Vector3 invViewY, ParticleSorter sorter)
        {
            // If you want, you can integrate the base builder here and not call it. It should result in slight speed up
            base.PatchVertexBuffer(vertexBuilder, invViewX, invViewY, sorter);

            // Update the non-default coordinates first, because they update off the default ones
            UVBuilder1?.BuildUVCoordinates(vertexBuilder, sorter, texCoord1);

            // Update the default coordinates last
            UVBuilder0?.BuildUVCoordinates(vertexBuilder, sorter, texCoord0);

            // If the particles have color field, the base class should have already passed the information
            if (HasColorField)
                return;

            // If there is no color stream we don't need to fill anything
            var colAttribute = vertexBuilder.GetAccessor(VertexAttributes.Color);
            if (colAttribute.Size <= 0)
                return;

            // Since the particles don't have their own color field, set the default color to white
            var color = 0xFFFFFFFF;

            vertexBuilder.RestartBuffer();
            foreach (var particle in sorter)
            {
                vertexBuilder.SetAttributePerParticle(colAttribute, (IntPtr)(&color));

                vertexBuilder.NextParticle();
            }

            vertexBuilder.RestartBuffer();
        }

    }
}
