// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A Diffuse map for the diffuse material feature.
    /// </summary>
    [DataContract("MaterialCelShadingMapFeature")]
    [Display("Cel Shading Map")]
    public class MaterialCelShadingMapFeature : MaterialFeature, IMaterialDiffuseFeature, IMaterialStreamProvider
    {
        public static readonly MaterialStreamDescriptor CelRampStream = new MaterialStreamDescriptor("CelRamp", "matCelRamp", MaterialKeys.DiffuseValue.PropertyType);
        public static readonly MaterialStreamDescriptor DiffuseStream = new MaterialStreamDescriptor("Diffuse", "matDiffuse", MaterialKeys.DiffuseValue.PropertyType);
        public static readonly MaterialStreamDescriptor ColorBaseStream = new MaterialStreamDescriptor("Color Base", "matColorBase", MaterialKeys.DiffuseValue.PropertyType);

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialDiffuseMapFeature"/> class.
        /// </summary>
        public MaterialCelShadingMapFeature()
        {
            DiffuseMap = new ComputeTextureColor();
            CelRampMap = new ComputeTextureColor();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialDiffuseMapFeature"/> class.
        /// </summary>
        /// <param name="diffuseMap">The diffuse map.</param>
        public MaterialCelShadingMapFeature(IComputeColor diffuseMap, IComputeColor celRampMap)
        {
            if (diffuseMap == null) throw new ArgumentNullException(nameof(diffuseMap));
            if (celRampMap == null) throw new ArgumentNullException(nameof(celRampMap));
            DiffuseMap = diffuseMap;
            CelRampMap = celRampMap;
        }

        /// <summary>
        /// Gets or sets the diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        [Display("Diffuse Map")]
        [NotNull]
        [DataMemberCustomSerializer]
        public IComputeColor DiffuseMap { get; set; }

        /// <summary>
        /// Gets or sets the cel ramp map.
        /// </summary>
        /// <value>The cel ramp map.</value>
        [Display("Ramp Map")]
        [NotNull]
        [DataMemberCustomSerializer]
        public IComputeColor CelRampMap { get; set; }

        public override void VisitFeature(MaterialGeneratorContext context)
        {
            if (DiffuseMap != null)
            {
                var computeColorSourceDiffuse = DiffuseMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));
                var computeColorSourceCelRamp = DiffuseMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceCelShader"));
                mixin.AddComposition("diffuseMap", computeColorSourceDiffuse);
                mixin.AddComposition("celRampMap", computeColorSourceCelRamp);

                context.UseStream(MaterialShaderStage.Pixel, DiffuseStream.Stream);
                context.UseStream(MaterialShaderStage.Pixel, CelRampStream.Stream);
                context.UseStream(MaterialShaderStage.Pixel, ColorBaseStream.Stream);
                context.AddShaderSource(MaterialShaderStage.Pixel, mixin);
            }
        }

        public IEnumerable<MaterialStreamDescriptor> GetStreams()
        {
            yield return ColorBaseStream;
            yield return DiffuseStream;
            yield return CelRampStream;
        }
    }
}