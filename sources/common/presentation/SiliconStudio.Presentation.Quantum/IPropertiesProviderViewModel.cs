using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// An interface representing an view model that can provide properties to build an <see cref="GraphViewModel"/>.
    /// </summary>
    public interface IPropertiesProviderViewModel
    {
        /// <summary>
        /// Gets whether this view model is currently able to provide properties.
        /// </summary>
        bool CanProvidePropertiesViewModel { get; }

        /// <summary>
        /// Retrieves the root <see cref="IGraphNode"/> to use to generate properties.
        /// </summary>
        /// <returns>The root <see cref="IGraphNode"/> to use to generate properties.</returns>
        IObjectNode GetRootNode();

        /// <summary>
        /// Indicates whether the children of the view model corresponding to the given node and index should be constructed.
        /// </summary>
        /// <param name="graphNode">The node of the view model to evaluate.</param>
        /// <param name="index">The index of the view model to evaluate.</param>
        /// <returns><see cref="ExpandReferencePolicy.Full"/> or <see cref="ExpandReferencePolicy.Partial"/> if respectively all or some of the members of the referenced node should be constructed, <see cref="ExpandReferencePolicy.None"/> otherwise.</returns>
        ExpandReferencePolicy ShouldConstructChildren(IGraphNode graphNode, Index index);

        /// <summary>
        /// Indicates whether the view model of a specific member should be constructed.
        /// </summary>
        /// <param name="member">The member to evaluate.</param>
        /// <param name="expandReferencePolicy">The result from previous call to <see cref="ShouldConstructChildren"/>.</param>
        /// <returns><c>True</c> if the member node should be constructed, <c>False</c> otherwise.</returns>
        bool ShouldConstructMember(IMemberNode member, ExpandReferencePolicy expandReferencePolicy);

        /// <summary>
        /// Indicates whether the view model of a specific item of a collection should be constructed.
        /// </summary>
        /// <param name="collection">The collection to evaluate.</param>
        /// <param name="index">The index of the item to evaluate.</param>
        /// <param name="expandReferencePolicy">The result from previous call to <see cref="ShouldConstructChildren"/>.</param>
        /// <returns><c>True</c> if the member node should be constructed, <c>False</c> otherwise.</returns>
        bool ShouldConstructItem(IObjectNode collection, Index index, ExpandReferencePolicy expandReferencePolicy);
    }
}
