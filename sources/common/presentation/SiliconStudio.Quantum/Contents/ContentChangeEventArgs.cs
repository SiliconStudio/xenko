using System;

namespace SiliconStudio.Quantum.Contents
{
    public enum ContentChangeType
    {
        None,
        ValueChange,
        CollectionAdd,
        CollectionRemove,
    }

    /// <summary>
    /// Arguments of the <see cref="IContent.Changing"/> and <see cref="IContent.Changed"/> events.
    /// </summary>
    public class ContentChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChangeEventArgs"/> class.
        /// </summary>
        /// <param name="content">The content that has changed.</param>
        /// <param name="index">The index in the content where the change occurred.</param>
        /// <param name="changeType">The type of change that occurred.</param>
        /// <param name="oldValue">The old value of the content.</param>
        /// <param name="newValue">The new value of the content.</param>
        public ContentChangeEventArgs(IContent content, Index index, ContentChangeType changeType, object oldValue, object newValue)
        {
            Content = content;
            Index = index;
            ChangeType = changeType;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the content that has changed.
        /// </summary>
        public IContent Content { get; }

        /// <summary>
        /// Gets the index where the change occurred.
        /// </summary>
        public Index Index { get; }

        /// <summary>
        /// The type of change.
        /// </summary>
        public ContentChangeType ChangeType { get; }

        /// <summary>
        /// Gets the old value of the content.
        /// </summary>
        public object OldValue { get; }

        /// <summary>
        /// Gets the new value of the content.
        /// </summary>
        public object NewValue { get; }
    }
}
