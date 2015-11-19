// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using SiliconStudio.ActionStack;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// A factory that creates an <see cref="ObservableModelNode"/> from a set of parameters.
    /// </summary>
    /// <param name="viewModel">The <see cref="ObservableViewModel"/> that owns the new <see cref="ObservableModelNode"/>.</param>
    /// <param name="baseName">The base name of this node. Can be null if <see paramref="index"/> is not. If so a name will be automatically generated from the index.</param>
    /// <param name="isPrimitive">Indicate whether this node should be considered as a primitive node.</param>
    /// <param name="modelNode">The model node bound to the new <see cref="ObservableModelNode"/>.</param>
    /// <param name="modelNodePath">The <see cref="ModelNodePath"/> corresponding to the given node.</param>
    /// <param name="contentType">The type of content contained by the new <see cref="ObservableModelNode"/>.</param>
    /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <c>null</c> must be passed otherwise</param>
    /// <returns>A new instance of <see cref="ObservableModelNode"/> corresponding to the given parameters.</returns>
    public delegate ObservableModelNode CreateNodeDelegate(ObservableViewModel viewModel, string baseName, bool isPrimitive, IGraphNode modelNode, ModelNodePath modelNodePath, Type contentType, object index);

    public class ObservableViewModel : EditableViewModel, IDisposable
    {
        public const string DefaultLoggerName = "Quantum";
        public const string HasChildPrefix = "HasChild_";
        public const string HasCommandPrefix = "HasCommand_";
        public const string HasAssociatedDataPrefix = "HasAssociatedData_";

        private readonly IEnumerable<IDirtiable> dirtiables;
        private readonly HashSet<string> nodeChangeList = new HashSet<string>();
        private IObservableNode rootNode;
        private ObservableViewModel parent;

        private Func<SingleObservableNode, object, string> formatSingleUpdateMessage = (node, value) => $"Update '{node.Name}'";
        private Func<CombinedObservableNode, object, string> formatCombinedUpdateMessage = (node, value) => $"Update '{node.Name}'";

        public static readonly CreateNodeDelegate DefaultObservableNodeFactory = DefaultCreateNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="ObservableViewModelService"/> to use for this view model.</param>
        /// <param name="modelContainer">A <see cref="ModelContainer"/> to use to build view model nodes.</param>
        /// <param name="dirtiables">The list of <see cref="IDirtiable"/> objects linked to this view model.</param>
        private ObservableViewModel(IViewModelServiceProvider serviceProvider, ModelContainer modelContainer, IEnumerable<IDirtiable> dirtiables)
            : base(serviceProvider)
        {
            if (modelContainer == null) throw new ArgumentNullException(nameof(modelContainer));
            if (dirtiables == null) throw new ArgumentNullException(nameof(dirtiables));
            ModelContainer = modelContainer;
            this.dirtiables = dirtiables;
            this.dirtiables.ForEach(x => x.DirtinessUpdated += DirtinessUpdated);
            ObservableViewModelService = serviceProvider.Get<ObservableViewModelService>();
            Logger = GlobalLogger.GetLogger(DefaultLoggerName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="ObservableViewModelService"/> to use for this view model.</param>
        /// <param name="modelContainer">A <see cref="ModelContainer"/> to use to build view model nodes.</param>
        /// <param name="modelNode">The root model node of the view model to generate.</param>
        /// <param name="dirtiables">The list of <see cref="IDirtiable"/> objects linked to this view model.</param>
        public ObservableViewModel(IViewModelServiceProvider serviceProvider, ModelContainer modelContainer, IGraphNode modelNode, IEnumerable<IDirtiable> dirtiables)
            : this(serviceProvider, modelContainer, dirtiables.SafeArgument("dirtiables").ToList())
        {
            if (modelNode == null) throw new ArgumentNullException(nameof(modelNode));
            var node = ObservableViewModelService.ObservableNodeFactory(this, "Root", modelNode.Content.IsPrimitive, modelNode, new ModelNodePath(modelNode), modelNode.Content.Type, null);
            Identifier = new ObservableViewModelIdentifier(node.ModelGuid);
            node.Initialize();
            RootNode = node;
            node.CheckConsistency();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dirtiables.ForEach(x => x.DirtinessUpdated -= DirtinessUpdated);
        }

        public static ObservableViewModel CombineViewModels(IViewModelServiceProvider serviceProvider, ModelContainer modelContainer, IReadOnlyCollection<ObservableViewModel> viewModels)
        {
            if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));
            var combinedViewModel = new ObservableViewModel(serviceProvider, modelContainer, viewModels.SelectMany(x => x.Dirtiables));

            var rootNodes = new List<ObservableModelNode>();
            foreach (var viewModel in viewModels)
            {
                if (!(viewModel.RootNode is SingleObservableNode))
                    throw new ArgumentException(@"The view models to combine must contains SingleObservableNode.", nameof(viewModels));

                viewModel.parent = combinedViewModel;
                var rootNode = (ObservableModelNode)viewModel.RootNode;
                rootNodes.Add(rootNode);
            }

            if (rootNodes.Count < 2)
                throw new ArgumentException(@"Called CombineViewModels with a collection of view models that is either empty or containt just a single item.", nameof(viewModels));

            // Find best match for the root node type
            var rootNodeType = rootNodes.First().Root.Type;
            if (rootNodes.Skip(1).Any(x => x.Type != rootNodeType))
                rootNodeType = typeof(object);

            CombinedObservableNode rootCombinedNode = CombinedObservableNode.Create(combinedViewModel, "Root", null, rootNodeType, rootNodes, null);
            combinedViewModel.Identifier = new ObservableViewModelIdentifier(rootNodes.Select(x => x.ModelGuid));
            rootCombinedNode.Initialize();
            combinedViewModel.RootNode = rootCombinedNode;
            return combinedViewModel;
        }

        /// <inheritdoc/>
        public override IEnumerable<IDirtiable> Dirtiables => dirtiables;

        /// <summary>
        /// Gets the root node of this observable view model.
        /// </summary>
        public IObservableNode RootNode { get { return rootNode; } private set { SetValueUncancellable(ref rootNode, value); } }
        
        /// <summary>
        /// Gets or sets a function that will generate a message for the action stack when a single node is modified. The function will receive
        /// the modified node and the new value as parameters and should return a string corresponding to the message to add to the action stack.
        /// </summary>
        public Func<SingleObservableNode, object, string> FormatSingleUpdateMessage { get { return formatSingleUpdateMessage; } set { if (value == null) throw new ArgumentException("The value cannot be null."); formatSingleUpdateMessage = value; } }

        /// <summary>
        /// Gets or sets a function that will generate a message for the action stack when combined nodes are modified. The function will receive
        /// the modified combined node and the new value as parameters and should return a string corresponding to the message to add to the action stack.
        /// </summary>
        public Func<CombinedObservableNode, object, string> FormatCombinedUpdateMessage { get { return formatCombinedUpdateMessage; } set { if (value == null) throw new ArgumentException("The value cannot be null."); formatCombinedUpdateMessage = value; } }
        
        /// <summary>
        /// Gets the <see cref="ObservableViewModelService"/> associated to this view model.
        /// </summary>
        public ObservableViewModelService ObservableViewModelService { get; }

        /// <summary>
        /// Gets an identifier for this view model.
        /// </summary>
        public ObservableViewModelIdentifier Identifier { get; private set; }

        /// <summary>
        /// Gets the <see cref="ModelContainer"/> used to store Quantum objects.
        /// </summary>
        public ModelContainer ModelContainer { get; }

        /// <summary>
        /// Gets the <see cref="Logger"/> associated to this view model.
        /// </summary>
        public Logger Logger { get; private set; }

        /// <summary>
        /// Raised when the dirtiness of the related <see cref="Dirtiables"/> is updated after a property change.
        /// </summary>
        public event EventHandler<ObservableViewModelDirtinessUpdatedArgs> ViewModelDirtinessUpdated;

        [Pure]
        public IObservableNode ResolveObservableNode(string path)
        {
            var members = path.Split('.');
            if (members[0] != RootNode.Name)
                return null;

            var currentNode = RootNode;
            foreach (var member in members.Skip(1))
            {
                currentNode = currentNode.Children.FirstOrDefault(x => x.Name == member);
                if (currentNode == null)
                    return null;
            }
            return currentNode;
        }

        internal void NotifyNodeChanged(string observableNodePath)
        {
            if (parent != null)
                parent.nodeChangeList.Add(observableNodePath);
            else
                nodeChangeList.Add(observableNodePath);
        }

        internal void RegisterAction(string observableNodePath, DirtiableActionItem actionItem)
        {
            // This must be done before adding the action item to the stack!
            NotifyNodeChanged(observableNodePath);
            ActionStack.Add(actionItem);
        }

        internal void BeginCombinedAction()
        {
            ActionStack.BeginTransaction();
        }

        internal void EndCombinedAction(string displayName, string observableNodePath, object value)
        {
            ActionStack.EndTransaction(displayName, x => new CombinedValueChangedActionItem(displayName, ObservableViewModelService, observableNodePath, Identifier, x));
        }

        private void DirtinessUpdated(object sender, DirtinessUpdatedEventArgs e)
        {
            var handler = ViewModelDirtinessUpdated;
            if (handler != null && nodeChangeList.Count > 0)
            {
                foreach (var nodeChange in nodeChangeList)
                {
                    handler(this, new ObservableViewModelDirtinessUpdatedArgs(this, nodeChange));
                }
            }
            nodeChangeList.Clear();
        }

        private static ObservableModelNode DefaultCreateNode(ObservableViewModel viewModel, string baseName, bool isPrimitive, IGraphNode modelNode, ModelNodePath modelNodePath, Type contentType, object index)
        {
            return ObservableModelNode.Create(viewModel, baseName, isPrimitive, modelNode, modelNodePath, contentType, index);
        }
    }
}
