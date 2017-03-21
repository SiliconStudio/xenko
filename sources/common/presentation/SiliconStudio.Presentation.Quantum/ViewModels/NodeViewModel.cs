using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.ViewModels
{
    [Obsolete("This interface is temporary to share properties while both GraphNodeViewModel and NodeViewModel2 exist")]
    public interface IGraphNodeViewModel : INodeViewModel
    {
        IMemberDescriptor GetMemberDescriptor();
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

    public class NodeViewModel : NodeViewModelBase, IGraphNodeViewModel
    {
        protected string[] DisplayNameDependentProperties;
        protected Func<string> DisplayNameProvider;

        private readonly List<INodePresenter> nodePresenters;
        private List<DependencyPath> dependencies;
        private int? customOrder;
        private bool isHighlighted;

        public static readonly object DifferentValues;


        static NodeViewModel()
        {
            typeof(NodeViewModel).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
        }

        protected internal NodeViewModel(GraphViewModel ownerViewModel, NodeViewModel parent, string baseName, Type nodeType, List<INodePresenter> nodePresenters)
            : base(ownerViewModel, nodeType, default(Index))
        {
            if (baseName == null)
                throw new ArgumentException("baseName and index can't be both null.");

            CombineMode = CombineMode.CombineOnlyForAll;
            SetName(baseName);

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
        /// Gets or sets the <see cref="CombineMode"/> of this single node.
        /// </summary>
        public CombineMode CombineMode { get; set; }

        /// <summary>
        /// Gets or sets a custom value for the <see cref="Order"/> of this node.
        /// </summary>
        // FIXME
        public int? CustomOrder { get { return NodePresenters.First().CustomOrder; } set { SetValue(ref customOrder, value, nameof(CustomOrder), nameof(Order)); } }

        /// <inheritdoc/>
        // TODO: generalize usage in the templates
        public bool IsHighlighted { get { return isHighlighted; } set { SetValue(ref isHighlighted, value); } }

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

        /// <inheritdoc/>
        public override void Destroy()
        {
            if (dependencies != null)
            {
                Owner.NodeValueChanged -= DependencyNodeValueChanged;
                dependencies = null;
            }
            base.Destroy();
        }

        /// <inheritdoc cref="NodeViewModelBase.AddCommand"/>
        public new void AddCommand([NotNull] INodeCommandWrapper command)
        {
            base.AddCommand(command);
        }

        /// <inheritdoc cref="NodeViewModelBase.RemoveCommand"/>
        public new bool RemoveCommand([NotNull] INodeCommandWrapper command)
        {
            return base.RemoveCommand(command);
        }

        /// <summary>
        /// Adds a dependency to the node represented by the given path.
        /// </summary>
        /// <param name="nodePath">The path to the node that should be a dependency of this node.</param>
        /// <param name="refreshOnNestedNodeChanges">If true, this node will also be refreshed when one of the child node of the dependency node changes.</param>
        /// <remarks>A node that is a dependency to this node will trigger a refresh of this node each time its value is modified (or the value of one of its parent).</remarks>
        public void AddDependency(string nodePath, bool refreshOnNestedNodeChanges)
        {
            if (string.IsNullOrEmpty(nodePath)) throw new ArgumentNullException(nameof(nodePath));

            if (dependencies == null)
            {
                dependencies = new List<DependencyPath>();
                Owner.NodeValueChanged += DependencyNodeValueChanged;
            }

            dependencies.Add(new DependencyPath(nodePath, refreshOnNestedNodeChanges));
        }

        /// <summary>
        /// Registers a function that can compute the display name of this node. If the function uses some children of this node to compute
        /// the display name, the name of these children can be passed so the function is re-evaluated each time one of these children value changes.
        /// </summary>
        /// <param name="provider">A function that can compute the display name of this node.</param>
        /// <param name="dependentProperties">The names of children that should trigger the re-evaluation of the display name when they are modified.</param>
        public void SetDisplayNameProvider(Func<string> provider, params string[] dependentProperties)
        {
            DisplayNameProvider = provider;
            DisplayNameDependentProperties = dependentProperties?.Select(EscapeName).ToArray();
            if (provider != null)
                DisplayName = provider();
        }

        protected override void OnPropertyChanged(params string[] propertyNames)
        {
            base.OnPropertyChanged(propertyNames);
            if (DisplayNameProvider != null && DisplayNameDependentProperties != null)
            {
                if (propertyNames.Any(x => DisplayNameDependentProperties.Contains(x)))
                {
                    DisplayName = DisplayNameProvider();
                }
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

        private void SetName(string nodeName)
        {
            var index = Index;
            nodeName = nodeName?.Replace(".", "-");

            if (!string.IsNullOrWhiteSpace(nodeName))
            {
                Name = nodeName;
                DisplayName = Utils.SplitCamelCase(nodeName);
            }
            else if (!index.IsEmpty)
            {
                // TODO: make a better interface for custom naming specification
                var propertyKey = index.Value as PropertyKey;
                if (propertyKey != null)
                {
                    string name = propertyKey.Name.Replace(".", "-");

                    if (name == "Key")
                        name = propertyKey.PropertyType.Name.Replace(".", "-");

                    Name = name;
                    var parts = propertyKey.Name.Split('.');
                    DisplayName = parts.Length == 2 ? $"{parts[1]} ({parts[0]})" : name;
                }
                else
                {
                    if (index.IsInt)
                        Name = "Item " + index.ToString().Replace(".", "-");
                    else
                        Name = index.ToString().Replace(".", "-");

                    DisplayName = Name;
                }
            }

            Name = EscapeName(Name);
        }

        private void DependencyNodeValueChanged(object sender, GraphViewModelNodeValueChanged e)
        {
            if (dependencies?.Any(x => x.ShouldRefresh(e.NodePath)) ?? false)
            {
                Refresh();
            }
        }

        IMemberDescriptor IGraphNodeViewModel.GetMemberDescriptor()
        {
            // FIXME
            var member = NodePresenters.First() as MemberNodePresenter;
            return member?.MemberDescriptor;
        }

        private struct DependencyPath
        {
            private readonly string path;
            private readonly bool refreshOnNestedNodeChanges;

            public DependencyPath(string path, bool refreshOnNestedNodeChanges)
            {
                this.path = path;
                this.refreshOnNestedNodeChanges = refreshOnNestedNodeChanges;
            }

            public bool ShouldRefresh(string modifiedNodePath)
            {
                if (IsContainingPath(modifiedNodePath, path))
                {
                    // The node that has changed is the dependent node or one of its parent, let's refresh
                    return true;
                }
                if (refreshOnNestedNodeChanges && IsContainingPath(path, modifiedNodePath))
                {
                    // The node that has changed is a child of the dependent node, and we asked for recursive dependencies, let's refresh
                    return true;
                }

                return false;
            }

            private static bool IsContainingPath(string containerPath, string containedPath)
            {
                if (!containedPath.StartsWith(containerPath))
                    return false;

                // Check if the strings are actually identical, or if the next character in the contained path is a property separator ('.')
                return containedPath.Length == containerPath.Length || containedPath[containerPath.Length] == '.';
            }
        }
    }
}
