// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.


using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Rendering.Tessellation;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// The displacement map for a surface material feature.
    /// </summary>
    [DataContract("MaterialTesselationFeature")]
    public abstract class MaterialTessellationBaseFeature : MaterialFeature, IMaterialTessellationFeature
    {
        private static readonly PropertyKey<bool> HasFinalCallback = new PropertyKey<bool>("MaterialTessellationBaseFeature.HasFinalCallback", typeof(MaterialTessellationBaseFeature));

        protected MaterialTessellationBaseFeature()
        {
            TriangleSize = 12f;
        }

        /// <summary>
        /// Gets or sets the desired triangle size.
        /// </summary>
        /// <userdoc>
        /// The desired triangles' size in pixels. This drives the tessellation factor.
        /// </userdoc>
        [DataMember(10)]
        [DataMemberRange(1, 100, 1, 5, 2)]
        [Display("Triangle Size")]
        public float TriangleSize { get; set; }

        /// <summary>
        /// Gets or sets the adjacent edges average activation state.
        /// </summary>
        /// <userdoc>
        /// Indicate if average should be performed on adjacent edges to prevent tessellation cracks.
        /// </userdoc>
        [DataMember(20)]
        [Display("Adjacent Edges Average")]
        public bool AdjacentEdgeAverage { get; set; }

        protected bool HasAlreadyTessellationFeature;

        public override void VisitFeature(MaterialGeneratorContext context)
        {
            // determine if an tessellation material have already been added in another layer
            HasAlreadyTessellationFeature = context.GetStreamFinalModifier<MaterialTessellationBaseFeature>(MaterialShaderStage.Domain) != null;

            // Notify problem on multiple tessellation techniques and return
            if (HasAlreadyTessellationFeature)
            {
                context.Log.Warning("A material cannot have more than one layer performing tessellation. The first tessellation method found, will be used.");
                return;
            }

            // reset the tessellation stream at the beginning of the stage
            context.AddStreamInitializer(MaterialShaderStage.Domain, "MaterialTessellationStream");

            // set the desired triangle size desired for this material
            context.Parameters.Set(TessellationKeys.DesiredTriangleSize, TriangleSize);

            // set the tessellation method and callback to add Displacement/Normal average shaders.
            if (AdjacentEdgeAverage && !context.Tags.Get(HasFinalCallback))
            {
                context.Tags.Set(HasFinalCallback, true);
                context.Material.TessellationMethod = XenkoTessellationMethod.AdjacentEdgeAverage;
                context.AddFinalCallback(MaterialShaderStage.Domain, AddAdjacentEdgeAverageMacros);
                context.AddFinalCallback(MaterialShaderStage.Domain, AddAdjacentEdgeAverageShaders);
            }
        }

        public void AddAdjacentEdgeAverageShaders(MaterialShaderStage stage, MaterialGeneratorContext context)
        {
            var tessellationShader = context.Parameters.Get(MaterialKeys.TessellationShader) as ShaderMixinSource;
            if(tessellationShader == null)
                return;

            if (context.GetStreamFinalModifier<MaterialDisplacementMapFeature>(MaterialShaderStage.Domain) != null)
            {
                tessellationShader.Mixins.Add(new ShaderClassSource("TessellationAE2", "TexCoord")); // this suppose Displacement from Texture -> TODO make it more flexible so that it works with any kind of displacement.
                tessellationShader.Mixins.Add(new ShaderClassSource("TessellationAE3", "normalWS"));
            }
        }

        public void AddAdjacentEdgeAverageMacros(MaterialShaderStage stage, MaterialGeneratorContext context)
        {
            var tessellationShader = context.Parameters.Get(MaterialKeys.TessellationShader) as ShaderMixinSource;
            if(tessellationShader == null)
                return;

            tessellationShader.Macros.Add(new ShaderMacro("InputControlPointCount", 12));
        }
    }
}
