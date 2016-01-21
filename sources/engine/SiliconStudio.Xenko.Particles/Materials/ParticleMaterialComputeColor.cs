// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Text.RegularExpressions;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Assets;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("ParticleMaterialComputeColor")]
    [Display("DynamicEmissive")]
    public class ParticleMaterialComputeColor : ParticleMaterialSimple
    {
        [DataMemberIgnore]
        protected override string EffectName { get; set; } = "ParticleEffect";

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
        /// <see cref="Materials.UVBuilder"/> defines how the base coordinates of the particle shape should be modified for texture scrolling, animation, etc.
        /// </summary>
        /// <userdoc>
        /// If left blank, the texture coordinates will be the original ones from the shape builder, usually (0, 0, 1, 1). Or you can define a custom texture coordinate builder which modifies the original coordinates for the sprite.
        /// </userdoc>
        [DataMember(200)]
        [Display("UV coords")]
        public UVBuilder UVBuilder;

        [DataMemberIgnore]
        private ShaderGeneratorContext shaderGeneratorContext;

        protected override void InitializeCore(RenderContext context)
        {
            base.InitializeCore(context);

            UpdateShaders();
        }

        private int shadersUpdateCounter;
        private void UpdateShaders()
        {
            // TODO Don't do this every frame!!! <- Propagate changes
            if (--shadersUpdateCounter > 0)
                return;
            shadersUpdateCounter = 10;

            // Weird bug? If the shaderGeneratorContext.Parameters stay the same the particles disappear
            if (shaderGeneratorContext != null)
            {
                ParameterCollections.Remove(shaderGeneratorContext.Parameters);
                shaderGeneratorContext = null;
            }

            if (shaderGeneratorContext == null)
            {
                shaderGeneratorContext = new ShaderGeneratorContext();
                ParameterCollections.Add(shaderGeneratorContext.Parameters);
            }

            shaderGeneratorContext.Parameters.Clear();

            if (ComputeColor != null)
            {
                var shaderBaseColor = ComputeColor.GenerateShaderSource(shaderGeneratorContext, new MaterialComputeColorKeys(ParticleBaseKeys.EmissiveMap, ParticleBaseKeys.EmissiveValue, Color.White));

                shaderGeneratorContext.Parameters.Set(ParticleBaseKeys.BaseColor, shaderBaseColor);

                // Check if shader code has changed
                if (!shaderBaseColor.Equals(shaderSource))
                {
                    shaderSource = shaderBaseColor;
                    VertexLayoutHasChanged = true;
                }
            }
        }

        private ShaderSource shaderSource;

        public override void UpdateVertexBuilder(ParticleVertexBuilder vertexBuilder)
        {
            base.UpdateVertexBuilder(vertexBuilder);

            // The arguments we need are in the GenericArguments, which is again just an array of strings
            // We could search it element by element, but in the end getting the entire string and searching it instead is the same

            var code = shaderSource?.ToString();

            if (code?.Contains("COLOR0") ?? false)
            {
                vertexBuilder.AddVertexElement(ParticleVertexElements.Color);                
            }

            var coordIndex = code?.IndexOf("TEXCOORD", 0, StringComparison.Ordinal) ?? -1;
            while (coordIndex >= 0)
            {
                var semanticIndex = 0;
                var subStr = code.Substring(coordIndex + 8);

                if (int.TryParse(Regex.Match(subStr, @"\d+").Value, out semanticIndex))
                {
                    semanticIndex = (semanticIndex <  0) ?  0 : semanticIndex;
                    semanticIndex = (semanticIndex > 15) ? 15 : semanticIndex;

                    vertexBuilder.AddVertexElement(ParticleVertexElements.TexCoord[semanticIndex]);
                }

                coordIndex = code.IndexOf("TEXCOORD", coordIndex + 1);
            }
        }

        public override void Setup(GraphicsDevice graphicsDevice, RenderContext context, Matrix viewMatrix, Matrix projMatrix, Color4 color)
        {
            base.Setup(graphicsDevice, context, viewMatrix, projMatrix, color);
            
            UpdateShaders();
        }


        public unsafe override void PatchVertexBuffer(ParticleVertexBuilder vertexBuilder, Vector3 invViewX, Vector3 invViewY, ParticleSorter sorter)
        {
            // If you want, you can integrate the base builder here and not call it. It should result in slight speed up
            base.PatchVertexBuffer(vertexBuilder, invViewX, invViewY, sorter);

            UVBuilder?.BuildUVCoordinates(vertexBuilder, sorter);

            // TODO Copy Texture fields

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
