// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A Metalness map for the specular material feature.
    /// </summary>
    [DataContract("MaterialMetalnessMapFeature")]
    [Display("Metalness Map")]
    public class MaterialMetalnessMapFeature : MaterialFeature, IMaterialSpecularFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialMetalnessMapFeature"/> class.
        /// </summary>
        public MaterialMetalnessMapFeature()
        {
            MetalnessMap = new ComputeTextureScalar();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialMetalnessMapFeature"/> class.
        /// </summary>
        /// <param name="metalnessMap">The metalness map.</param>
        public MaterialMetalnessMapFeature(IComputeScalar metalnessMap)
        {
            MetalnessMap = metalnessMap;
        }

        /// <summary>
        /// Gets or sets the metalness map.
        /// </summary>
        /// <value>The metalness map.</value>
        /// <userdoc>The map specifying the metalness of the material.</userdoc>
        [Display("Metalness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public IComputeScalar MetalnessMap { get; set; }

        public override void VisitFeature(MaterialGeneratorContext context)
        {
            if (MetalnessMap != null)
            {
                var computeColorSource = MetalnessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.MetalnessMap, MaterialKeys.MetalnessValue));
                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceMetalness"));
                mixin.AddComposition("metalnessMap", computeColorSource);
                context.UseStream(MaterialShaderStage.Pixel, "matSpecular");
                context.AddShaderSource(MaterialShaderStage.Pixel, mixin);
            }
        }
    }
}