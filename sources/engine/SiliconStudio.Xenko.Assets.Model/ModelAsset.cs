// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SharpYaml.Serialization;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Model
{
    [DataContract("Model")]
    [AssetDescription(FileExtension, false)]
    [AssetCompiler(typeof(ModelAssetCompiler))]
    [Display(190, "Model")]
    [AssetFormatVersion(XenkoConfig.PackageName, "1.4.0-beta")]
    [AssetUpgrader(XenkoConfig.PackageName, 0, 2, typeof(Upgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.2", "1.4.0-beta", typeof(EmptyAssetUpgrader))]
    public sealed class ModelAsset : AssetImportTracked, IModelAsset
    {
        /// <summary>
        /// The default file extension used by the <see cref="ModelAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkm3d;pdxm3d";

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelAsset"/> class.
        /// </summary>
        public ModelAsset()
        {
            ScaleImport = 1.0f;
            Materials = new List<ModelMaterial>();
            Nodes = new List<NodeInformation>();
            SetDefaults();
        }

        /// <summary>
        /// Gets or sets the scale import.
        /// </summary>
        /// <value>The scale import.</value>
        /// <userdoc>The scale applied when importing a model.</userdoc>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float ScaleImport { get; set; }

        /// <summary>
        /// The materials.
        /// </summary>
        /// <userdoc>
        /// The list of materials in the model.
        /// </userdoc>
        [DataMember(40)]
        [MemberCollection(ReadOnly = true)]
        public List<ModelMaterial> Materials { get; private set; }

        /// <summary>
        /// List that stores if a node should be preserved
        /// </summary>
        /// <userdoc>
        /// The mesh nodes of the model.
        /// When checked, the nodes are kept in the runtime version of the model. 
        /// Otherwise, all the meshes of model are merged and the node information is lost.
        /// Nodes should be preserved in order to be animated or linked to entities.
        /// </userdoc>
        [DataMember(50), DiffMember(Diff3ChangeType.MergeFromAsset2)]
        public List<NodeInformation> Nodes { get; private set; }

        /// <summary>
        /// Gets or sets if the mesh will be compacted (meshes will be merged).
        /// </summary>
        [DataMemberIgnore]
        public bool Compact
        {
            get
            {
                return Nodes.Any(x => !x.Preserve);
            }
        }

        protected override int InternalBuildOrder
        {
            get { return -100; } // We want Model to be scheduled early since they tend to take the longest (bad concurrency at end of build)
        }

        /// <summary>
        /// Returns to list of nodes that are preserved (they cannot be merged with other ones).
        /// </summary>
        /// <userdoc>
        /// Checking nodes will garantee them to be available at runtime. Otherwise, it may be merged with their parents (for optimization purposes).
        /// </userdoc>
        [DataMemberIgnore]
        public List<string> PreservedNodes
        {
            get
            {
                return Nodes.Where(x => x.Preserve).Select(x => x.Name).ToList();
            }
        }

        /// <inheritdoc/>
        [DataMemberIgnore]
        public IEnumerable<KeyValuePair<string, MaterialInstance>> MaterialInstances { get { return Materials.Select(x => new KeyValuePair<string, MaterialInstance>(x.Name, x.MaterialInstance)); } }
        
        /// <summary>
        /// Preserve the nodes.
        /// </summary>
        /// <param name="nodesToPreserve">List of nodes to preserve.</param>
        public void PreserveNodes(List<string> nodesToPreserve)
        {
            foreach (var nodeName in nodesToPreserve)
            {
                foreach (var node in Nodes)
                {
                    if (node.Name.Equals(nodeName))
                        node.Preserve = true;
                }
            }
        }

        /// <summary>
        /// No longer preserve any node.
        /// </summary>
        public void PreserveNoNode()
        {
            foreach (var node in Nodes)
                node.Preserve = false;
        }

        /// <summary>
        /// Preserve all the nodes.
        /// </summary>
        public void PreserveAllNodes()
        {
            foreach (var node in Nodes)
                node.Preserve = true;
        }

        /// <summary>
        /// Invert the preservation of the nodes.
        /// </summary>
        public void InvertPreservation()
        {
            foreach (var node in Nodes)
                node.Preserve = !node.Preserve;
        }

        public override void SetDefaults()
        {
            if (Nodes != null)
                Nodes.Clear();
        }

        class Upgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                foreach (var modelMaterial in asset.Materials)
                {
                    var material = modelMaterial.Material;
                    if (material != null)
                    {
                        modelMaterial.MaterialInstance = new YamlMappingNode();
                        modelMaterial.MaterialInstance.Material = material;
                        modelMaterial.Material = DynamicYamlEmpty.Default;
                    }
                }
            }
        }
    }
}