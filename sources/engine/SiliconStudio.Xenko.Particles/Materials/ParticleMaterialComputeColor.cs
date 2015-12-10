using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Assets;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
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
        [Display("Color map")]
        public IComputeColor ComputeColor;

        [DataMemberIgnore]
        private ShaderGeneratorContext shaderGeneratorContext;

        protected override void InitializeCore(RenderContext context)
        {
            base.InitializeCore(context);

            shaderGeneratorContext = new ShaderGeneratorContext();
            ParameterCollections.Add(shaderGeneratorContext.Parameters);

            MandatoryVariation |= ParticleEffectVariation.HasTex0;
        }

        public override void Setup(GraphicsDevice graphicsDevice, RenderContext context, Matrix viewMatrix, Matrix projMatrix, Color4 color)
        {
            base.Setup(graphicsDevice, context, viewMatrix, projMatrix, color);

            ///////////////
            // Shader permutations parameters - shaders will change dynamically based on those parameters
            SetParameter(ParticleBaseKeys.HasTexture, false);


            SetParameter(ParticleBaseKeys.RenderFlagSwizzle, (uint)0);

//            SetParameter(ParticleBaseKeys.ComputeColor0, new ShaderClassSource("ComputeColorBlue"));

            // If particles don't have individual color, we can pass the color tint as part of the uniform color scale


            //            SetParameter(TexturingKeys.Texture0, texture0); // ??

            // var materialContext = new MaterialGeneratorContext(); // Shared for the particle system
            // VisitFeature(materialContext);

            {
                shaderGeneratorContext.Parameters.Clear();

                var shaderSource = ComputeColor.GenerateShaderSource(shaderGeneratorContext, new MaterialComputeColorKeys(MaterialKeys.EmissiveMap, MaterialKeys.EmissiveValue, Color.White));

                shaderGeneratorContext.Parameters.Set(ParticleBaseKeys.BaseColor, shaderSource);
            }

            ApplyEffect(graphicsDevice);
        }



        // THIS IS JUST A COPY FOR MEMO, IT DOESN'T DO ANYTHING
        public static readonly MaterialStreamDescriptor DiffuseStream = new MaterialStreamDescriptor("Diffuse", "matDiffuse", MaterialKeys.DiffuseValue.PropertyType);
        public static readonly MaterialStreamDescriptor ColorBaseStream = new MaterialStreamDescriptor("Color Base", "matColorBase", MaterialKeys.DiffuseValue.PropertyType);

        public void VisitFeature(MaterialGeneratorContext context) // TODO Override
        {
            // Where is it called from ?

            if (ComputeColor != null)
            {
                var computeColorSource = ComputeColor.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceDiffuse"));      // Name of the shader - class MaterialSurfaceDiffuse : IMaterialSurfacePixel
                mixin.AddComposition("diffuseMap", computeColorSource);                 // compose ComputeColor diffuseMap;

                context.UseStream(MaterialShaderStage.Pixel, DiffuseStream.Stream);     // streams.matDiffuse   = colorBase;
                context.UseStream(MaterialShaderStage.Pixel, ColorBaseStream.Stream);   // streams.matColorBase = colorBase;

                context.AddSurfaceShader(MaterialShaderStage.Pixel, mixin);
            }

            // How to compile the shader?
        }

    }
}
