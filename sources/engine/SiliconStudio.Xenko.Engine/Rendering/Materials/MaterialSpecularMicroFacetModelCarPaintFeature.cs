// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// The microfacet specular shading model.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetModelCarPaintFeature")]
    [Display("Car Paint Microfacet")]
    public class MaterialSpecularMicrofacetModelCarPaintFeature : MaterialSpecularMicrofacetModelFeature
    {
        /// <userdoc>Specify the function to use to calculate the Fresnel component of the micro-facet lighting equation. 
        /// This defines the amount of the incoming light that is reflected.</userdoc>
        [DataMember(10)]
        [Display("Fresnel")]
        [NotNull]
        public IMaterialSpecularMicrofacetFresnelFunction Fresnel { get; set; } = new MaterialSpecularMicrofacetFresnelSchlick();

        /// <userdoc>Specify the function to use to calculate the visibility component of the micro-facet lighting equation.</userdoc>
        [DataMember(20)]
        [Display("Visibility")]
        [NotNull]
        public IMaterialSpecularMicrofacetVisibilityFunction Visibility { get; set; } = new MaterialSpecularMicrofacetVisibilitySmithSchlickGGX();

        /// <userdoc>Specify the function to use to calculate the normal distribution in the micro-facet lighting equation. 
        /// This defines how the normal is distributed.</userdoc>
        [DataMember(30)]
        [Display("Normal Distribution")]
        [NotNull]
        public IMaterialSpecularMicrofacetNormalDistributionFunction NormalDistribution { get; set; } = new MaterialSpecularMicrofacetNormalDistributionGGX();

        /// <userdoc>Specify the function to use to calculate the environment DFG term in the micro-facet lighting equation. 
        /// This defines how the material reflects specular cubemaps.</userdoc>
        [DataMember(30)]
        [Display("Environment (DFG)")]
        [NotNull]
        public IMaterialSpecularMicrofacetEnvironmentFunction Environment { get; set; } = new MaterialSpecularMicrofacetEnvironmentGGXLUT();

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            var isMetalFlakesPass = context.PassIndex == 0;

            if (isMetalFlakesPass)
            {
                base.GenerateShader(context);
                return;
            }

            var shaderSource = new ShaderMixinSource();
            shaderSource.Mixins.Add(new ShaderClassSource("MaterialSurfaceShadingSpecularMicrofacetCarPaint"));
            
            var shaderBuilder = context.AddShading(this);
            shaderBuilder.LightDependentSurface = shaderSource;
        }
    }
}
