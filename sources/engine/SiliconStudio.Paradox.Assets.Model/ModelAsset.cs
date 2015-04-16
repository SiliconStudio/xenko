// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Assets.ProceduralModels;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Model
{
    [DataContract("Model")]
    [AssetFileExtension(FileExtension)]
    [AssetCompiler(typeof(ModelAssetCompiler))]
    [ThumbnailCompiler(PreviewerCompilerNames.ModelThumbnailCompilerQualifiedName, true, Priority = 10000)]
    [Display("Model", "A 3D model")]
    [AssetFormatVersion(AssetFormatVersion, null)]
    public sealed class ModelAsset : AssetImportTracked, IModelAsset
    {
        public const int AssetFormatVersion = 1;

        /// <summary>
        /// The default file extension used by the <see cref="ModelAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxm3d";

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelAsset"/> class.
        /// </summary>
        public ModelAsset()
        {
            SerializedVersion = AssetFormatVersion;
            ScaleImport = 0.01f;
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
        [DefaultValue(0.01f)]
        public float ScaleImport { get; set; }

        /// <summary>
        /// The materials.
        /// </summary>
        /// <userdoc>
        /// The list of materials in the model.
        /// </userdoc>
        [DataMember(40)]
        public List<ModelMaterial> Materials { get; private set; }

        /// <summary>
        /// List that stores if a node should be preserved
        /// </summary>
        /// <userdoc>
        /// The nodes of the model.
        /// </userdoc>
        [DataMember(50), DiffUseAsset2]
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

        //class Upgrader : AssetUpgraderBase
        //{
        //    protected override void UpgradeAsset(ILogger log, dynamic asset)
        //    {
        //        foreach (var keyValue in asset.MeshParameters)
        //        {
        //            var parameters = asset.MeshParameters[keyValue.Key].Parameters["~Items"];
        //            parameters.Node.Style = YamlStyle.Block;

        //            MoveToParameters(asset, parameters, keyValue.Key, "CastShadows", LightingKeys.CastShadows);
        //            MoveToParameters(asset, parameters, keyValue.Key, "ReceiveShadows", LightingKeys.ReceiveShadows);
        //            MoveToParameters(asset, parameters, keyValue.Key, "Layer", RenderingParameters.EntityGroup);
        //        }

        //        // Get the Model, and generate an Id if the previous one wasn't the empty one
        //        var emptyGuid = Guid.Empty.ToString().ToLowerInvariant();
        //        var id = asset.Id;
        //        if (id != null && id.Node.Value != emptyGuid)
        //            asset.Id = Guid.NewGuid().ToString().ToLowerInvariant();

        //        // Bump asset version -- make sure it is stored right after Id
        //        asset.SerializedVersion = AssetFormatVersion;
        //        asset.MoveChild("SerializedVersion", asset.IndexOf("Id") + 1);
        //    }

        //    public void MoveToParameters(dynamic asset, dynamic parameters, object key, string paramName, ParameterKey pk)
        //    {
        //        var paramValue = asset.MeshParameters[key][paramName];
        //        if (paramValue != null)
        //        {
        //            parameters[pk.Name] = paramValue;
        //            asset.MeshParameters[key].RemoveChild(paramName);
        //        }
        //    }
        //}
    }
}