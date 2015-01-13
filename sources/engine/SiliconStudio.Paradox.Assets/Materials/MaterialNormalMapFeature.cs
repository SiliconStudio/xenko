// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// The normal map for a surface material feature.
    /// </summary>
    [DataContract("MaterialNormalMapFeature")]
    [Display("Normal Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialNormalMapFeature : IMaterialSurfaceFeature
    {
        /// <summary>
        /// Gets or sets the normal map.
        /// </summary>
        /// <value>The normal map.</value>
        [Display("Normal Map")]
        [DefaultValue(null)]
        public MaterialComputeColor NormalMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the normal is only stored in XY components and Z is assumed to be 1.0.
        /// </summary>
        /// <value><c>true</c> if this instance is xy normal; otherwise, <c>false</c>.</value>
        /// TODO: We could use an enum as we could have other normal encoding, but for now, assume that we only have [xyz] and [xy1]
        [DefaultValue(false)]
        public bool IsXYNormal { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialNormalMapFeature()
                {
                    NormalMap = new MaterialTextureComputeColor() // TODO: handle xy vs xyz
                };
            }
        }

        public void Visit(MaterialGeneratorContext context)
        {
            if (NormalMap != null)
            {
                // Inform the context that we are using matNormal (from the MaterialSurfaceNormalMap shader)
                context.UseStream("matNormal");

                var computeColorSource = NormalMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.NormalMap, MaterialKeys.NormalValue));
                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceNormalMap", IsXYNormal));
                mixin.AddComposition("normalMap", computeColorSource);
                context.AddSurfaceShader(mixin);
            }
        }
    }
}