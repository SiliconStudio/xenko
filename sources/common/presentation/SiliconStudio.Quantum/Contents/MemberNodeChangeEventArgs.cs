using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Quantum.Contents
{
    public enum ContentChangeType
    {
        None,
        ValueChange,
        CollectionUpdate,
        CollectionAdd,
        CollectionRemove,
    }

    /// <summary>
    /// Arguments of the <see cref="IContentNode.Changing"/> and <see cref="IContentNode.Changed"/> events.
    /// </summary>
    public class MemberNodeChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberNodeChangeEventArgs"/> class.
        /// </summary>
        /// <param name="member">The member that has changed.</param>
        /// <param name="index">The index in the member where the change occurred.</param>
        /// <param name="changeType">The type of change that occurred.</param>
        /// <param name="oldValue">The old value of the member or the item of the member that has changed.</param>
        /// <param name="newValue">The new value of the member or the item of the member that has changed.</param>
        public MemberNodeChangeEventArgs([NotNull] IMemberNode member, Index index, ContentChangeType changeType, object oldValue, object newValue)
        {
            Member = member;
            Index = index;
            ChangeType = changeType;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the member that has changed.
        /// </summary>
        [NotNull]
        public IMemberNode Member { get; }

        /// <summary>
        /// Gets the index where the change occurred.
        /// </summary>
        public Index Index { get; }

        /// <summary>
        /// The type of change.
        /// </summary>
        public ContentChangeType ChangeType { get; }

        /// <summary>
        /// Gets the old value of the member or the item of the member that has changed.
        /// </summary>
        public object OldValue { get; }

        /// <summary>
        /// Gets the new value of the member or the item of the member that has changed.
        /// </summary>
        public object NewValue { get; }
    }
}
