using System;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Quantum;

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
        private readonly IMemberNode node;
        private readonly string propertyName;
        private readonly PropertyChangeDelegate propertyChanging;
        private readonly PropertyChangeDelegate propertyChanged;
        private readonly Func<TTargetType, TContentType> converter;
        private readonly IUndoRedoService actionService;
        private readonly bool notifyChangesOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodeBinding{TTargetType, TContentType}"/> class.
        /// </summary>
        /// <param name="node">The graph node bound to this instance.</param>
        /// <param name="propertyName">The name of the property of the view model that is bound to this instance.</param>
        /// <param name="propertyChanging">The delegate to invoke when the node content is about to change.</param>
        /// <param name="propertyChanged">The delegate to invoke when the node content has changed.</param>
        /// <param name="converter">A converter function to convert between the property type and the content type.</param>
        /// <param name="actionService"></param>
        /// <param name="notifyChangesOnly">If <c>True</c>, delegates will be invoked only if the content of the node has actually changed. Otherwise, they will be invoked every time the node is updated, even if the new value is equal to the previous one.</param>
        public GraphNodeBinding(IMemberNode node, string propertyName, PropertyChangeDelegate propertyChanging, PropertyChangeDelegate propertyChanged, Func<TTargetType, TContentType> converter, IUndoRedoService actionService, bool notifyChangesOnly = true)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            this.node = node;
            this.propertyName = propertyName;
            this.propertyChanging = propertyChanging;
            this.propertyChanged = propertyChanged;
            this.converter = converter;
            this.actionService = actionService;
            this.notifyChangesOnly = notifyChangesOnly;
            node.ValueChanging += ValueChanging;
            node.ValueChanged += ValueChanged;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            node.ValueChanging -= ValueChanging;
            node.ValueChanged -= ValueChanged;
        }

        /// <summary>
        /// Gets the current value of the graph node.
        /// </summary>
        /// <returns>The current value of the graph node.</returns>
        /// <remarks>This method can be invoked from a property getter.</remarks>
        public TContentType GetNodeValue()
        {
            var value = (TContentType)node.Retrieve();
            return value;
        }

        /// <summary>
        /// Sets the current value of the graph node.
        /// </summary>
        /// <param name="value">The value to set for the graph node content.</param>
        /// <remarks>This method can be invoked from a property setter.</remarks>
        /// <remarks>This method will invoke the delegates passed to the constructor of this instance if the new value is different from the previous one.</remarks>
        public void SetNodeValue(TTargetType value)
        {
            using (var transaction = actionService?.CreateTransaction())
            {
                node.Update(converter(value));
                actionService?.SetName(transaction, $"Update property {propertyName}");
            }
        }

        private void ValueChanging(object sender, INodeChangeEventArgs e)
        {
            if (!notifyChangesOnly || !Equals(e.OldValue, e.NewValue))
            {
                propertyChanging?.Invoke(new[] { propertyName });
            }
        }

        private void ValueChanged(object sender, INodeChangeEventArgs e)
        {
            if (!notifyChangesOnly || !Equals(e.OldValue,e.NewValue))
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
        /// <param name="actionService"></param>
        /// <param name="notifyChangesOnly">If <c>True</c>, delegates will be invoked only if the content of the node has actually changed. Otherwise, they will be invoked every time the node is updated, even if the new value is equal to the previous one.</param>
        public GraphNodeBinding(IMemberNode node, string propertyName, PropertyChangeDelegate propertyChanging, PropertyChangeDelegate propertyChanged, IUndoRedoService actionService, bool notifyChangesOnly = true)
            : base(node, propertyName, propertyChanging, propertyChanged, x => x, actionService, notifyChangesOnly)
        {
        }

        /// <summary>
        /// Gets or sets the current node value.
        /// </summary>
        /// <remarks>The setter of this property will invoke the delegates passed to the constructor of this instance if the new value is different from the previous one.</remarks>
        public TContentType Value { get { return GetNodeValue(); } set { SetNodeValue(value); } }
    }

}
