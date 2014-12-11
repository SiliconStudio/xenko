// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class CombinedObservableNode : ObservableNode
    {
        private readonly List<SingleObservableNode> combinedNodes;
        private readonly List<object> combinedNodeInitialValues;
        private readonly HashSet<object> distinctCombinedNodeInitialValues;
        private readonly int? order;

        protected static readonly HashSet<CombinedObservableNode> ChangedNodes = new HashSet<CombinedObservableNode>();

        protected static bool ChangeInProgress;

        private IDictionary<string, object> associatedData;

        protected CombinedObservableNode(ObservableViewModel ownerViewModel, string name, CombinedObservableNode parentNode, IEnumerable<SingleObservableNode> combinedNodes, object index)
            : base(ownerViewModel, parentNode, index)
        {
            this.combinedNodes = new List<SingleObservableNode>(combinedNodes);
            Name = name;
            DisplayName = this.combinedNodes.First().DisplayName;

            combinedNodeInitialValues = new List<object>();
            distinctCombinedNodeInitialValues = new HashSet<object>();

            bool isReadOnly = false;
            bool isVisible = false;
            bool nullOrder = false;

            foreach (var node in this.combinedNodes)
            {
                if (node.IsReadOnly)
                    isReadOnly = true;

                if (node.IsVisible)
                    isVisible = true;

                if (node.Order == null)
                    nullOrder = true;

                if (order == node.Order || (!nullOrder && order == null))
                    order = node.Order;

                combinedNodeInitialValues.Add(node.Value);
                distinctCombinedNodeInitialValues.Add(node.Value);
                node.PropertyChanged += NodePropertyChanged;
            }
            IsReadOnly = isReadOnly;
            IsVisible = isVisible;

            ResetInitialValues = new AnonymousCommand(ServiceProvider, () => { Owner.BeginCombinedAction(); CombinedNodes.Zip(combinedNodeInitialValues).ForEach(x => x.Item1.Value = x.Item2); Refresh(); Owner.EndCombinedAction(Owner.FormatCombinedUpdateMessage(this, null), Path, null); });
        }


        internal void Initialize()
        {
            Initialize(false);
        }

        private void Initialize(bool isUpdating)
        {
            var commandGroups = new Dictionary<string, List<ModelNodeCommandWrapper>>();
            foreach (var node in combinedNodes)
            {
                foreach (var command in node.Commands)
                {
                    var list = commandGroups.GetOrCreateValue(command.Name);
                    list.Add((ModelNodeCommandWrapper)command);
                }
            }

            foreach (var commandGroup in commandGroups)
            {
                var mode = commandGroup.Value.First().CombineMode;
                if (commandGroup.Value.Any(x => x.CombineMode != mode))
                    throw new InvalidOperationException(string.Format("Inconsistent combine mode among command {0}", commandGroup.Key));

                var shouldCombine = mode != CombineMode.DoNotCombine && (mode == CombineMode.AlwaysCombine || commandGroup.Value.Count == combinedNodes.Count);

                if (shouldCombine)
                {
                    var command = new CombinedNodeCommandWrapper(ServiceProvider, commandGroup.Key, Path, Owner.Identifier, commandGroup.Value);
                    AddCommand(command);
                }
            }

            if (!HasList || HasDictionary)
            {
                var commonChildren = GetCommonChildren();
                GenerateChildren(commonChildren, isUpdating);
            }
            else
            {
                var allChildren = GetAllChildrenByValue();
                if (allChildren != null)
                {
                    GenerateListChildren(allChildren, isUpdating);
                }
            }

            if (Owner.ObservableViewModelService != null)
            {
                var data = Owner.ObservableViewModelService.RequestAssociatedData(this, isUpdating);
                SetValue(ref associatedData, data, "AssociatedData");
            }
            CheckDynamicMemberConsistency();
        }

        private void NodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ChangeInProgress)
            {
                ChangedNodes.Add(this);
            }
        }

        internal static CombinedObservableNode Create(ObservableViewModel ownerViewModel, string name, CombinedObservableNode parent, Type contentType, IEnumerable<SingleObservableNode> combinedNodes, object index)
        {
            var node = (CombinedObservableNode)Activator.CreateInstance(typeof(CombinedObservableNode<>).MakeGenericType(contentType), ownerViewModel, name, parent, combinedNodes, index);
            return node;
        }

        /// <inheritdoc/>
        public override sealed bool IsPrimitive { get { return CombinedNodes.All(x => x.IsPrimitive); } }

        public IReadOnlyCollection<SingleObservableNode> CombinedNodes { get { return combinedNodes; } }

        public bool HasMultipleValues { get { if (Type.IsValueType || Type == typeof(string)) return CombinedNodes.Any(x => !Equals(x.Value, CombinedNodes.First().Value)); return Children.Any(x => ((CombinedObservableNode)x).HasMultipleValues); } }

        public bool HasMultipleInitialValues { get { if (Type.IsValueType || Type == typeof(string)) return distinctCombinedNodeInitialValues.Count > 1; return Children.Any(x => ((CombinedObservableNode)x).HasMultipleInitialValues); } }

        public ICommandBase ResetInitialValues { get; private set; }

        public IEnumerable<object> DistinctInitialValues { get { return distinctCombinedNodeInitialValues; } }

        public override int? Order { get { return order; } }

        /// <inheritdoc/>
        public override sealed bool HasList { get { return CombinedNodes.First().HasList; } }

        /// <inheritdoc/>
        public override sealed bool HasDictionary { get { return CombinedNodes.First().HasDictionary; } }

        // TODO: we shall find a better way to handle combined associated data...
        /// <inheritdoc/>
        public override IDictionary<string, object> AssociatedData { get { return associatedData; } }

        // TODO: do not remove from parent if we can avoid it
        public void Refresh()
        {
            if (Parent == null) throw new InvalidOperationException("The node to refresh can be a root node.");

            OnPropertyChanging("TypedValue", "HasMultipleValues", "IsPrimitive", "HasList", "HasDictionary");
            if (CombinedNodes.Any(x => x != null))
            {
                var parent = (CombinedObservableNode)Parent;
                parent.RemoveChild(this);

                if (AreCombinable(CombinedNodes))
                {
                    ClearCommands();

                    foreach (var child in Children.ToList())
                        RemoveChild(child);

                    foreach (var modelNode in CombinedNodes.OfType<ObservableModelNode>())
                        modelNode.Refresh();

                    Initialize(true);
                }
                parent.AddChild(this);
            }
            OnPropertyChanged("TypedValue", "HasMultipleValues", "IsPrimitive", "HasList", "HasDictionary");
        }

        public static bool AreCombinable(IEnumerable<SingleObservableNode> nodes, bool ignoreNameConstraint = false)
        {
            bool firstNode = true;

            Type type = null;
            string name = null;
            object index = null;
            foreach (var node in nodes)
            {
                if (firstNode)
                {
                    type = node.Type;
                    name = node.Name;
                    index = node.Index;
                    firstNode = false;
                }
                else
                {
                    if (node.Type != type)
                        return false;
                    if (!ignoreNameConstraint && node.Name != name)
                        return false;
                    if (!Equals(node.Index, index))
                        return false;
                }
            }
            return true;
        }

        private void GenerateChildren(IEnumerable<KeyValuePair<string, List<SingleObservableNode>>> commonChildren, bool isUpdating)
        {
            foreach (var children in commonChildren)
            {
                var contentType = children.Value.First().Type;
                var index = children.Value.First().Index;
                CombinedObservableNode child = Create(Owner, children.Key, this, contentType, children.Value, index);
                child.Initialize(isUpdating);
                AddChild(child);
            }
        }

        private void GenerateListChildren(IEnumerable<KeyValuePair<object, List<SingleObservableNode>>> allChildren, bool isUpdating)
        {
            int currentIndex = 0;
            foreach (var children in allChildren)
            {
                if (!ShouldCombine(children.Value, CombinedNodes.Count, "(ListItem)", true))
                    continue;

                var contentType = children.Value.First().Type;
                var name = string.Format("Item {0}", currentIndex);
                CombinedObservableNode child = Create(Owner, name, this, contentType, children.Value, currentIndex);
                child.Initialize(isUpdating);
                child.DisplayName = name;
                ++currentIndex;
                AddChild(child);
            }
        }

        private IEnumerable<KeyValuePair<string, List<SingleObservableNode>>> GetCommonChildren()
        {
            var allChildNodes = new Dictionary<string, List<SingleObservableNode>>();
            foreach (var singleNode in CombinedNodes)
            {
                foreach (var observableNode in singleNode.Children)
                {
                    var child = (SingleObservableNode)observableNode;
                    var list = allChildNodes.GetOrCreateValue(child.Name);
                    list.Add(child);
                }
            }

            return allChildNodes.Where(x => ShouldCombine(x.Value, CombinedNodes.Count, x.Key));
        }

        private static bool ShouldCombine(List<SingleObservableNode> nodes, int combineCount, string name, bool ignoreNameConstraint = false)
        {
            CombineMode? combineMode = null;

            if (!AreCombinable(nodes, ignoreNameConstraint))
                return false;

            foreach (var node in nodes)
            {
                if (combineMode == null)
                    combineMode = node.CombineMode;

                if (combineMode != node.CombineMode)
                    throw new InvalidOperationException(string.Format("Inconsistent values of CombineMode in single nodes for child '{0}'", name));
            }

            if (combineMode == CombineMode.DoNotCombine)
                return false;

            return combineMode == CombineMode.AlwaysCombine || nodes.Count == combineCount;
        }

        private IEnumerable<KeyValuePair<object, List<SingleObservableNode>>> GetAllChildrenByValue()
        {
            var allChildNodes = new List<KeyValuePair<object, List<SingleObservableNode>>>();
            foreach (var singleNode in CombinedNodes)
            {
                var usedSlots = new List<List<SingleObservableNode>>();
                foreach (var observableNode in singleNode.Children)
                {
                    var child = (SingleObservableNode)observableNode;
                    if (!child.Type.IsValueType && child.Type != typeof(string))
                        return null;

                    var list = allChildNodes.FirstOrDefault(x => Equals(x.Key, child.Value) && !usedSlots.Contains(x.Value)).Value;
                    if (list == null)
                    {
                        list = new List<SingleObservableNode>();
                        allChildNodes.Add(new KeyValuePair<object, List<SingleObservableNode>>(child.Value, list));
                    }
                    list.Add(child);
                    usedSlots.Add(list);
                }
            }

            return allChildNodes;
        }
    }

    public class CombinedObservableNode<T> : CombinedObservableNode
    {
        public CombinedObservableNode(ObservableViewModel ownerViewModel, string name, CombinedObservableNode parentNode, IEnumerable<SingleObservableNode> combinedNodes, object index)
            : base(ownerViewModel, name, parentNode, combinedNodes, index)
        {
            DependentProperties.Add("TypedValue", new[] { "Value" });
        }

        /// <summary>
        /// Gets or sets the value of this node through a correctly typed property, which is more adapted to binding.
        /// </summary>
        public T TypedValue
        {
            get
            {
                return HasMultipleValues ? default(T) : (T)CombinedNodes.First().Value;
            }
            set
            {
                Owner.BeginCombinedAction();
                ChangeInProgress = true;
                CombinedNodes.Where(x => x.IsVisible).ForEach(x => x.Value = value);
                var changedNodes = ChangedNodes.Where(x => x != this).ToList();
                ChangedNodes.Clear();
                ChangeInProgress = false;
                Refresh();
                changedNodes.ForEach(x => x.Refresh());
                string displayName = Owner.FormatCombinedUpdateMessage(this, value);
                Owner.EndCombinedAction(displayName, Path, value);
            }
        }

        /// <inheritdoc/>
        public override Type Type { get { return typeof(T); } }

        /// <inheritdoc/>
        public override sealed object Value { get { return TypedValue; } set { TypedValue = (T)value; } }
    }
}
