using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.ViewModels
{
    [Obsolete("This interface is temporary to share properties while both GraphNodeViewModel and NodeViewModel2 exist")]
    public interface IGraphNodeViewModel : INodeViewModel
    {
        IMemberDescriptor GetMemberDescriptor();

        void AddAssociatedData(string key, object value);
    }

    public class ValueChangingEventArgs : EventArgs
    {
        private bool coerced;

        public ValueChangingEventArgs(object newValue)
        {
            NewValue = newValue;
        }

        public object NewValue { get; private set; }

        //public bool Cancel { get; set; }

        //public void Coerce(object value)
        //{
        //    NewValue = value;
        //    coerced = true;
        //}
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public ValueChangedEventArgs(object oldValue)
        {
            OldValue = oldValue;
        }

        public object OldValue { get; }
    }

    public class NodeViewModel : SingleNodeViewModel, IGraphNodeViewModel
    {
        private readonly List<INodePresenter> nodePresenters;

        private int? customOrder;

        public static readonly object DifferentValues;

        protected internal NodeViewModel(GraphViewModel ownerViewModel, NodeViewModel parent, string baseName, Type nodeType, List<INodePresenter> nodePresenters)
            : base(ownerViewModel, nodeType, baseName, default(Index))
        {
            this.nodePresenters = nodePresenters;
            foreach (var nodePresenter in nodePresenters)
            {
                var member = nodePresenter as MemberNodePresenter;
                var displayAttribute = member?.MemberAttributes.OfType<DisplayAttribute>().FirstOrDefault();
                // TODO: check for discrepencies in the display attribute name
                if (displayAttribute != null)
                    DisplayName = displayAttribute.Name;

                // Display this node if at least one presenter is visible
                if (nodePresenter.IsVisible)
                    IsVisible = true;
            }

            // TODO: find a way to "merge" display name if they are different (string.Join?)
            DisplayName = nodePresenters.First().DisplayName;
            parent?.AddChild(this);
        }

        /// <summary>
        /// Gets or sets a custom value for the <see cref="Order"/> of this node.
        /// </summary>
        public int? CustomOrder { get { return customOrder; } set { SetValue(ref customOrder, value, nameof(CustomOrder), nameof(Order)); } }

        /// <inheritdoc/>
        // FIXME
        public override int? Order => CustomOrder ?? NodePresenters.First().Order;

        /// <inheritdoc/>
        public sealed override bool HasCollection => CollectionDescriptor.IsCollection(Type);

        /// <inheritdoc/>
        public sealed override bool HasDictionary => DictionaryDescriptor.IsDictionary(Type);

        public IReadOnlyCollection<INodePresenter> NodePresenters => nodePresenters;

        // FIXME

        protected internal override object InternalNodeValue { get { return GetNodeValue(); } set { SetNodeValue(value); } }

        [Obsolete]
        // FIXME
        public override bool IsPrimitive => NodePresenters.First().IsPrimitive;

        public void FinishInitialization()
        {
            var commonCommands = new Dictionary<INodePresenterCommand, int>();
            var commonAttachedProperties = new Dictionary<PropertyKey, object>();
            foreach (var nodePresenter in nodePresenters)
            {
                foreach (var command in nodePresenter.Commands)
                {
                    int count;
                    if (!commonCommands.TryGetValue(command, out count))
                    {
                        commonCommands.Add(command, 1);
                    }
                    else
                    {
                        commonCommands[command] = count + 1;
                    }
                }
                foreach (var attachedProperty in nodePresenter.AttachedProperties)
                {
                    object value;
                    if (!commonAttachedProperties.TryGetValue(attachedProperty.Key, out value))
                    {
                        commonAttachedProperties.Add(attachedProperty.Key, attachedProperty.Value);
                    }
                    // TODO: properly combine, in the same way that for the value (using DifferentValue object, etc.)
                }
            }
            foreach (var command in commonCommands)
            {
                if (command.Key.CombineMode == CombineMode.DoNotCombine && command.Value > 1)
                    continue;

                if (command.Key.CombineMode == CombineMode.CombineOnlyForAll && command.Value < nodePresenters.Count)
                    continue;

                var commandWrapper = new NodePresenterCommandWrapper(ServiceProvider, nodePresenters, command.Key);
                AddCommand(commandWrapper);
            }
            foreach (var attachedProperty in commonAttachedProperties)
            {
                AddAssociatedData(attachedProperty.Key.Name, attachedProperty.Value);
            }
        }

        protected override void Refresh()
        {

        }

        protected virtual object GetNodeValue()
        {
            object currentValue = null;
            var isFirst = true;
            foreach (var nodePresenter in NodePresenters)
            {
                if (isFirst)
                {
                    currentValue = nodePresenter.Value;
                }
                else if (nodePresenter.Factory.IsPrimitiveType(nodePresenter.Value?.GetType()))
                {
                    if (!Equals(currentValue, nodePresenter.Value))
                        return DifferentValues;
                }
                else
                {
                    // FIXME: handle object references at AssetNodeViewModel level
                    if (currentValue?.GetType() != nodePresenter.Value?.GetType())
                        return DifferentValues;
                }
                isFirst = false;
            }
            return currentValue;
        }

        protected virtual void SetNodeValue(object newValue)
        {
            foreach (var nodePresenter in NodePresenters)
            {
                // TODO: normally it shouldn't take that path (since it uses commands), but this is not safe with newly instantiated values
                // fixme adding a test to check whether it's a content type from Quantum point of view might be safe enough.
                nodePresenter.UpdateValue(newValue);
            }
        }

        IMemberDescriptor IGraphNodeViewModel.GetMemberDescriptor()
        {
            // FIXME
            var member = NodePresenters.First() as MemberNodePresenter;
            return member?.MemberDescriptor;
        }
    }
}
