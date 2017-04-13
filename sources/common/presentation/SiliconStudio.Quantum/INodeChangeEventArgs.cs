using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// A global interface representing any kind of change in a node.
    /// </summary>
    public interface INodeChangeEventArgs
    {
        /// <summary>
        /// The node that has changed.
        /// </summary>
        [NotNull]
        IGraphNode Node { get; }

        /// <summary>
        /// The type of change.
        /// </summary>
        ContentChangeType ChangeType { get; }

        /// <summary>
        /// The old value of the node or the item of the node that has changed.
        /// </summary>
        object OldValue { get; }

        /// <summary>
        /// The new value of the node or the item of the node that has changed.
        /// </summary>
        object NewValue { get; }
    }
}
