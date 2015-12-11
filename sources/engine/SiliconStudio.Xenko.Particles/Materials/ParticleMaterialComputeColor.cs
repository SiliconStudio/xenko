using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Assets;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("ParticleMaterialComputeColor")]
    [Display("DynamicColor")]
    public class ParticleMaterialComputeColor : ParticleMaterialBase
    {
        [DataMemberIgnore]
        protected override string EffectName { get; set; } = "ParticleBatch";

        [DataMember(100)]
        [Display("Emissive")]
        public IComputeColor ComputeColor { get; set; } = new ComputeTextureColor();

        [DataMember(110)]
        [Display("Intensity")]
        public IComputeScalar ComputeIntensity { get; set; } = new ComputeFloat();

        // [DataMember(130)]
        // Texture Coordinates builder - fixed, scroll and flipbook

        [DataMember(200)]
        [Display("Texture coordinates")]
        public UVBuilderBase UVBuilder;

        [DataMemberIgnore]
        private ShaderGeneratorContext shaderGeneratorContext;

        protected override void InitializeCore(RenderContext context)
        {
            base.InitializeCore(context);

            UpdateShaders();

            // TODO Change - part of the LayoutBuilder refactoring
            MandatoryVariation |= ParticleEffectVariation.HasTex0;
        }

        private int shadersUpdateCounter = 0;
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
            }

            if (ComputeIntensity != null)
            {
                var shaderBaseIntensity = ComputeIntensity.GenerateShaderSource(shaderGeneratorContext,
                    new MaterialComputeColorKeys(ParticleBaseKeys.IntensityMap, ParticleBaseKeys.IntensityValue, Color.White));
                shaderGeneratorContext.Parameters.Set(ParticleBaseKeys.BaseIntensity, shaderBaseIntensity);
            }
        }

        public override void Setup(GraphicsDevice graphicsDevice, RenderContext context, Matrix viewMatrix, Matrix projMatrix, Color4 color)
        {
            base.Setup(graphicsDevice, context, viewMatrix, projMatrix, color);
            
            UpdateShaders();

            ApplyEffect(graphicsDevice);
        }


        public override void PatchVertexBuffer(ParticleVertexLayout vtxBuilder, Vector3 invViewX, Vector3 invViewY, int maxVertices, ParticlePool pool)
        {
            // If you want, you can integrate the base builder here and not call it. It should result in slight speed up
            base.PatchVertexBuffer(vtxBuilder, invViewX, invViewY, maxVertices, pool);

            if (UVBuilder != null)
            {
                UVBuilder.BuildUVCoordinates(vtxBuilder, pool);
            }
        }

    }
    }
