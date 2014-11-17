// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Assets.Effect;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;

namespace SiliconStudio.Paradox.Assets.Model
{
    [DataContract("MeshMaterialParameters")]
    public class MeshMaterialParameters
    {
        public MeshMaterialParameters()
        {
            Parameters = new ParameterCollectionData();
            CastShadows = false;
            ReceiveShadows = false;
            Layer = RenderLayers.RenderLayerAll;
        }

        /// <summary>
        /// The name of the node the mesh is attached to.
        /// </summary>
        /// <userdoc>
        /// The node the mesh is linked to in the hierarchy.
        /// </userdoc>
        [DataMember(10)]
        public string NodeName;

        /// <summary>
        /// The reference to the material.
        /// </summary>
        /// <userdoc>
        /// The material the mesh uses.
        /// </userdoc>
        [DataMember(20)]
        public AssetReference<MaterialAsset> Material;
        
        /// <summary>
        /// The mesh casts shadow.
        /// </summary>
        /// <userdoc>
        /// If checked, the mesh will cast shadows.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(false)]
        public bool CastShadows;

        /// <summary>
        /// The mesh receives shadow.
        /// </summary>
        /// <userdoc>
        /// If checked, the mesh will receive shadows.
        /// </userdoc>
        [DataMember(40)]
        [DefaultValue(false)]
        public bool ReceiveShadows;

        /// <summary>
        /// The layer of the mesh.
        /// </summary>
        /// <userdoc>
        /// The layer the mesh belongs to.
        /// </userdoc>
        [DataMember(50)]
        [DefaultValue(RenderLayers.RenderLayerAll)]
        public RenderLayers Layer;

        /// <summary>
        /// The mesh parameters.
        /// </summary>
        /// <userdoc>
        /// The mesh-specific parameters. This will override the material parameters.
        /// </userdoc>
        [DataMember(60)]
        public ParameterCollectionData Parameters;

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