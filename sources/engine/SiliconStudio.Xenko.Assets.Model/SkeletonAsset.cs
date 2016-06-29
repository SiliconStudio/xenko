using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Xenko.Assets.Model
{
    [DataContract("Skeleton")]
    [AssetDescription(FileExtension, AllowArchetype = false)]
    [AssetCompiler(typeof(SkeletonAssetCompiler))]
    [Display(180, "Skeleton", "A skeleton (node hierarchy)")]
    public class SkeletonAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="SkeletonAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkskel";

        /// <summary>
        /// Gets or sets the source file of this asset.
        /// </summary>
        /// <value>The source.</value>
        /// <userdoc>
        /// The source file of this asset.
        /// </userdoc>
        [DataMember(-50)]
        [DefaultValue(null)]
        [SourceFileMember(true)]
        public UFile Source { get; set; } = new UFile("");

        /// <summary>
        /// Gets or sets the scale import.
        /// </summary>
        /// <value>The scale import.</value>
        /// <userdoc>The scale applied when importing a model.</userdoc>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float ScaleImport { get; set; } = 1.0f;

        /// <summary>
        /// List that stores if a node should be preserved
        /// </summary>
        /// <userdoc>
        /// The mesh nodes of the model.
        /// When checked, the nodes are kept in the runtime version of the model. 
        /// Otherwise, all the meshes of model are merged and the node information is lost.
        /// Nodes should be preserved in order to be animated or linked to entities.
        /// </userdoc>
        [DataMember(20), DiffMember(Diff3ChangeType.MergeFromAsset2)]
        public List<NodeInformation> Nodes { get; } = new List<NodeInformation>();

        [DataMemberIgnore]
        public override UFile MainSource => Source;

        protected override int InternalBuildOrder => -200; // We want Model to be scheduled early since they tend to take the longest (bad concurrency at end of build)

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

        /// <summary>
        /// Returns to list of nodes that are preserved (they cannot be merged with other ones).
        /// </summary>
        /// <userdoc>
        /// Checking nodes will garantee them to be available at runtime. Otherwise, it may be merged with their parents (for optimization purposes).
        /// </userdoc>
        [DataMemberIgnore]
        public List<KeyValuePair<string, bool>> NodesWithPreserveInfo
        {
            get
            {
                return Nodes.Select(x => new KeyValuePair<string, bool>(x.Name, x.Preserve)).ToList();
            }
        }

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
    }
}
