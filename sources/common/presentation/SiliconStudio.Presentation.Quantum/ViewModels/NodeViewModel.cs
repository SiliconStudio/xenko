using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Windows;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Collections;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using Expression = System.Linq.Expressions.Expression;

namespace SiliconStudio.Presentation.Quantum.ViewModels
{
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

    public class NodeViewModel : DispatcherViewModel, INodeViewModel, IDynamicMetaObjectProvider
    {
        internal class DifferentValuesObject { public readonly string Name = "DifferentValues"; };

        protected string[] DisplayNameDependentProperties;
        protected Func<string> DisplayNameProvider;
        protected static readonly HashSet<string> ReservedNames = new HashSet<string>();
        private readonly AutoUpdatingSortedObservableCollection<INodeViewModel> children = new AutoUpdatingSortedObservableCollection<INodeViewModel>(new AnonymousComparer<INodeViewModel>(CompareChildren), nameof(Name), nameof(Index), nameof(Order));
        private readonly ObservableCollection<INodeCommandWrapper> commands = new ObservableCollection<INodeCommandWrapper>();
        private readonly Dictionary<string, object> associatedData = new Dictionary<string, object>();
        private readonly List<string> changingProperties = new List<string>();
        private bool isVisible;
        private bool isReadOnly;
        private string displayName;
        private int visibleChildrenCount;
        private List<INodeViewModel> initializingChildren = new List<INodeViewModel>();
        private readonly List<INodePresenter> nodePresenters;
        private int? customOrder;
        private bool isHighlighted;

        public static readonly object DifferentValues = new DifferentValuesObject();

        static NodeViewModel()
        {
            typeof(NodeViewModel).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
        }

        protected internal NodeViewModel(GraphViewModel ownerViewModel, NodeViewModel parent, string baseName, Type nodeType, List<INodePresenter> nodePresenters)
            : base(ownerViewModel.ServiceProvider)
        {
            DependentProperties.Add(nameof(Path), new[] { nameof(DisplayPath) });
            Owner = ownerViewModel;
            Type = nodeType;

            if (baseName == null)
                throw new ArgumentException("baseName and index can't be both null.");

            CombineMode = CombineMode.CombineOnlyForAll;
            Name = EscapeName(baseName);

            this.nodePresenters = nodePresenters;
            foreach (var nodePresenter in nodePresenters)
            {
                nodePresenter.ValueChanging += ValueChanging;
                nodePresenter.ValueChanged += ValueChanged;
            }

            UpdateViewModelProperties();

            parent?.AddChild(this);
        }

        /// <summary>
        /// Gets the <see cref="GraphViewModel"/> that owns this node.
        /// </summary>
        public GraphViewModel Owner { get; }

        /// <summary>
        /// Gets the expected type of <see cref="NodeValue"/>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets or sets the name of this node. Note that the name can be used to access this node from its parent using a dynamic object.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the name used to display the node to the user.
        /// </summary>
        public string DisplayName { get { return displayName; } set { SetValue(ref displayName, value); } }

        /// <summary>
        /// Gets the path of this node. The path is constructed from the name of all nodes from the root to this one, separated by periods.
        /// </summary>
        [Obsolete]
        public string Path => Parent != null ? Parent.Path + '.' + Name : Name;

        /// <summary>
        /// Gets the display path of this node. The path is constructed from the <see cref="DisplayName"/> of all nodes from the root to this one, separated by periods.
        /// </summary>
        public string DisplayPath { get { if (Parent == null) return string.Empty; var parentPath = Parent.DisplayPath; return parentPath != string.Empty ? parentPath + '.' + DisplayName : DisplayName; } }

        /// <summary>
        /// Gets the parent of this node.
        /// </summary>
        public INodeViewModel Parent { get; private set; }

        /// <summary>
        /// Gets the root of this node.
        /// </summary>
        public INodeViewModel Root { get { INodeViewModel root = this; while (root.Parent != null) root = root.Parent; return root; } }

        /// <summary>
        /// Gets or sets whether this node should be displayed in the view.
        /// </summary>
        public bool IsVisible { get { return isVisible; } set { SetValue(ref isVisible, value, () => IsVisibleChanged?.Invoke(this, EventArgs.Empty)); } }

        /// <summary>
        /// Gets or sets whether this node can be modified in the view.
        /// </summary>
        public bool IsReadOnly { get { return isReadOnly; } set { SetValue(ref isReadOnly, value); } }

        /// <inheritdoc/>
        public object NodeValue { get { return GetNodeValue(); } set { SetNodeValue(ConvertValue(value)); } }

        /// <summary>
        /// Gets the list of children nodes.
        /// </summary>
        public IReadOnlyCollection<INodeViewModel> Children => initializingChildren != null ? (IReadOnlyCollection<INodeViewModel>)initializingChildren : children;

        /// <summary>
        /// Gets the list of commands available in this node.
        /// </summary>
        public IEnumerable<INodeCommandWrapper> Commands => commands;

        /// <summary>
        /// Gets additional data associated to this content. This can be used when the content itself does not contain enough information to be used as a view model.
        /// </summary>
        public IReadOnlyDictionary<string, object> AssociatedData => associatedData;

        /// <summary>
        /// Gets the level of depth of this node, starting from 0 for the root node.
        /// </summary>
        public int Level => Parent?.Level + 1 ?? 0;

        /// <summary>
        /// Gets the order number of this node in its parent.
        /// </summary>
        // FIXME
        public int? Order => CustomOrder ?? NodePresenters.First().Order;

        /// <summary>
        /// Gets the member info (if any).
        /// </summary>
        public MemberInfo MemberInfo => null;

        /// <summary>
        /// Gets whether this node contains a collection.
        /// </summary>
        /// <remarks>Used mostly for sorting purpose.</remarks>
        /// <seealso cref="HasDictionary"/>
        public bool HasCollection => CollectionDescriptor.IsCollection(Type);

        /// <summary>
        /// Gets whether this node contains a dictionary.
        /// </summary>
        /// <remarks>Usually a dictionary is also a collection.</remarks>
        /// <seealso cref="HasCollection"/>
        public bool HasDictionary => DictionaryDescriptor.IsDictionary(Type);

        /// <inheritdoc/>
        public int VisibleChildrenCount { get { return visibleChildrenCount; } private set { SetValue(ref visibleChildrenCount, value); } }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> IsVisibleChanged;

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
        public IReadOnlyCollection<INodePresenter> NodePresenters => nodePresenters;

        /// <summary>
        /// Indicates whether the given name is reserved for the name of a property in an <see cref="NodeViewModel"/>. Any children node with a colliding name will
        /// be escaped with the <see cref="EscapeName"/> method.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns><c>True</c> if the name is reserved, <c>false</c> otherwise.</returns>
        public static bool IsReserved(string name)
        {
            return ReservedNames.Contains(name);
        }

        /// <summary>
        /// Escapes the name of a child to avoid name collision with a property.
        /// </summary>
        /// <param name="name">The name to escape.</param>
        /// <returns>The escaped name.</returns>
        /// <remarks>Names are escaped using a trailing underscore character.</remarks>
        public static string EscapeName(string name)
        {
            var escaped = !IsReserved(name) ? name : name + "_";
            return escaped.Replace(".", "-");
        }

        public void FinishInitialization()
        {
            if (initializingChildren != null)
            {
                OnPropertyChanging(nameof(Children));
                foreach (var child in initializingChildren)
                {
                    children.Add(child);
                }
                initializingChildren = null;
                OnPropertyChanged(nameof(Children));
            }

            var commonCommands = new Dictionary<INodePresenterCommand, int>();
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

            var commonAttachedProperties = nodePresenters.SelectMany(x => x.AttachedProperties).GroupBy(x => x.Key).ToList();
            foreach (var attachedProperty in commonAttachedProperties)
            {
                var combiner = attachedProperty.Key.Metadatas.OfType<PropertyCombinerMetadata>().FirstOrDefault()?.Combiner ?? DefaultCombineAttachedProperty;
                var values = attachedProperty.Select(x => x.Value).ToList();
                var value = values.Count == 1 ? values[0] : combiner(values);
                AddAssociatedData(attachedProperty.Key.Name, value);
            }
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
                    if (!AreValueEqual(currentValue, nodePresenter.Value))
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
                var oldValue = nodePresenter.Value;
                if (!Equals(oldValue, newValue))
                {
                    nodePresenter.UpdateValue(newValue);
                }
            }
        }

        /// <summary>
        /// Returns the child node with the matching name.
        /// </summary>
        /// <param name="name">The name of the <see cref="NodeViewModel"/> to look for.</param>
        /// <returns>The corresponding child node, or <c>null</c> if no child with the given name exists.</returns>
        public INodeViewModel GetChild(string name)
        {
            name = EscapeName(name);
            return Children.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Returns the command with the matching name.
        /// </summary>
        /// <param name="name">The name of the command to look for.</param>
        /// <returns>The corresponding command, or <c>null</c> if no command with the given name exists.</returns>
        public ICommandBase GetCommand(string name)
        {
            name = EscapeName(name);
            return Commands.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Returns the additionnal data with the matching name.
        /// </summary>
        /// <param name="name">The name of the additionnal data to look for.</param>
        /// <returns>The corresponding additionnal data, or <c>null</c> if no data with the given name exists.</returns>
        public object GetAssociatedData(string name)
        {
            name = EscapeName(name);
            return AssociatedData.FirstOrDefault(x => x.Key == name).Value;
        }

        /// <summary>
        /// Returns the child node, the command or the additional data with the matching name.
        /// </summary>
        /// <param name="name">The name of the object to look for.</param>
        /// <returns>The corresponding object, or <c>null</c> if no object with the given name exists.</returns>
        public object GetDynamicObject(string name)
        {
            name = EscapeName(name);
            return GetChild(name) ?? GetCommand(name) ?? GetAssociatedData(name) ?? DependencyProperty.UnsetValue;
        }

        /// <inheritdoc/>
        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new NodeViewModelDynamicMetaObject(parameter, this);
        }

        internal void NotifyPropertyChanging(string propertyName)
        {
            if (!changingProperties.Contains(propertyName))
            {
                changingProperties.Add(propertyName);
                OnPropertyChanging(propertyName, GraphViewModel.HasChildPrefix + propertyName);
            }
        }

        internal void NotifyPropertyChanged(string propertyName)
        {
            if (changingProperties.Remove(propertyName))
            {
                OnPropertyChanged(propertyName, GraphViewModel.HasChildPrefix + propertyName);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}: [{GetNodeValue()}]";
        }

        protected void AddAssociatedData(string key, object value)
        {
            if (initializingChildren == null)
            {
                OnPropertyChanging(key);
            }
            associatedData.Add(key, value);
            if (initializingChildren == null)
            {
                OnPropertyChanged(key);
            }
        }

        public bool RemoveAssociatedData(string key)
        {
            if (initializingChildren == null)
            {
                OnPropertyChanging(key);
            }
            var result = associatedData.Remove(key);
            if (initializingChildren == null)
            {
                OnPropertyChanged(key);
            }
            return result;
        }

        /// <summary>
        /// Refreshes this node, updating its properties and its child nodes.
        /// </summary>
        public void Refresh()
        {
            foreach (var child in Children.ToList())
            {
                RemoveChild((NodeViewModel)child);
            }
            foreach (var command in Commands.ToList())
            {
                RemoveCommand(command);
            }
            foreach (var data in AssociatedData.ToList())
            {
                RemoveAssociatedData(data.Key);
            }

            Owner.GraphViewModelService.NodeViewModelFactory.GenerateChildren(Owner, this, nodePresenters);
            FinishInitialization();
        }


        /// <summary>
        /// Adds the given node to the list of children of this node.
        /// </summary>
        /// <param name="child">The node to add as child.</param>
        protected void AddChild(NodeViewModel child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (child.Parent != null) throw new InvalidOperationException("The node already have a parent.");
            if (Children.Contains(child)) throw new InvalidOperationException("The node is already in the children list of its parent.");
            if (initializingChildren == null)
            {
                NotifyPropertyChanging(child.Name);
            }
            child.Parent = this;

            if (initializingChildren == null)
            {
                children.Add(child);
                NotifyPropertyChanged(child.Name);
            }
            else
            {
                initializingChildren.Add(child);
            }
            if (child.IsVisible)
                ++VisibleChildrenCount;
            child.IsVisibleChanged += ChildVisibilityChanged;
        }

        protected void RemoveChild(NodeViewModel node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (!Children.Contains(node)) throw new InvalidOperationException("The node is not in the children list of its parent.");

            if (node.IsVisible)
                --VisibleChildrenCount;
            node.IsVisibleChanged -= ChildVisibilityChanged;

            if (initializingChildren == null)
            {
                NotifyPropertyChanging(node.Name);
            }

            node.Parent = null;
            if (initializingChildren == null)
            {
                children.Remove(node);
                NotifyPropertyChanged(node.Name);
            }
            else
            {
                initializingChildren.Remove(node);
            }
        }

        /// <summary>
        /// Adds the provided <paramref name="command"/> to this node.
        /// </summary>
        /// <param name="command">The command to add.</param>
        public void AddCommand([NotNull] INodeCommandWrapper command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            OnPropertyChanging($"{GraphViewModel.HasCommandPrefix}{command.Name}");
            OnPropertyChanging(command.Name);
            commands.Add(command);
            OnPropertyChanged(command.Name);
            OnPropertyChanged($"{GraphViewModel.HasCommandPrefix}{command.Name}");
        }

        /// <summary>
        /// Removes the provided <paramref name="command"/> from this node, if it exists.
        /// </summary>
        /// <param name="command">The command to remove.</param>
        /// <returns><c>true</c> if the command was sucessfully removed; otherwise, <c>false</c>.</returns>
        public bool RemoveCommand([NotNull] INodeCommandWrapper command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            OnPropertyChanging($"{GraphViewModel.HasCommandPrefix}{command.Name}");
            OnPropertyChanging(command.Name);
            var removed = commands.Remove(command);
            OnPropertyChanged(command.Name);
            OnPropertyChanged($"{GraphViewModel.HasCommandPrefix}{command.Name}");
            return removed;
        }

        protected void CheckDynamicMemberConsistency()
        {
            var memberNames = new HashSet<string>();
            foreach (var child in Children)
            {
                if (string.IsNullOrWhiteSpace(child.Name))
                    throw new InvalidOperationException("This node has a child with a null or blank name");

                if (child.Name.Contains('.'))
                    throw new InvalidOperationException($"This node has a child which contains a period (.) in its name: {child.Name}");

                if (memberNames.Contains(child.Name))
                    throw new InvalidOperationException($"This node contains several members named {child.Name}");

                memberNames.Add(child.Name);
            }

            foreach (var command in Commands.OfType<NodeCommandWrapperBase>())
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                    throw new InvalidOperationException("This node has a command with a null or blank name {0}");

                if (memberNames.Contains(command.Name))
                    throw new InvalidOperationException($"This node contains several members named {command.Name}");

                memberNames.Add(command.Name);
            }

            foreach (var associatedDataKey in AssociatedData.Keys)
            {
                if (string.IsNullOrWhiteSpace(associatedDataKey))
                    throw new InvalidOperationException("This node has associated data with a null or blank name {0}");

                if (memberNames.Contains(associatedDataKey))
                    throw new InvalidOperationException($"This node contains several members named {associatedDataKey}");

                memberNames.Add(associatedDataKey);
            }
        }

        private static bool DebugQuantumPropertyChanges = true;

        protected override void OnPropertyChanging(params string[] propertyNames)
        {
            if (DebugQuantumPropertyChanges && HasPropertyChangingSubscriber)
            {
                foreach (var property in propertyNames)
                {
                    Owner.Logger.Debug($"Node Property changing: [{Path}].{property}");
                }
            }
            base.OnPropertyChanging(propertyNames);
        }

        protected override void OnPropertyChanged(params string[] propertyNames)
        {
            if (DebugQuantumPropertyChanges && HasPropertyChangedSubscriber)
            {
                foreach (var property in propertyNames)
                {
                    Owner.Logger.Debug($"Node Property changed: [{Path}].{property}");
                }
            }
            if (DisplayNameProvider != null && DisplayNameDependentProperties != null)
            {
                if (propertyNames.Any(x => DisplayNameDependentProperties.Contains(x)))
                {
                    DisplayName = DisplayNameProvider();
                }
            }
            base.OnPropertyChanged(propertyNames);
        }

        protected virtual bool AreValueEqual(object value1, object value2)
        {
            return Equals(value1, value2);
        }

        private void ChildVisibilityChanged(object sender, EventArgs e)
        {
            var node = (INodeViewModel)sender;
            if (node.IsVisible)
                ++VisibleChildrenCount;
            else
                --VisibleChildrenCount;
        }

        private void ValueChanging(object sender, ValueChangingEventArgs valueChangingEventArgs)
        {
            ((NodeViewModel)Parent)?.NotifyPropertyChanging(Name);
            OnPropertyChanging(nameof(NodeValue));
        }

        private void ValueChanged(object sender, ValueChangedEventArgs valueChangedEventArgs)
        {
            ((NodeViewModel)Parent)?.NotifyPropertyChanged(Name);

            // This node can have been disposed by its parent already (if its parent is being refreshed and share the same source node)
            // In this case, let's trigger the notifications gracefully before being discarded, but skip refresh
            if (!IsDestroyed)
            {
                Refresh();
            }

            OnPropertyChanged(nameof(NodeValue));
            UpdateViewModelProperties();
            Owner.NotifyNodeChanged(Path);
        }

        private void UpdateViewModelProperties()
        {
            var shouldBeVisible = false;
            var shouldBeReadOnly = false;

            foreach (var nodePresenter in nodePresenters)
            {
                // Display this node if at least one presenter is visible
                if (nodePresenter.IsVisible)
                    shouldBeVisible = true;

                // Make it read-only if at least one presenter is read-only
                if (nodePresenter.IsReadOnly)
                    shouldBeReadOnly = true;
            }

            IsVisible = shouldBeVisible;
            IsReadOnly = shouldBeReadOnly;

            // TODO: find a way to "merge" display name if they are different (string.Join?)
            DisplayName = Utils.SplitCamelCase(nodePresenters.First().DisplayName);
        }

        private static int CompareChildren(INodeViewModel a, INodeViewModel b)
        {
            // Order has the best priority for comparison, if set.
            if ((a.Order ?? 0) != (b.Order ?? 0))
                return (a.Order ?? 0).CompareTo(b.Order ?? 0);

            // Then, try to use metadata token (if members)
            if (a.MemberInfo != null || b.MemberInfo != null)
            {
                var comparison = a.MemberInfo.CompareMetadataTokenWith(b.MemberInfo);
                if (comparison != 0)
                    return comparison;
            }

            // Then we use name, only if both orders are unset.
            if (a.Order == null && b.Order == null)
            {
                return string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase);
            }

            // Otherwise, the first child would be the one who have an order value.
            return a.Order == null ? 1 : -1;
        }

        private object ConvertValue(object value)
        {
            if (Type.IsInstanceOfType(value))
                return value;

            if (value is IConvertible)
            {
                var typeCode = Type.GetTypeCode(Type);
                if (typeCode != TypeCode.Empty && typeCode != TypeCode.Object)
                {
                    return Convert.ChangeType(value, Type);
                }
            }

            return TypeDescriptor.GetConverter(Type).ConvertFrom(value);
        }

        private static object DefaultCombineAttachedProperty(IEnumerable<object> arg)
        {
            object result = null;
            bool isFirst = true;
            foreach (var value in arg)
            {
                if (isFirst)
                    result = value;
                else
                    result = Equals(result, value) ? result : DifferentValues;
                isFirst = false;
            }
            return result;
        }
    }
}
