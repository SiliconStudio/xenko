// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
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

        static CombinedObservableNode()
        {
            typeof(CombinedObservableNode).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
        }

        protected CombinedObservableNode(ObservableViewModel ownerViewModel, string name, IEnumerable<SingleObservableNode> combinedNodes, Index index)
            : base(ownerViewModel, index)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            DependentProperties.Add(nameof(Value), new[] { nameof(HasMultipleValues), nameof(IsPrimitive), nameof(HasList), nameof(HasDictionary) });
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
                if (node.IsDestroyed)
                    throw new InvalidOperationException("One of the combined node is already disposed.");

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
            }
            IsReadOnly = isReadOnly;
            IsVisible = isVisible;

            ResetInitialValues = new AnonymousCommand(ServiceProvider, () =>
            {
                using (Owner.BeginCombinedAction(Owner.FormatCombinedUpdateMessage(this, null), Path))
                {
                    CombinedNodes.Zip(combinedNodeInitialValues).ForEach(x => x.Item1.Value = x.Item2);
                    Refresh();
                }
            });
        }

        internal void Initialize()
        {
            var commandGroups = new Dictionary<string, List<ModelNodeCommandWrapper>>();
            foreach (var node in combinedNodes)
            {
                if (node.IsDestroyed)
                    throw new InvalidOperationException("One of the combined node is already disposed.");

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
                    throw new InvalidOperationException($"Inconsistent combine mode among command {commandGroup.Key}");

                var shouldCombine = mode != CombineMode.DoNotCombine && (mode == CombineMode.AlwaysCombine || commandGroup.Value.Count == combinedNodes.Count);

                if (shouldCombine)
                {
                    var command = new CombinedNodeCommandWrapper(ServiceProvider, commandGroup.Key, commandGroup.Value);
                    AddCommand(command);
                }
            }

            if (!HasList || HasDictionary)
            {
                var commonChildren = GetCommonChildren();
                GenerateChildren(commonChildren);
            }
            else
            {
                var commonChildren = GetCommonChildrenInList();
                if (commonChildren != null)
                {
                    // TODO: Disable list children for now - they need to be improved a lot (resulting combinaison is very random, especially for list of ints
                    GenerateChildren(commonChildren);
                }
            }
            foreach (var key in AssociatedData.Keys.ToList())
            {
                RemoveAssociatedData(key);
            }

            // TODO: we add associatedData added to SingleObservableNode this way, but it's a bit dangerous. Maybe we should check that all combined nodes have this data entry, and all with the same value.
            foreach (var singleData in CombinedNodes.SelectMany(x => x.AssociatedData).Where(x => !AssociatedData.ContainsKey(x.Key)))
            {
                AddAssociatedData(singleData.Key, singleData.Value);
            }

            FinalizeChildrenInitialization();

            CheckDynamicMemberConsistency();
        }

        internal static CombinedObservableNode Create(ObservableViewModel ownerViewModel, string name, CombinedObservableNode parent, Type contentType, IEnumerable<SingleObservableNode> combinedNodes, Index index)
        {
            var node = (CombinedObservableNode)Activator.CreateInstance(typeof(CombinedObservableNode<>).MakeGenericType(contentType), ownerViewModel, name, combinedNodes, index);
            return node;
        }

        /// <inheritdoc/>
        public override sealed bool IsPrimitive { get { return CombinedNodes.All(x => x.IsPrimitive); } }

        public IReadOnlyCollection<SingleObservableNode> CombinedNodes => combinedNodes;

        public bool HasMultipleValues => ComputeHasMultipleValues();

        public bool HasMultipleInitialValues => ComputeHasMultipleInitialValues();

        public ICommandBase ResetInitialValues { get; private set; }

        public IEnumerable<object> DistinctInitialValues => distinctCombinedNodeInitialValues;

        public override int? Order => order;

        public bool GroupByType { get; set; }

        /// <inheritdoc/>
        public override sealed bool HasList => CombinedNodes.First().HasList;

        /// <inheritdoc/>
        public override sealed bool HasDictionary => CombinedNodes.First().HasDictionary;

        /// <inheritdoc/>
        public override void Destroy()
        {
            foreach (var node in CombinedNodes.Where(x => !x.IsDestroyed))
            {
                node.Destroy();
            }
            base.Destroy();
        }

        public void Refresh()
        {
            if (Parent == null) throw new InvalidOperationException("The node to refresh can be a root node.");

            if (CombinedNodes.Any(x => x != null))
            {
                var parent = (CombinedObservableNode)Parent;
                parent.NotifyPropertyChanging(Name);
                OnPropertyChanging(nameof(HasMultipleValues), nameof(IsPrimitive), nameof(HasList), nameof(HasDictionary));
                
                if (AreCombinable(CombinedNodes))
                {
                    ClearCommands();

                    // Destroy all children and remove them
                    Children.SelectDeep(x => x.Children).ForEach(x => x.Destroy());
                    foreach (var child in Children.Cast<ObservableNode>().ToList())
                    {
                        RemoveChild(child);
                    }

                    Initialize();
                }

                OnPropertyChanged(nameof(HasMultipleValues), nameof(IsPrimitive), nameof(HasList), nameof(HasDictionary));
                parent.NotifyPropertyChanged(Name);
            }
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

        private void GenerateChildren(IEnumerable<KeyValuePair<string, List<SingleObservableNode>>> commonChildren)
        {
            foreach (var children in commonChildren)
            {
                var contentType = children.Value.First().Type;
                var index = children.Value.First().Index;
                CombinedObservableNode child = Create(Owner, children.Key, this, contentType, children.Value, index);
                AddChild(child);
                child.Initialize();
            }
        }

        private void GenerateListChildren(IEnumerable<KeyValuePair<object, List<SingleObservableNode>>> allChildren)
        {
            int currentIndex = 0;
            foreach (var children in allChildren)
            {
                if (!ShouldCombine(children.Value, CombinedNodes.Count, "(ListItem)", true))
                    continue;

                var contentType = children.Value.First().Type;
                var name = $"Item {currentIndex}";
                CombinedObservableNode child = Create(Owner, name, this, contentType, children.Value, new Index(currentIndex));
                AddChild(child);
                child.Initialize();
                child.DisplayName = name;
                ++currentIndex;
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

        private IEnumerable<KeyValuePair<string, List<SingleObservableNode>>> GetCommonChildrenInList()
        {
            var allChildNodes = new Dictionary<string, List<SingleObservableNode>>();
            ITypeDescriptor singleType = null;
            foreach (var singleNode in CombinedNodes)
            {
                var descriptor = TypeDescriptorFactory.Default.Find(singleNode.Type);
                if (singleType != null && singleType != descriptor)
                    return null;

                singleType = descriptor;
            }

            // If we're in a collection of value type, use usual name-based combination (which should actually be index-based)
            if (singleType.GetInnerCollectionType().IsValueType)
                return GetCommonChildren();

            if (GroupByType)
            {
                return null;
            }
            return GetCommonChildren();
            //// When the collection are not of 
            //foreach (var observableNode in singleNode.Children)
            //{
            //    var child = (SingleObservableNode)observableNode;
            //    var list = allChildNodes.GetOrCreateValue(child.Name);
            //    list.Add(child);
            //}

            //return allChildNodes.Where(x => ShouldCombine(x.Value, CombinedNodes.Count, x.Key));
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
                    throw new InvalidOperationException($"Inconsistent values of CombineMode in single nodes for child '{name}'");
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

        private bool ComputeHasMultipleValues()
        {
            if (IsPrimitive)
                return CombinedNodes.Any(x => !Equals(x.Value, CombinedNodes.First().Value));

            return !AreAllValuesOfTheSameType(CombinedNodes.Select(x => x.Value));
        }

        private bool ComputeHasMultipleInitialValues()
        {
            if (IsPrimitive)
                return distinctCombinedNodeInitialValues.Count > 1;

            return !AreAllValuesOfTheSameType(distinctCombinedNodeInitialValues);
        }

        private static bool AreAllValuesOfTheSameType(IEnumerable<object> values)
        {
            bool first = true;
            bool isNull = false;
            Type type = null;

            foreach (var value in values)
            {
                // Check status of the first value
                if (first)
                {
                    first = false;
                    if (value == null)
                        isNull = true;
                    else
                        type = value.GetType();
                    continue;
                }

                // For every other values...
                if (value != null)
                {
                    // Check if it should be null
                    if (isNull)
                        return false;

                    // Check if its type matches
                    if (type != value.GetType())
                        return false;
                }
                else if (!isNull)
                {
                    // Check if it should be non-null
                    return false;
                }
            }
            return true;
        }
    }

    public class CombinedObservableNode<T> : CombinedObservableNode
    {
        private bool refreshQueued;

        public CombinedObservableNode(ObservableViewModel ownerViewModel, string name, IEnumerable<SingleObservableNode> combinedNodes, Index index)
            : base(ownerViewModel, name, combinedNodes, index)
        {
            DependentProperties.Add(nameof(TypedValue), new[] { nameof(Value) });
            foreach (var node in CombinedNodes)
            {
                node.ValueChanged += CombinedNodeValueChanged;
            }
        }

        private void CombinedNodeValueChanged(object sender, EventArgs e)
        {
            // Defer the refresh of one frame and ensure we execute it only once.
            if (!refreshQueued)
            {
                Dispatcher.InvokeAsync(TriggerRefresh);
                refreshQueued = true;
            }
        }

        private void TriggerRefresh()
        {
            Dispatcher.EnsureAccess();

            if (!refreshQueued)
                return;

            try
            {
                if (!IsPrimitive)
                {
                    Refresh();
                }
            }
            finally
            {
                refreshQueued = false;
            }
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
                var displayName = Owner.FormatCombinedUpdateMessage(this, value);
                using (Owner.BeginCombinedAction(displayName, Path))
                {
                    OnPropertyChanging(nameof(TypedValue));
                    CombinedNodes.ForEach(x => x.Value = value);
                    OnPropertyChanged(nameof(TypedValue));
                }
            }
        }

        /// <inheritdoc/>
        public override Type Type => typeof(T);

        /// <inheritdoc/>
        public override sealed object Value { get { return TypedValue; } set { TypedValue = (T)value; } }
    }
}
