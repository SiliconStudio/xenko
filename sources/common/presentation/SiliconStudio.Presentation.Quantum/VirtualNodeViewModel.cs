using System;
using System.Collections;
using System.Linq;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public class VirtualNodeViewModel : SingleNodeViewModel
    {
        protected readonly Func<object> Getter;
        protected readonly Action<object> Setter;
        private IGraphNode associatedNode;
        private bool updatingValue;
        private bool initialized;

        static VirtualNodeViewModel()
        {
            typeof(VirtualNodeViewModel).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
        }

        protected internal VirtualNodeViewModel(GraphViewModel owner, Type type, string name, bool isPrimitive, int? order, Index index, Func<object> getter, Action<object> setter)
            : base(owner, type, name, index)
        {
            if (getter == null) throw new ArgumentNullException(nameof(getter));
            Getter = getter;
            Setter = setter;
            Order = order;
            IsPrimitive = isPrimitive;
            Name = name;
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            if (associatedNode != null)
            {
                associatedNode.UnregisterChanging(ContentChanging);
                associatedNode.UnregisterChanged(ContentChanged);
                associatedNode = null;
            }
            base.Destroy();
        }

        public override int? Order { get; }

        public override bool HasCollection => typeof(ICollection).IsAssignableFrom(Type);

        public override bool HasDictionary => typeof(IDictionary).IsAssignableFrom(Type);

        public override bool IsPrimitive { get; }

        /// <inheritdoc/>
        protected internal sealed override object InternalNodeValue { get { return Getter(); } set { SetNodeValue(value); } }

        /// <summary>
        /// Clears the list of children from this <see cref="VirtualNodeViewModel"/>.
        /// </summary>
        public void ClearChildren()
        {
            foreach (var child in Children.Cast<NodeViewModelBase>().ToList())
            {
                RemoveChild(child);
            }
        }

        /// <summary>
        /// Registers an <see cref="IGraphNode"/> object to this virtual node so when the content is modified, this node will trigger notifications
        /// of property changes for the <see cref="VirtualNodeViewModel{T}.TypedValue"/> property.
        /// </summary>
        /// <param name="nodeent">The content to register.</param>
        /// <remarks>Events subscriptions are cleaned when this virtual node is disposed.</remarks>
        public void RegisterContentForNotifications(IGraphNode node)
        {
            if (associatedNode != null)
                throw new InvalidOperationException("A content has already been registered to this virtual node");

            associatedNode = node;
            associatedNode.RegisterChanging(ContentChanging);
            associatedNode.RegisterChanged(ContentChanged);
        }

        public void CompleteInitialization()
        {
            // Safety check
            if (initialized) throw new InvalidOperationException("This node has already been initialized.");
            Owner.GraphViewModelService.NotifyNodeInitialized(this);
            initialized = true;
        }

        protected override void Refresh()
        {
            // TODO: what do we want to do for virtual nodes? They are constructed completely externally...
        }

        protected virtual void SetNodeValue(object value)
        {
            updatingValue = true;
            SetValue(() => Setter(value), nameof(InternalNodeValue));
            updatingValue = false;
        }

        private void ContentChanging(object sender, INodeChangeEventArgs e)
        {
            if (!updatingValue)
                OnPropertyChanging(nameof(InternalNodeValue));
        }

        private void ContentChanged(object sender, INodeChangeEventArgs e)
        {
            if (!updatingValue)
                OnPropertyChanged(nameof(InternalNodeValue));
        }
    }
}
