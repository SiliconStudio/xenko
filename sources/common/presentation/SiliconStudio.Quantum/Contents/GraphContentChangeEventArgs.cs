namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// Arguments of the <see cref="GraphNodeChangeListener"/> events.
    /// </summary>
    public class GraphContentChangeEventArgs : ContentChangeEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphContentChangeEventArgs"/> class.
        /// </summary>
        /// <param name="e">A <see cref="ContentChangeEventArgs"/> instance corresponding to this event.</param>
        public GraphContentChangeEventArgs(ContentChangeEventArgs e)
            : this(e.Content, e.Index, e.ChangeType, e.OldValue, e.NewValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphContentChangeEventArgs"/> class.
        /// </summary>
        /// <param name="content">The content that has changed.</param>
        /// <param name="index">The index in the content where the change occurred.</param>
        /// <param name="changeType">The type of change that occurred.</param>
        /// <param name="oldValue">The old value of the content.</param>
        /// <param name="newValue">The new value of the content.</param>
        public GraphContentChangeEventArgs(IContent content, Index index, ContentChangeType changeType, object oldValue, object newValue)
            : base(content, index, changeType, oldValue, newValue)
        {
        }
    }
}