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
        /// <param name="path">The path to the node of content that has been modified.</param>
        public GraphContentChangeEventArgs(ContentChangeEventArgs e, GraphNodePath path)
            : this(e.Content, e.Index, e.ChangeType, e.OldValue, e.NewValue, path)
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
        /// <param name="path">The path to the node of content that has been modified.</param>
        public GraphContentChangeEventArgs(IContent content, Index index, ContentChangeType changeType, object oldValue, object newValue, GraphNodePath path)
            : base(content, index, changeType, oldValue, newValue)
        {
            Path = path;
        }

        /// <summary>
        /// Gets the path to the node of content that has been modified.
        /// </summary>
        public GraphNodePath Path { get; }
    }
}
