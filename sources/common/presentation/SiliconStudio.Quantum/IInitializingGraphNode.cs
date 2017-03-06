using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// An interface representing an <see cref="IGraphNode"/> during its initialization phase.
    /// </summary>
    public interface IInitializingGraphNode : IGraphNode
    {
        /// <summary>
        /// Seal the node, indicating its construction is finished and that no more children or commands will be added.
        /// </summary>
        void Seal();

        /// <summary>
        /// Add a command to this node. The node must not have been sealed yet.
        /// </summary>
        /// <param name="command">The node command to add.</param>
        void AddCommand(INodeCommand command);
    }
}
