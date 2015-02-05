// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects.Materials;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// A Diffuse map for the diffuse material feature.
    /// </summary>
    [DataContract("MaterialDiffuseMapFeature")]
    [Display("Diffuse Map")]
    public class MaterialDiffuseMapFeature : IMaterialDiffuseFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialDiffuseMapFeature"/> class.
        /// </summary>
        public MaterialDiffuseMapFeature()
        {
            DiffuseMap = new ComputeTextureColor();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialDiffuseMapFeature"/> class.
        /// </summary>
        /// <param name="diffuseMap">The diffuse map.</param>
        public MaterialDiffuseMapFeature(IComputeColor diffuseMap)
        {
            if (diffuseMap == null) throw new ArgumentNullException("diffuseMap");
            DiffuseMap = diffuseMap;
        }

        /// <summary>
        /// Gets or sets the diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        [Display("Diffuse Map")]
        [NotNull]
        [DataMemberCustomSerializer]
        public IComputeColor DiffuseMap { get; set; }

        public void Visit(MaterialGeneratorContext context)
        {
            context.SetStream("matDiffuse", DiffuseMap, MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White);
        }
    }
}