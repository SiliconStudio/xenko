// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Rendering.Materials.ComputeColors;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    [DataContract("MaterialEmissiveMapFeature")]
    [Display("Emissive Map")]
    public class MaterialEmissiveMapFeature : IMaterialEmissiveFeature, IMaterialStreamProvider
    {
        private static readonly MaterialStreamDescriptor EmissiveStream = new MaterialStreamDescriptor("Emissive", "matEmissive", MaterialKeys.EmissiveValue.PropertyType);

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialEmissiveMapFeature"/> class.
        /// </summary>
        public MaterialEmissiveMapFeature() : this(new ComputeTextureColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialEmissiveMapFeature"/> class.
        /// </summary>
        /// <param name="emissiveMap">The emissive map.</param>
        /// <exception cref="System.ArgumentNullException">emissiveMap</exception>
        public MaterialEmissiveMapFeature(IComputeColor emissiveMap)
        {
            if (emissiveMap == null) throw new ArgumentNullException("emissiveMap");
            EmissiveMap = emissiveMap;
            Intensity = new ComputeFloat(1.0f);
            UseAlpha = false;
        }

        /// <summary>
        /// Gets or sets the diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        [Display("Emissive Map")]
        [NotNull]
        [DataMember(10)]
        public IComputeColor EmissiveMap { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        [Display("Intensity")]
        [NotNull]
        [DataMember(20)]
        public IComputeScalar Intensity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use alpha].
        /// </summary>
        /// <value><c>true</c> if [use alpha]; otherwise, <c>false</c>.</value>
        [DataMember(30)]
        [DefaultValue(false)]
        public bool UseAlpha { get; set; }

        public bool IsLightDependent
        {
            get
            {
                return false;
            }
        }

        public void Visit(MaterialGeneratorContext context)
        {
            context.SetStream(EmissiveStream.Stream, EmissiveMap, MaterialKeys.EmissiveMap, MaterialKeys.EmissiveValue);
            context.SetStream("matEmissiveIntensity", Intensity, MaterialKeys.EmissiveIntensityMap, MaterialKeys.EmissiveIntensity);

            context.AddShading(this, new ShaderClassSource("MaterialSurfaceEmissiveShading", UseAlpha));
        }

        public bool Equals(IMaterialShadingModelFeature other)
        {
            return other is MaterialEmissiveMapFeature;
        }

        public IEnumerable<MaterialStreamDescriptor> GetStreams()
        {
            yield return EmissiveStream;
        }
    }
}