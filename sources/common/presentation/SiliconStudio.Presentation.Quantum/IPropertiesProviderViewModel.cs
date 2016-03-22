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
    }
}
