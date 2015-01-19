// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    [DataContract("MaterialEmissiveMapFeature")]
    [Display("Emissive Map")]
    public class MaterialEmissiveMapFeature : IMaterialEmissiveFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialEmissiveMapFeature"/> class.
        /// </summary>
        public MaterialEmissiveMapFeature() : this(new MaterialTextureComputeColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialEmissiveMapFeature"/> class.
        /// </summary>
        /// <param name="emissiveMap">The emissive map.</param>
        /// <exception cref="System.ArgumentNullException">emissiveMap</exception>
        public MaterialEmissiveMapFeature(IMaterialComputeColor emissiveMap)
        {
            if (emissiveMap == null) throw new ArgumentNullException("emissiveMap");
            EmissiveMap = emissiveMap;
            Intensity = new MaterialFloatComputeNode(1.0f);
        }

        /// <summary>
        /// Gets or sets the diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        [Display("Emissive Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public IMaterialComputeColor EmissiveMap { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        [Display("Intensity")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public IMaterialComputeScalar Intensity { get; set; }

        public bool IsLightDependent
        {
            get
            {
                return false;
            }
        }

        public void Visit(MaterialGeneratorContext context)
        {
            context.SetStream("matEmissive", EmissiveMap, MaterialKeys.EmissiveMap, MaterialKeys.EmissiveValue);
            context.SetStream("matEmissiveIntensity", Intensity, null, MaterialKeys.EmissiveIntensity);
            // TODO: Add shading model
            context.AddShading(this, new ShaderClassSource("TODOEmissive"));
        }

        public bool Equals(IMaterialShadingModelFeature other)
        {
            return other is MaterialEmissiveMapFeature;
        }
    }
}