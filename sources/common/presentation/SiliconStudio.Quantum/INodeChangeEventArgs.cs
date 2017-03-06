using SiliconStudio.Core.Annotations;
using SiliconStudio.Quantum.Contents;

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
        /// The index where the change occurred. Must be <see cref="Quantum.Index.Empty"/> if <see cref="ChangeType"/> is
        /// <see cref="ContentChangeType.ValueChange"/>, and another value than <see cref="Quantum.Index.Empty"/> in other cases.
        /// </summary>
        Index Index { get; }

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
