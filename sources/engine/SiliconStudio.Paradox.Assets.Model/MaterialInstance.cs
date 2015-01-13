// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Assets.Effect;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;

namespace SiliconStudio.Paradox.Assets.Model
{
    [DataContract("ModelMaterialParameters")]
    [Obsolete]
    public class MaterialInstance
    {
        public MaterialInstance()
        {
            Parameters = new ParameterCollection();
        }

        /// <summary>
        /// The reference to the material.
        /// </summary>
        /// <userdoc>
        /// The material the mesh uses.
        /// </userdoc>
        [DataMember(20)]
        public AssetReference<MaterialAsset> Material;

        /// <summary>
        /// The mesh parameters.
        /// </summary>
        /// <userdoc>
        /// The mesh-specific parameters. This will override the material parameters.
        /// </userdoc>
        [DataMember(60)]
        public ParameterCollection Parameters;

        /// <summary>
        /// The light permutation parameters
        /// </summary>
        /// <userdoc>
        /// The lighting configurations the mesh supports.
        /// </userdoc>
        [DataMember(70)]
        [DefaultValue(null)]
        public AssetReference<LightingAsset> LightingParameters;
    }
}