using System;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// Arguments of the <see cref="IContent.Changing"/> and <see cref="IContent.Changed"/> events.
    /// </summary>
    public class ContentChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChangeEventArgs"/> class.
        /// </summary>
        /// <param name="content">The content that has changed.</param>
        /// <param name="index">The index where the change occurred, if applicable. <c>null</c> otherwise.</param>
        /// <param name="oldValue">The old value of the content.</param>
        /// <param name="newValue">The new value of the content.</param>
        public ContentChangeEventArgs(IContent content, object index, object oldValue, object newValue)
        {
            Content = content;
            Index = index;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the content that has changed.
        /// </summary>
        public IContent Content { get; }

        /// <summary>
        /// Gets the index where the change occurred, if applicable. This property is <c>null</c> otherwise.
        /// </summary>
        public object Index { get; }

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