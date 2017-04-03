// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    [DataContract("MaterialEmissiveMapFeature")]
    [Display("Emissive Map")]
    public class MaterialEmissiveMapFeature : MaterialFeature, IMaterialEmissiveFeature, IMaterialStreamProvider
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
        /// <userdoc>The map specifying the color emitted by the material.</userdoc>
        [Display("Emissive Map")]
        [NotNull]
        [DataMember(10)]
        public IComputeColor EmissiveMap { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        /// <userdoc>The map specifying the intensity of the light emitted by the material. This scales the color value specified by emissive map.</userdoc>
        [Display("Intensity")]
        [NotNull]
        [DataMember(20)]
        public IComputeScalar Intensity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the alpha component of the emissive map as main alpha color for the material.
        /// </summary>
        /// <value><c>true</c> if [use alpha]; otherwise, <c>false</c>.</value>
        /// <userdoc>If checked, use the alpha component of the emissive map as main alpha color for the material. Otherwise, ignore it and use the diffuse alpha color.</userdoc>
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

        public override void VisitFeature(MaterialGeneratorContext context)
        {
            Vector4 emissiveMin = Vector4.Zero;
            Vector4 emissiveMax = new Vector4(float.MaxValue);
            EmissiveMap.ClampFloat4(ref emissiveMin, ref emissiveMax);
            Intensity.ClampFloat(0, float.MaxValue);

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