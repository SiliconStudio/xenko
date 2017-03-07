using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class RootNodePresenter : NodePresenterBase
    {
        private readonly IObjectNode rootNode;

        public RootNodePresenter([NotNull] INodePresenterFactoryInternal factory, IObjectNode rootNode)
            : base(factory, null)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            this.rootNode = rootNode;
        }

        public override string Name => "Root";
        public override List<INodeCommand> Commands { get; }
        public override Type Type => rootNode.Type;
        public override Index Index => Index.Empty;
        public override bool IsPrimitive => false;
        public override bool IsEnumerable => rootNode.IsEnumerable;
        public override ITypeDescriptor Descriptor => rootNode.Descriptor;
        public override int? Order => null;
        public override object Value => rootNode.Retrieve();
        public override event EventHandler<ValueChangingEventArgs> ValueChanging;
        public override event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected override IObjectNode ParentingNode => rootNode;

        public override void UpdateValue(object newValue)
        {
            throw new NodePresenterException($"A {nameof(RootNodePresenter)} cannot have its own value updated.");
        }

        public override void AddItem(object value)
        {
            if (!rootNode.IsEnumerable)
                throw new NodePresenterException($"{nameof(RootNodePresenter)}.{nameof(AddItem)} cannot be invoked on objects that are not collection.");

            try
            {
                rootNode.Add(value);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void AddItem(object value, Index index)
        {
            if (!rootNode.IsEnumerable)
                throw new NodePresenterException($"{nameof(RootNodePresenter)}.{nameof(AddItem)} cannot be invoked on objects that are not collection.");

            try
            {
                rootNode.Add(value, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void RemoveItem(object value, Index index)
        {
            if (!rootNode.IsEnumerable)
                throw new NodePresenterException($"{nameof(RootNodePresenter)}.{nameof(RemoveItem)} cannot be invoked on objects that are not collection.");

            try
            {
                rootNode.Remove(value, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while removing an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void UpdateItem(object newValue, Index index)
        {
            if (!rootNode.IsEnumerable)
                throw new NodePresenterException($"{nameof(RootNodePresenter)}.{nameof(UpdateItem)} cannot be invoked on objects that are not collection.");

            try
            {
                rootNode.Update(newValue, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating an item of the node, see the inner exception for more information.", e);
            }
        }
    }
}
