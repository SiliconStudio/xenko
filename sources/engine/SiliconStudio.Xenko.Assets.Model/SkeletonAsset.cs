using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.Assets.Model
{
    [DataContract("Skeleton")]
    [AssetDescription(FileExtension)]
    //[AssetCompiler(typeof(SkeletonAssetCompiler))]
    //[ObjectFactory(typeof(SkeletonFactory))]
    [Display(180, "Skeleton", "A skeleton (node hierarchy)")]
    public class SkeletonAsset : AssetImportTracked
    {
        /// <summary>
        /// The default file extension used by the <see cref="SkeletonAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkskel";

        /// <summary>
        /// List that stores if a node should be preserved
        /// </summary>
        /// <userdoc>
        /// The mesh nodes of the model.
        /// When checked, the nodes are kept in the runtime version of the model. 
        /// Otherwise, all the meshes of model are merged and the node information is lost.
        /// Nodes should be preserved in order to be animated or linked to entities.
        /// </userdoc>
        [DataMember(10), DiffMember(Diff3ChangeType.MergeFromAsset2)]
        public List<NodeInformation> Nodes { get; } = new List<NodeInformation>();

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
        public List<string> PreservedNodes
        {
            get
            {
                return Nodes.Where(x => x.Preserve).Select(x => x.Name).ToList();
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

        public override void SetDefaults()
        {
            if (Nodes != null)
                Nodes.Clear();
        }
    }
}