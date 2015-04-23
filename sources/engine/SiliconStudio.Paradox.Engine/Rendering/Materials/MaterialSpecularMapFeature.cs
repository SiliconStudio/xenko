// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// A Specular map for the specular material feature.
    /// </summary>
    [DataContract("MaterialSpecularMapFeature")]
    [Display("Specular Map")]
    public class MaterialSpecularMapFeature : IMaterialSpecularFeature, IMaterialStreamProvider
    {
        private static readonly MaterialStreamDescriptor SpecularStream = new MaterialStreamDescriptor("Specular", "matSpecular", MaterialKeys.SpecularValue.PropertyType);

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialSpecularMapFeature"/> class.
        /// </summary>
        public MaterialSpecularMapFeature()
        {
            SpecularMap = new ComputeTextureColor();
            Intensity = new ComputeFloat(1.0f);
            IsEnergyConservative = true;
        }

        /// <summary>
        /// Gets or sets the specular map.
        /// </summary>
        /// <value>The specular map.</value>
        [DataMember(10)]
        [Display("Specular Map")]
        [NotNull]
        public IComputeColor SpecularMap { get; set; }

        /// <summary>
        /// Gets or sets the specular intensity.
        /// </summary>
        /// <value>The intensity.</value>
        [DataMember(20)]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public IComputeScalar Intensity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is energy conservative.
        /// </summary>
        /// <value><c>true</c> if this instance is energy conservative; otherwise, <c>false</c>.</value>
        [DataMember(30)]
        [DefaultValue(true)]
        [Display("Is Energy Conservative?")]
        public bool IsEnergyConservative { get; set; }

        public void Visit(MaterialGeneratorContext context)
        {
            context.SetStream(SpecularStream.Stream, SpecularMap, MaterialKeys.SpecularMap, MaterialKeys.SpecularValue);
            context.SetStream("matSpecularIntensity", Intensity, null, MaterialKeys.SpecularIntensityValue);
        }

        public IEnumerable<MaterialStreamDescriptor> GetStreams()
        {
            yield return SpecularStream;
        }
    }
}