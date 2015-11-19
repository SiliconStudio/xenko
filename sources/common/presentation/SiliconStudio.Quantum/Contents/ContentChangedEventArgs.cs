using System;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// Arguments of the <see cref="IContent.Changed"/> event.
    /// </summary>
    public class ContentChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChangedEventArgs"/> class.
        /// </summary>
        /// <param name="content">The content that has changed.</param>
        /// <param name="oldValue">The old value of the content.</param>
        /// <param name="newValue">The new value of the content.</param>
        public ContentChangedEventArgs(IContent content, object oldValue, object newValue)
        {
            Content = content;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the content that has changed.
        /// </summary>
        public IContent Content { get; }

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