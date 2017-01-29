using System;
using System.Collections;
using System.Linq;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class VirtualNodeViewModel : SingleNodeViewModel
    {
        protected readonly Func<object> Getter;
        protected readonly Action<object> Setter;
        private IMemberNode associatedContent;
        private bool updatingValue;
        private bool initialized;

        static VirtualNodeViewModel()
        {
            typeof(VirtualNodeViewModel).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
        }

        protected VirtualNodeViewModel(GraphViewModel owner, string name, bool isPrimitive, int? order, Index index, Func<object> getter, Action<object> setter)
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

        public override bool HasCollection => typeof(ICollection).IsAssignableFrom(Type);

        public override bool HasDictionary => typeof(IDictionary).IsAssignableFrom(Type);

        public override bool IsPrimitive { get; }

        /// <summary>
        /// Clears the list of children from this <see cref="VirtualNodeViewModel"/>.
        /// </summary>
        public void ClearChildren()
        {
            foreach (var child in Children.Cast<NodeViewModel>().ToList())
            {
                RemoveChild(child);
            }
        }

        /// <summary>
        /// Registers an <see cref="IContentNode"/> object to this virtual node so when the content is modified, this node will trigger notifications
        /// of property changes for the <see cref="VirtualNodeViewModel{T}.TypedValue"/> property.
        /// </summary>
        /// <param name="content">The content to register.</param>
        /// <remarks>Events subscriptions are cleaned when this virtual node is disposed.</remarks>
        public void RegisterContentForNotifications(IMemberNode content)
        {
            if (associatedContent != null)
                throw new InvalidOperationException("A content has already been registered to this virtual node");

            associatedContent = content;
            associatedContent.Changing += ContentChanging;
            associatedContent.Changed += ContentChanged;
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

        protected virtual void SetTypedValue(object value)
        {
            updatingValue = true;
            SetValue(() => Setter(value), nameof(VirtualNodeViewModel<object>.TypedValue));
            updatingValue = false;
        }

        private void ContentChanging(object sender, MemberNodeChangeEventArgs e)
        {
            if (!updatingValue)
                OnPropertyChanging(nameof(VirtualNodeViewModel<object>.TypedValue));
        }

        private void ContentChanged(object sender, MemberNodeChangeEventArgs e)
        {
            if (!updatingValue)
                OnPropertyChanged(nameof(VirtualNodeViewModel<object>.TypedValue));
        }
    }

    public class VirtualNodeViewModel<T> : VirtualNodeViewModel
    {
        public VirtualNodeViewModel(GraphViewModel owner, string name, bool isPrimitive, int? order, Index index, Func<object> getter, Action<object> setter)
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
        public sealed override object Value { get { return TypedValue; } set { TypedValue = (T)value; } }
    }
}
