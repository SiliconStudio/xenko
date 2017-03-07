using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class RootNodePresenter : IInitializingNodePresenter
    {
        private readonly IObjectNode rootNode;
        private readonly List<INodePresenter> children = new List<INodePresenter>();

        public RootNodePresenter(IObjectNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public void Dispose()
        {

        }

        public string Name => "Root";
        public INodePresenter Parent => null;
        public IReadOnlyList<INodePresenter> Children => children;
        public List<INodeCommand> Commands { get; }
        public Type Type => rootNode.Type;
        public Index Index => Index.Empty;
        public bool IsPrimitive => false;
        public bool IsEnumerable => rootNode.IsEnumerable;
        public ITypeDescriptor Descriptor => rootNode.Descriptor;
        public int? Order => null;
        public object Value { get { return rootNode.Retrieve(); } set { throw new InvalidOperationException("A RootNodePresenter value cannot be modified"); } }
        public event EventHandler<ValueChangingEventArgs> ValueChanging;
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public void UpdateValue(object newValue)
        {
            throw new NodePresenterException($"A {nameof(RootNodePresenter)} cannot have its own value updated.");
        }

        public void AddItem(object value)
        {
            if (!rootNode.IsEnumerable)
                throw new NodePresenterException($"{nameof(RootNodePresenter)}.{nameof(AddItem)} cannot be invoked on objects that are not collection.");

            try
            {
                rootNode.Add(value);
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public void AddItem(object value, Index index)
        {
            if (!rootNode.IsEnumerable)
                throw new NodePresenterException($"{nameof(RootNodePresenter)}.{nameof(AddItem)} cannot be invoked on objects that are not collection.");

            try
            {
                rootNode.Add(value, index);
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public void RemoveItem(object value, Index index)
        {
            if (!rootNode.IsEnumerable)
                throw new NodePresenterException($"{nameof(RootNodePresenter)}.{nameof(RemoveItem)} cannot be invoked on objects that are not collection.");

            try
            {
                rootNode.Remove(value, index);
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while removing an item to the node, see the inner exception for more information.", e);
            }
        }

        public void UpdateItem(object newValue, Index index)
        {
            if (!rootNode.IsEnumerable)
                throw new NodePresenterException($"{nameof(RootNodePresenter)}.{nameof(UpdateItem)} cannot be invoked on objects that are not collection.");

            try
            {
                rootNode.Update(newValue, index);
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating an item of the node, see the inner exception for more information.", e);
            }
        }

        void IInitializingNodePresenter.AddChild([NotNull] IInitializingNodePresenter child)
        {
            children.Add(child);
        }

        void IInitializingNodePresenter.FinalizeInitialization()
        {
            children.Sort(GraphNodePresenter.CompareChildren);
        }
    }
}
