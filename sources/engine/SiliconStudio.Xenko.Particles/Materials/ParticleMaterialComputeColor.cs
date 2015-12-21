// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Assets;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("ParticleMaterialComputeColor")]
    [Display("DynamicColor")]
    public class ParticleMaterialComputeColor : ParticleMaterialSimple
    {
        [DataMemberIgnore]
        protected override string EffectName { get; set; } = "ParticleEffect";

        [DataMember(100)]
        [Display("Emissive")]
        public IComputeColor ComputeColor { get; set; } = new ComputeTextureColor();

        //[DataMember(110)]
        //[Display("Intensity")]
        //public IComputeScalar ComputeIntensity { get; set; } = new ComputeFloat();

        [DataMember(200)]
        [Display("Texture coordinates")]
        public UVBuilderBase UVBuilder;

        [DataMemberIgnore]
        private ShaderGeneratorContext shaderGeneratorContext;

        protected override void InitializeCore(RenderContext context)
        {
            base.InitializeCore(context);

            UpdateShaders();
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

                var shaderText = shaderBaseColor.ToString();

                shaderGeneratorContext.Parameters.Set(ParticleBaseKeys.BaseColor, shaderBaseColor);
            }

            //if (ComputeIntensity != null)
            //{
            //    var shaderBaseIntensity = ComputeIntensity.GenerateShaderSource(shaderGeneratorContext,
            //        new MaterialComputeColorKeys(ParticleBaseKeys.IntensityMap, ParticleBaseKeys.IntensityValue, Color.White));
            //    shaderGeneratorContext.Parameters.Set(ParticleBaseKeys.BaseIntensity, shaderBaseIntensity);
            //}
        }

        public override void Setup(GraphicsDevice graphicsDevice, RenderContext context, Matrix viewMatrix, Matrix projMatrix, Color4 color)
        {
            base.Setup(graphicsDevice, context, viewMatrix, projMatrix, color);
            
            UpdateShaders();
        }


        public override void PatchVertexBuffer(ParticleVertexBuilder vertexBuilder, Vector3 invViewX, Vector3 invViewY, ParticleSorter sorter)
        {
            // If you want, you can integrate the base builder here and not call it. It should result in slight speed up
            base.PatchVertexBuffer(vertexBuilder, invViewX, invViewY, sorter);

            UVBuilder?.BuildUVCoordinates(vertexBuilder, sorter);
        }

    }
}
