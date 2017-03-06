using SiliconStudio.Core.Annotations;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Internal
{
    /// <summary>
    /// An interface exposing internal methods of <see cref="IAssetNode"/>
    /// </summary>
    internal interface IAssetNodeInternal : IAssetNode
    {
        /// <summary>
        /// Gets or sets whether the override properties of this node are currently being reset.
        /// </summary>
        bool ResettingOverride { get; set; }

        /// <summary>
        /// Sets the <see cref="AssetPropertyGraph"/> of the asset related to this node.
        /// </summary>
        /// <param name="assetPropertyGraph"></param>
        void SetPropertyGraph([NotNull] AssetPropertyGraph assetPropertyGraph);

        /// <summary>
        /// Gets the base of this node.
        /// </summary>
        /// <param name="node">The base node to set.</param>
        void SetBaseNode(IGraphNode node);
    }
}
