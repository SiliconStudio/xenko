// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Windows;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Collections;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using Expression = System.Linq.Expressions.Expression;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class NodeViewModel : DispatcherViewModel, INodeViewModel, IDynamicMetaObjectProvider
    {
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

        static NodeViewModel()
        {
            typeof(NodeViewModel).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
            ReservedNames.Add("TypedValue");
            ReservedNames.Add("Type");
        }

        protected NodeViewModel(GraphViewModel ownerViewModel, Type type, Index index)
            : base(ownerViewModel.ServiceProvider)
        {
            DependentProperties.Add(nameof(Path), new[] { nameof(DisplayPath) });
            Owner = ownerViewModel;
            Type = type;
            Index = index;
            IsVisible = true;
            IsReadOnly = false;
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
        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets the name used to display the node to the user.
        /// </summary>
        public string DisplayName { get { return displayName; } set { SetValue(ref displayName, value); } }

        /// <summary>
        /// Gets the path of this node. The path is constructed from the name of all nodes from the root to this one, separated by periods.
        /// </summary>
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
        /// Gets whether this node contains a primitive value. A primitive value has no children node and does not need to refresh its hierarchy when its value is modified.
        /// </summary>
        public abstract bool IsPrimitive { get; }

        /// <summary>
        /// Gets or sets whether this node should be displayed in the view.
        /// </summary>
        public bool IsVisible { get { return isVisible; } set { SetValue(ref isVisible, value, () => IsVisibleChanged?.Invoke(this, EventArgs.Empty)); } }

        /// <summary>
        /// Gets or sets whether this node can be modified in the view.
        /// </summary>
        public bool IsReadOnly { get { return isReadOnly; } set { SetValue(ref isReadOnly, value); } }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        protected internal abstract object InternalNodeValue { get; set; }

        /// <inheritdoc/>
        public object NodeValue { get { return InternalNodeValue; } set { InternalNodeValue = ConvertValue(value); } }

        /// <summary>
        /// Gets or sets the index of this node, relative to its parent node when its contains a collection. Can be null of this node is not in a collection.
        /// </summary>
        public Index Index { get; }

        public bool HasIndex => Index != Index.Empty;

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
        public abstract int? Order { get; }

        /// <summary>
        /// Gets the member info (if any).
        /// </summary>
        public virtual MemberInfo MemberInfo => null;

        /// <summary>
        /// Gets whether this node contains a collection.
        /// </summary>
        /// <remarks>Used mostly for sorting purpose.</remarks>
        /// <seealso cref="HasDictionary"/>
        public abstract bool HasCollection { get; }

        /// <summary>
        /// Gets whether this node contains a dictionary.
        /// </summary>
        /// <remarks>Usually a dictionary is also a collection.</remarks>
        /// <seealso cref="HasCollection"/>
        public abstract bool HasDictionary { get; }

        /// <inheritdoc/>
        public int VisibleChildrenCount { get { return visibleChildrenCount; } private set { SetValue(ref visibleChildrenCount, value); } }

        internal new bool IsDestroyed => base.IsDestroyed;

        /// <inheritdoc/>
        [Obsolete("This event is deprecated, IContent.Changed should be used instead")] // Unless needed for virtual/combined nodes?
        public event EventHandler<EventArgs> ValueChanged;
        
        /// <inheritdoc/>
        public event EventHandler<EventArgs> IsVisibleChanged;

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
            return !IsReserved(name) ? name : name + "_";
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            EnsureNotDestroyed(Name);
            base.Destroy();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}: [{InternalNodeValue}]";
        }

        public void AddAssociatedData(string key, object value)
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

        public void AddOrUpdateAssociatedData(string key, object value)
        {
            if (initializingChildren == null)
            {
                OnPropertyChanging(key);
            }
            associatedData[key] = value;
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
        protected abstract void Refresh();

        /// <summary>
        /// Indicates whether this node can be moved.
        /// </summary>
        /// <param name="newParent">The new parent of the node once moved.</param>
        /// <returns><c>true</c> if the node can be moved, <c>fals</c> otherwise.</returns>
        public bool CanMove(INodeViewModel newParent)
        {
            if (newParent is CombinedNodeViewModel)
                return false;

            var parent = newParent;
            while (parent != null)
            {
                if (parent == this)
                    return false;
                parent = parent.Parent;
            }
            return true;
        }

        /// <summary>
        /// Moves the node by setting it a new parent.
        /// </summary>
        /// <param name="newParent">The new parent of the node once moved.</param>
        /// <param name="newName">The new name to give to the node once moved. This will modify its path. If <c>null</c>, it does not modify the name.</param>
        public void Move(INodeViewModel newParent, string newName = null)
        {
            if (this is CombinedNodeViewModel)
                throw new InvalidOperationException("A CombinedNodeViewModel cannot be moved.");
            if (newParent is CombinedNodeViewModel)
                throw new ArgumentException("The new parent cannot be a CombinedNodeViewModel");

            var parent = (NodeViewModel)newParent;
            while (parent != null)
            {
                if (parent == this)
                    throw new InvalidOperationException("A node cannot be moved into itself or one of its children.");
                parent = (NodeViewModel)parent.Parent;
            }

            if (newParent.Children.Any(x => (newName == null && x.Name == Name) || x.Name == newName))
                throw new InvalidOperationException("Unable to move this node, a node with the same name already exists.");

            if (Parent != null)
            {
                parent = (NodeViewModel)Parent;
                parent.RemoveChild(this);
            }

            if (newName != null)
            {
                Name = newName;
            }
            ((NodeViewModel)newParent).AddChild(this);
        }
        
        /// <summary>
        /// Returns the child node with the matching name.
        /// </summary>
        /// <param name="name">The name of the <see cref="NodeViewModel"/> to look for.</param>
        /// <returns>The corresponding child node, or <c>null</c> if no child with the given name exists.</returns>
        public NodeViewModel GetChild(string name)
        {
            name = EscapeName(name);
            return (NodeViewModel)Children.FirstOrDefault(x => x.Name == name);
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

        /// <summary>
        /// Finalizes the initialization of this node.
        /// </summary>
        /// <remarks>This method is called after all sibling of this node have been initialized.</remarks>
        protected internal virtual void FinalizeInitialization()
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
        protected void AddCommand([NotNull] INodeCommandWrapper command)
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
        protected bool RemoveCommand([NotNull] INodeCommandWrapper command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            OnPropertyChanging($"{GraphViewModel.HasCommandPrefix}{command.Name}");
            OnPropertyChanging(command.Name);
            var removed = commands.Remove(command);
            OnPropertyChanged(command.Name);
            OnPropertyChanged($"{GraphViewModel.HasCommandPrefix}{command.Name}");
            return removed;
        }

        protected void ClearCommands()
        {
            var commandNames = commands.Select(x => x.Name).ToList();
            foreach (string commandName in commandNames)
            {
                OnPropertyChanging($"{GraphViewModel.HasCommandPrefix}{commandName}");
                OnPropertyChanging(commandName);
            }
            commands.Clear();
            for (int i = commandNames.Count - 1; i >= 0; --i)
            {
                OnPropertyChanged(commandNames[i]);
                OnPropertyChanged($"{GraphViewModel.HasCommandPrefix}{commandNames[i]}");
            }
        }

        protected void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);

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

            foreach (var command in Commands.OfType<ModelNodeCommandWrapper>())
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
            base.OnPropertyChanged(propertyNames);
        }

        private void ChildVisibilityChanged(object sender, EventArgs e)
        {
            var node = (INodeViewModel)sender;
            if (node.IsVisible)
                ++VisibleChildrenCount;
            else
                --VisibleChildrenCount;
        }

        private static int CompareChildren(INodeViewModel a, INodeViewModel b)
        {
            // Order has the best priority for comparison, if set.
            if ((a.Order ?? 0) != (b.Order ?? 0))
                return (a.Order ?? 0).CompareTo(b.Order ?? 0);

            // Then we use index, if they are set and comparable.
            if (!a.Index.IsEmpty && !b.Index.IsEmpty)
            {
                if (a.Index.Value.GetType() == b.Index.Value.GetType())
                {
                    return a.Index.CompareTo(b.Index); 
                }
            }

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
    }
}
