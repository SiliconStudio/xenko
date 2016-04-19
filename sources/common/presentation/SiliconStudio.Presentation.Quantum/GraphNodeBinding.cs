using System;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// This class allows to bind a property of a view model to a <see cref="IGraphNode"/> and properly trigger property change notifications
    /// when the node value is modified.
    /// </summary>
    /// <typeparam name="TTargetType">The type of property bound to the graph node.</typeparam>
    /// <typeparam name="TContentType">The type of content in the graph node.</typeparam>
    public class GraphNodeBinding<TTargetType, TContentType> : IDisposable
    {
        public delegate void PropertyChangeDelegate(string[] propertyNames);
        private readonly IGraphNode node;
        private readonly string propertyName;
        private readonly PropertyChangeDelegate propertyChanging;
        private readonly PropertyChangeDelegate propertyChanged;
        private readonly Func<TContentType, TTargetType> converter;
        private readonly bool notifyChangesOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodeBinding{TTargetType, TContentType}"/> class.
        /// </summary>
        /// <param name="node">The graph node bound to this instance.</param>
        /// <param name="propertyName">The name of the property of the view model that is bound to this instance.</param>
        /// <param name="propertyChanging">The delegate to invoke when the node content is about to change.</param>
        /// <param name="propertyChanged">The delegate to invoke when the node content has changed.</param>
        /// <param name="converter">A converter function to convert between the content type and the property type.</param>
        /// <param name="notifyChangesOnly">If <c>True</c>, delegates will be invoked only if the content of the node has actually changed. Otherwise, they will be invoked every time the node is updated, even if the new value is equal to the previous one.</param>
        public GraphNodeBinding(IGraphNode node, string propertyName, PropertyChangeDelegate propertyChanging, PropertyChangeDelegate propertyChanged, Func<TContentType, TTargetType> converter, bool notifyChangesOnly = true)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            this.node = node;
            this.propertyName = propertyName;
            this.propertyChanging = propertyChanging;
            this.propertyChanged = propertyChanged;
            this.converter = converter;
            this.notifyChangesOnly = notifyChangesOnly;
            node.Content.Changing += ContentChanging;
            node.Content.Changed += ContentChanged;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            node.Content.Changing -= ContentChanging;
            node.Content.Changed -= ContentChanged;
        }

        /// <summary>
        /// Gets the current value of the graph node.
        /// </summary>
        /// <returns>The current value of the graph node.</returns>
        /// <remarks>This method can be invoked from a property getter.</remarks>
        public TTargetType GetNodeValue()
        {
            var value = (TContentType)node.Content.Retrieve();
            return converter(value);
        }

        /// <summary>
        /// Sets the current value of the graph node.
        /// </summary>
        /// <param name="value">The value to set for the graph node content.</param>
        /// <remarks>This method can be invoked from a property setter.</remarks>
        /// <remarks>This method will invoke the delegates passed to the constructor of this instance if the new value is different from the previous one.</remarks>
        public void SetNodeValue(TContentType value)
        {
            node.Content.Update(value);
        }

        private void ContentChanging(object sender, ContentChangeEventArgs e)
        {
            if (!notifyChangesOnly || !Equals(Content.Retrieve(e.OldValue, e.Index, e.Content.Descriptor), Content.Retrieve(e.NewValue, e.Index, e.Content.Descriptor)))
            {
                propertyChanging?.Invoke(new[] { propertyName });
            }
        }

        private void ContentChanged(object sender, ContentChangeEventArgs e)
        {
            if (!notifyChangesOnly || !Equals(Content.Retrieve(e.OldValue, e.Index, e.Content.Descriptor), Content.Retrieve(e.NewValue, e.Index, e.Content.Descriptor)))
            {
                propertyChanged?.Invoke(new[] { propertyName });
            }
        }
    }

    /// <summary>
    /// This is a specialization of the <see cref="GraphNodeBinding{TTargetType, TContentType}"/> class, when the target type is the same that the
    /// content type.
    /// This class allows to bind a property of a view model to a <see cref="IGraphNode"/> and properly trigger property change notifications
    /// when the node value is modified.
    /// </summary>
    /// <typeparam name="TContentType">The type of the node content and the property bound to the graph node.</typeparam>
    public class GraphNodeBinding<TContentType> : GraphNodeBinding<TContentType, TContentType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodeBinding{TContentType}"/> class.
        /// </summary>
        /// <param name="node">The graph node bound to this instance.</param>
        /// <param name="propertyName">The name of the property of the view model that is bound to this instance.</param>
        /// <param name="propertyChanging">The delegate to invoke when the node content is about to change.</param>
        /// <param name="propertyChanged">The delegate to invoke when the node content has changed.</param>
        /// <param name="notifyChangesOnly">If <c>True</c>, delegates will be invoked only if the content of the node has actually changed. Otherwise, they will be invoked every time the node is updated, even if the new value is equal to the previous one.</param>
        public GraphNodeBinding(IGraphNode node, string propertyName, PropertyChangeDelegate propertyChanging, PropertyChangeDelegate propertyChanged, bool notifyChangesOnly = true)
            : base(node, propertyName, propertyChanging, propertyChanged, x => x, notifyChangesOnly)
        {
        }

        /// <summary>
        /// Gets or sets the current node value.
        /// </summary>
        /// <remarks>The setter of this property will invoke the delegates passed to the constructor of this instance if the new value is different from the previous one.</remarks>
        public TContentType Value { get { return GetNodeValue(); } set { SetNodeValue(value); } }
    }

}
