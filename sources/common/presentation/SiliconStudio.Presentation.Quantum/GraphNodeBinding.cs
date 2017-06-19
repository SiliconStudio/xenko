// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class GraphNodeBinding<TTargetType, TContentType> : IDisposable
    {
        public delegate void PropertyChangeDelegate(string[] propertyNames);

        protected readonly IUndoRedoService actionService;
        protected readonly string propertyName;
        protected readonly Func<TTargetType, TContentType> converter;
        private readonly PropertyChangeDelegate propertyChanging;
        private readonly PropertyChangeDelegate propertyChanged;
        private readonly bool notifyChangesOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberGraphNodeBinding{TTargetType, TContentType}"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property of the view model that is bound to this instance.</param>
        /// <param name="propertyChanging">The delegate to invoke when the node content is about to change.</param>
        /// <param name="propertyChanged">The delegate to invoke when the node content has changed.</param>
        /// <param name="converter">A converter function to convert between the property type and the content type.</param>
        /// <param name="actionService"></param>
        /// <param name="notifyChangesOnly">If <c>True</c>, delegates will be invoked only if the content of the node has actually changed. Otherwise, they will be invoked every time the node is updated, even if the new value is equal to the previous one.</param>
        public GraphNodeBinding(string propertyName, PropertyChangeDelegate propertyChanging, PropertyChangeDelegate propertyChanged, Func<TTargetType, TContentType> converter, IUndoRedoService actionService, bool notifyChangesOnly = true)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            this.propertyName = propertyName;
            this.propertyChanging = propertyChanging;
            this.propertyChanged = propertyChanged;
            this.converter = converter;
            this.actionService = actionService;
            this.notifyChangesOnly = notifyChangesOnly;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Gets the current value of the graph node.
        /// </summary>
        /// <returns>The current value of the graph node.</returns>
        /// <remarks>This method can be invoked from a property getter.</remarks>
        public abstract TContentType GetNodeValue();

        /// <summary>
        /// Sets the current value of the graph node.
        /// </summary>
        /// <param name="value">The value to set for the graph node content.</param>
        /// <remarks>This method can be invoked from a property setter.</remarks>
        /// <remarks>This method will invoke the delegates passed to the constructor of this instance if the new value is different from the previous one.</remarks>
        public abstract void SetNodeValue(TTargetType value);

        protected void ValueChanging(object sender, INodeChangeEventArgs e)
        {
            if (!notifyChangesOnly || !Equals(e.OldValue, e.NewValue))
            {
                propertyChanging?.Invoke(new[] { propertyName });
            }
        }

        protected void ValueChanged(object sender, INodeChangeEventArgs e)
        {
            if (!notifyChangesOnly || !Equals(e.OldValue,e.NewValue))
            {
                propertyChanged?.Invoke(new[] { propertyName });
            }
        }
    }
}
