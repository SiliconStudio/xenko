using System;
using System.Collections;
using System.Linq;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class VirtualObservableNode : SingleObservableNode
    {
        protected readonly Func<object> Getter;
        protected readonly Action<object> Setter;
        private IContent associatedContent;
        private bool updatingValue;

        static VirtualObservableNode()
        {
            typeof(VirtualObservableNode).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
        }

        protected VirtualObservableNode(ObservableViewModel owner, string name, bool isPrimitive, int? order, Index index, Func<object> getter, Action<object> setter)
            : base(owner, name, index)
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
            if (associatedContent != null)
            {
                associatedContent.Changing -= ContentChanging;
                associatedContent.Changed -= ContentChanged;
                associatedContent = null;
            }
            base.Destroy();
        }

        public override int? Order { get; }

        public override bool HasList => typeof(ICollection).IsAssignableFrom(Type);

        public override bool HasDictionary => typeof(IDictionary).IsAssignableFrom(Type);

        public override bool IsPrimitive { get; }

        /// <summary>
        /// Clears the list of children from this <see cref="VirtualObservableNode"/>.
        /// </summary>
        public void ClearChildren()
        {
            foreach (var child in Children.Cast<ObservableNode>().ToList())
            {
                RemoveChild(child);
            }
        }

        /// <summary>
        /// Registers an <see cref="IContent"/> object to this virtual node so when the content is modified, this node will trigger notifications
        /// of property changes for the <see cref="VirtualObservableNode{T}.TypedValue"/> property.
        /// </summary>
        /// <param name="content">The content to register.</param>
        /// <remarks>Events subscriptions are cleaned when this virtual node is disposed.</remarks>
        public void RegisterContentForNotifications(IContent content)
        {
            if (associatedContent != null)
                throw new InvalidOperationException("A content has already been registered to this virtual node");

            associatedContent = content;
            associatedContent.Changing += ContentChanging;
            associatedContent.Changed += ContentChanged;
        }

        public new void AddCommand(INodeCommandWrapper command)
        {
            base.AddCommand(command);
        }

        protected virtual void SetTypedValue(object value)
        {
            updatingValue = true;
            SetValue(() => Setter(value), nameof(VirtualObservableNode<object>.TypedValue));
            updatingValue = false;
        }

        private void ContentChanging(object sender, ContentChangeEventArgs e)
        {
            if (!updatingValue)
                OnPropertyChanging(nameof(VirtualObservableNode<object>.TypedValue));
        }

        private void ContentChanged(object sender, ContentChangeEventArgs e)
        {
            if (!updatingValue)
                OnPropertyChanged(nameof(VirtualObservableNode<object>.TypedValue));
        }
    }

    public class VirtualObservableNode<T> : VirtualObservableNode
    {
        public VirtualObservableNode(ObservableViewModel owner, string name, bool isPrimitive, int? order, Index index, Func<object> getter, Action<object> setter)
            : base(owner, name, isPrimitive, order, index, getter, setter)
        {
            DependentProperties.Add(nameof(TypedValue), new[] { nameof(Value) });
        }

        /// <summary>
        /// Gets or sets the value of this node through a correctly typed property, which is more adapted to binding.
        /// </summary>
        public T TypedValue { get { return (T)Getter(); } set { SetTypedValue(value); } }

        /// <inheritdoc/>
        public override Type Type => typeof(T);

        /// <inheritdoc/>
        public override sealed object Value { get { return TypedValue; } set { TypedValue = (T)value; } }
    }
}
