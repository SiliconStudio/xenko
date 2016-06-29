// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
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

namespace SiliconStudio.Xenko.Particles.Materials
{
    /// <summary>
    /// <see cref="ParticleMaterialComputeColor"/> uses a <see cref="IComputeColor"/> tree to calculate the pixel's emissive value
    /// </summary>
    [DataContract("ParticleMaterialComputeColor")]
    [Display("Emissive Map")]
    public class ParticleMaterialComputeColor : ParticleMaterialSimple
    {
        // TODO Part of the graphics improvement XK-3052
        private int shadersUpdateCounter;

        // TODO Part of the graphics improvement XK-3052
        private ShaderSource shaderSource;

        [DataMemberIgnore]
        public override string EffectName { get; protected set; } = "ParticleEffect";

        /// <summary>
        /// <see cref="IComputeColor"/> allows several channels to be blended together, including textures, vertex streams and fixed values.
        /// </summary>
        /// <userdoc>
        /// Emissive component ignores light and defines a fixed color this particle should use (emit) when rendered.
        /// </userdoc>
        [DataMember(100)]
        [Display("Emissive Map")]
        public IComputeColor ComputeColor { get; set; } = new ComputeTextureColor();

        /// <summary>
        /// <see cref="Materials.UVBuilder"/> defines how the base coordinates of the particle shape should be modified for texture scrolling, animation, etc.
        /// </summary>
        /// <userdoc>
        /// If left blank, the texture coordinates will be the original ones from the shape builder, usually (0, 0, 1, 1). Or you can define a custom texture coordinate builder which modifies the original coordinates for the sprite.
        /// </userdoc>
        [DataMember(200)]
        [Display("UV coords")]
        public UVBuilder UVBuilder { get; set; }

        /// <summary>
        /// Forces the creation of texture coordinates as vertex attribute
        /// </summary>
        /// <userdoc>
        /// Forces the creation of texture coordinates as vertex attribute
        /// </userdoc>
        [DataMember(300)]
        [Display("Force texcoords")]
        public bool ForceTexCoords { get; set; } = false;

        /// <inheritdoc />
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

        /// <summary>
        /// Polls the shader generator if the shader code has changed and has to be reloaded
        /// </summary>
        /// <param name="graphicsDevice">The current <see cref="GraphicsDevice"/></param>
        private void UpdateShaders(GraphicsDevice graphicsDevice)
        {
            // TODO Part of the graphics improvement XK-3052
            // Don't do this every frame, we have to propagate changes better
            if (--shadersUpdateCounter > 0)
                return;
            shadersUpdateCounter = 10;

            if (ComputeColor != null)
            {
                var shaderGeneratorContext = new ShaderGeneratorContext(graphicsDevice)
                {
                    Parameters = Parameters,
                    ColorSpace = graphicsDevice.ColorSpace
                };

                var newShaderSource = ComputeColor.GenerateShaderSource(shaderGeneratorContext, new MaterialComputeColorKeys(ParticleBaseKeys.EmissiveMap, ParticleBaseKeys.EmissiveValue, Color.White));

                // Check if shader code has changed
                if (!newShaderSource.Equals(shaderSource))
                {
                    shaderSource = newShaderSource;
                    Parameters.Set(ParticleBaseKeys.BaseColor, shaderSource);

                    // TODO: Is this necessary?
                    HasVertexLayoutChanged = true;
                }
            }
        }

        /// <inheritdoc />
        public override void UpdateVertexBuilder(ParticleVertexBuilder vertexBuilder)
        {
            base.UpdateVertexBuilder(vertexBuilder);

            // TODO Part of the graphics improvement XK-3052
            //  Ideally, the whole code here should be extracting information from the ShaderBytecode instead as it is quite unreliable and hacky to extract semantics with text matching.
            //  The arguments we need are in the GenericArguments, which is again just an array of strings
            //  We could search it element by element, but in the end getting the entire string and searching it instead is the same
            {
                var code = shaderSource?.ToString();

                if (code?.Contains("COLOR0") ?? false)
                {
                    vertexBuilder.AddVertexElement(ParticleVertexElements.Color);
                }

                var coordIndex = code?.IndexOf("TEXCOORD", 0, StringComparison.Ordinal) ?? -1;

                if (coordIndex < 0)
                {
                    // If there is no explicit texture coordinate usage, but we can still force it
                    if (ForceTexCoords)
                    {
                        vertexBuilder.AddVertexElement(ParticleVertexElements.TexCoord[0]);
                    }
                }

                while (coordIndex >= 0)
                {
                    var semanticIndex = 0;
                    var subStr = code.Substring(coordIndex + 8);

                    if (int.TryParse(Regex.Match(subStr, @"\d+").Value, out semanticIndex))
                    {
                        semanticIndex = (semanticIndex < 0) ? 0 : semanticIndex;
                        semanticIndex = (semanticIndex > 15) ? 15 : semanticIndex;

                        vertexBuilder.AddVertexElement(ParticleVertexElements.TexCoord[semanticIndex]);
                    }

                    coordIndex = code.IndexOf("TEXCOORD", coordIndex + 1);
                }
            } // Part of the graphics improvement XK-3052

        }

        public override void ValidateEffect(RenderContext context, ref EffectValidator effectValidator)
        {
            base.ValidateEffect(context, ref effectValidator);

            effectValidator.ValidateParameter(ParticleBaseKeys.BaseColor, shaderSource);
        }

        /// <inheritdoc />
        public unsafe override void PatchVertexBuffer(ParticleVertexBuilder vertexBuilder, Vector3 invViewX, Vector3 invViewY, ParticleSorter sorter)
        {
            // If you want, you can implement the base builder here and not call it. It should result in slight speed up
            base.PatchVertexBuffer(vertexBuilder, invViewX, invViewY, sorter);

            //  The UV Builder, if present, animates the basic (0, 0, 1, 1) uv coordinates of each billboard
            UVBuilder?.BuildUVCoordinates(vertexBuilder, sorter, vertexBuilder.DefaultTexCoords);
            vertexBuilder.RestartBuffer();

            // If the particles have color field, the base class should have already passed the information
            if (HasColorField)
                return;

            // If the particles don't have color field but there is no color stream either we don't need to fill anything
            var colAttribute = vertexBuilder.GetAccessor(VertexAttributes.Color);
            if (colAttribute.Size <= 0)
                return;

            // Since the particles don't have their own color field, set the default color to white
            var color = 0xFFFFFFFF;

            // TODO: for loop. Remove IEnumerable from sorter
            foreach (var particle in sorter)
            {
                vertexBuilder.SetAttributePerParticle(colAttribute, (IntPtr)(&color));

                vertexBuilder.NextParticle();
            }

            vertexBuilder.RestartBuffer();
        }

    }
}
