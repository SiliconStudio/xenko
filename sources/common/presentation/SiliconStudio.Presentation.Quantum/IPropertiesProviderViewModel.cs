using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// An interface representing an view model that can provide properties to build an <see cref="ObservableViewModel"/>.
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
        IGraphNode GetRootNode();

        /// <summary>
        /// Indicates whether the observable node corresponding to the given graph node should be build.
        /// </summary>
        /// <param name="node">The corresponding graph node.</param>
        /// <param name="index">The index of the element in the node, if relevant..</param>
        /// <returns><c>True</c> if the node should be constructed, <c>False</c> otherwise.</returns>
        bool ShouldConstructNode(IContentNode node, Index index);
    }
}
