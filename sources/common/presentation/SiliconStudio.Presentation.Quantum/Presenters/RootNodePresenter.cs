using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Quantum.ViewModels;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class RootNodePresenter : NodePresenterBase
    {
        protected readonly IObjectNode RootNode;

        public RootNodePresenter([NotNull] INodePresenterFactoryInternal factory, IPropertyProviderViewModel propertyProvider, [NotNull] IObjectNode rootNode)
            : base(factory, propertyProvider, null)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (rootNode == null) throw new ArgumentNullException(nameof(rootNode));
            RootNode = rootNode;
            Name = "Root";

            foreach (var command in factory.AvailableCommands)
            {
                if (command.CanAttach(this))
                    Commands.Add(command);
            }

            AttachCommands();
        }

        public sealed override List<INodePresenterCommand> Commands { get; } = new List<INodePresenterCommand>();
        public override Type Type => RootNode.Type;
        public override Index Index => Index.Empty;
        public override bool IsPrimitive => false;
        public override bool IsEnumerable => RootNode.IsEnumerable;
        public override ITypeDescriptor Descriptor => RootNode.Descriptor;
        public override int? Order => null;
        public override object Value => RootNode.Retrieve();
        public override event EventHandler<ValueChangingEventArgs> ValueChanging;
        public override event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected override IObjectNode ParentingNode => RootNode;

        public override void UpdateValue(object newValue)
        {
            throw new NodePresenterException($"A {nameof(RootNodePresenter)} cannot have its own value updated.");
        }

        public override void AddItem(object value)
        {
            if (!RootNode.IsEnumerable)
                throw new NodePresenterException($"{nameof(RootNodePresenter)}.{nameof(AddItem)} cannot be invoked on objects that are not collection.");

            try
            {
                ValueChanging?.Invoke(this, new ValueChangingEventArgs(Value));
                RootNode.Add(value);
                Refresh();
                ValueChanged?.Invoke(this, new ValueChangedEventArgs(Value));
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void AddItem(object value, Index index)
        {
            if (!RootNode.IsEnumerable)
                throw new NodePresenterException($"{nameof(RootNodePresenter)}.{nameof(AddItem)} cannot be invoked on objects that are not collection.");

            try
            {
                ValueChanging?.Invoke(this, new ValueChangingEventArgs(Value));
                RootNode.Add(value, index);
                Refresh();
                ValueChanged?.Invoke(this, new ValueChangedEventArgs(Value));
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void RemoveItem(object value, Index index)
        {
            if (!RootNode.IsEnumerable)
                throw new NodePresenterException($"{nameof(RootNodePresenter)}.{nameof(RemoveItem)} cannot be invoked on objects that are not collection.");

            try
            {
                ValueChanging?.Invoke(this, new ValueChangingEventArgs(Value));
                RootNode.Remove(value, index);
                Refresh();
                ValueChanged?.Invoke(this, new ValueChangedEventArgs(Value));
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while removing an item to the node, see the inner exception for more information.", e);
            }
        }

        public override NodeAccessor GetNodeAccessor()
        {
            return new NodeAccessor(RootNode, Index.Empty);
        }
    }
}
