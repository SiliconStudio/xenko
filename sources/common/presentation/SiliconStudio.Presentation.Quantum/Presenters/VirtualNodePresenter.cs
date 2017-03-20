using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class VirtualNodePresenter : NodePresenterBase
    {
        [NotNull] private readonly Func<object> getter;
        private readonly Action<object> setter;
        private readonly List<Attribute> memberAttributes = new List<Attribute>();

        public VirtualNodePresenter([NotNull] INodePresenterFactoryInternal factory, IPropertyProviderViewModel propertyProvider, [NotNull] INodePresenter parent, string name, Type type, int? order, [NotNull] Func<object> getter, Action<object> setter)
            : base(factory, propertyProvider, parent)
        {
            this.getter = getter;
            this.setter = setter;
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (getter == null) throw new ArgumentNullException(nameof(getter));
            Name = name;
            Type = type;
            Order = order;
            Descriptor = TypeDescriptorFactory.Default.Find(type);

            AttachCommands();
        }

        public override string Name { get; }

        public sealed override List<INodePresenterCommand> Commands { get; } = new List<INodePresenterCommand>();

        public override Type Type { get; }

        public override bool IsPrimitive => false;

        public override bool IsEnumerable => false;

        public override Index Index => Index.Empty;

        public override ITypeDescriptor Descriptor { get; }

        public override int? Order { get; }

        public override object Value => getter();

        public IReadOnlyList<Attribute> MemberAttributes => memberAttributes;

        protected override IObjectNode ParentingNode => null;

        public override event EventHandler<ValueChangingEventArgs> ValueChanging;

        public override event EventHandler<ValueChangedEventArgs> ValueChanged;

        public override void UpdateValue(object newValue)
        {
            try
            {
                var oldValue = getter();
                ValueChanging?.Invoke(this, new ValueChangingEventArgs(newValue));
                setter(newValue);
                Refresh();
                ValueChanged?.Invoke(this, new ValueChangedEventArgs(oldValue));
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating the value of the node, see the inner exception for more information.", e);
            }
        }

        public override void AddItem(object value)
        {
            throw new NodePresenterException($"{nameof(AddItem)} cannot be used on a {nameof(VirtualNodePresenter)}.");
        }

        public override void AddItem(object value, Index index)
        {
            throw new NodePresenterException($"{nameof(AddItem)} cannot be used on a {nameof(VirtualNodePresenter)}.");
        }

        public override void RemoveItem(object value, Index index)
        {
            throw new NodePresenterException($"{nameof(RemoveItem)} cannot be used on a {nameof(VirtualNodePresenter)}.");
        }

        public override void UpdateItem(object newValue, Index index)
        {
            throw new NodePresenterException($"{nameof(UpdateItem)} cannot be used on a {nameof(VirtualNodePresenter)}.");
        }

        public override NodeAccessor GetNodeAccessor()
        {
            return default(NodeAccessor);
        }
    }
}