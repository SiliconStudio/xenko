namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// An interface representing an object capable of enriching a node of an <see cref="ObservableViewModel"/>.
    /// </summary>
    public interface IPropertyNodeUpdater
    {
        /// <summary>
        /// Updates the given node.
        /// </summary>
        /// <param name="node">The node to enrich.</param>
        void UpdateNode(SingleObservableNode node);
    }
}
