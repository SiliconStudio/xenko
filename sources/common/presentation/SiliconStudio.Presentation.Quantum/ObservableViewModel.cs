// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
    /// <param name="graphNodePath">The <see cref="GraphNodePath"/> corresponding to the given node.</param>
    /// <param name="contentType">The type of content contained by the new <see cref="ObservableModelNode"/>.</param>
    /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <see cref="Index.Empty"/> must be passed otherwise</param>
    /// <returns>A new instance of <see cref="ObservableModelNode"/> corresponding to the given parameters.</returns>
    public delegate ObservableModelNode CreateNodeDelegate(ObservableViewModel viewModel, string baseName, bool isPrimitive, IGraphNode modelNode, GraphNodePath graphNodePath, Type contentType, Index index);

    public class ObservableViewModel : DispatcherViewModel
    {
        public const string DefaultLoggerName = "Quantum";
        public const string HasChildPrefix = "HasChild_";
        public const string HasCommandPrefix = "HasCommand_";
        public const string HasAssociatedDataPrefix = "HasAssociatedData_";

        private readonly HashSet<string> combinedNodeChanges = new HashSet<string>();
        private IObservableNode rootNode;
        private ObservableViewModel parent;
        private List<ObservableViewModel> children = new List<ObservableViewModel>();

        private Func<CombinedObservableNode, object, string> formatCombinedUpdateMessage = (node, value) => $"Update property '{node.Name}'";

        public static readonly CreateNodeDelegate DefaultObservableNodeFactory = DefaultCreateNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="ObservableViewModelService"/> to use for this view model.</param>
        private ObservableViewModel(IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            ObservableViewModelService = serviceProvider.Get<ObservableViewModelService>();
            Logger = GlobalLogger.GetLogger(DefaultLoggerName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="ObservableViewModelService"/> to use for this view model.</param>
        /// <param name="propertyProvider">The object providing properties to display</param>
        /// <param name="graphNode">The root node of the view model to generate.</param>
        private ObservableViewModel(IViewModelServiceProvider serviceProvider, IPropertiesProviderViewModel propertyProvider, IGraphNode graphNode)
            : this(serviceProvider)
        {
            if (graphNode == null) throw new ArgumentNullException(nameof(graphNode));
            PropertiesProvider = propertyProvider;
            var node = ObservableViewModelService.ObservableNodeFactory(this, "Root", graphNode.Content.IsPrimitive, graphNode, new GraphNodePath(graphNode), graphNode.Content.Type, Index.Empty);
            node.Initialize();
            RootNode = node;
            node.CheckConsistency();
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            RootNode.Children.SelectDeep(x => x.Children).ForEach(x => x.Destroy());
            RootNode.Destroy();
        }

        public static ObservableViewModel Create(IViewModelServiceProvider serviceProvider, IPropertiesProviderViewModel propertyProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            if (propertyProvider == null) throw new ArgumentNullException(nameof(propertyProvider));

            if (!propertyProvider.CanProvidePropertiesViewModel)
                return null;

            var rootNode = propertyProvider.GetRootNode();
            if (rootNode == null)
                return null;

            return new ObservableViewModel(serviceProvider, propertyProvider, rootNode);
        }

        public static ObservableViewModel CombineViewModels(IViewModelServiceProvider serviceProvider, IReadOnlyCollection<ObservableViewModel> viewModels)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));
            var combinedViewModel = new ObservableViewModel(serviceProvider);

            var rootNodes = new List<ObservableModelNode>();
            foreach (var viewModel in viewModels)
            {
                if (!(viewModel.RootNode is SingleObservableNode))
                    throw new ArgumentException(@"The view models to combine must contains SingleObservableNode.", nameof(viewModels));

                viewModel.parent = combinedViewModel;
                combinedViewModel.children.Add(viewModel);
                var rootNode = (ObservableModelNode)viewModel.RootNode;
                rootNodes.Add(rootNode);
            }

            if (rootNodes.Count < 2)
                throw new ArgumentException(@"Called CombineViewModels with a collection of view models that is either empty or containt just a single item.", nameof(viewModels));

            // Find best match for the root node type
            var rootNodeType = rootNodes.First().Root.Type;
            if (rootNodes.Skip(1).Any(x => x.Type != rootNodeType))
                rootNodeType = typeof(object);

            CombinedObservableNode rootCombinedNode = CombinedObservableNode.Create(combinedViewModel, "Root", null, rootNodeType, rootNodes, Index.Empty);
            rootCombinedNode.Initialize();
            combinedViewModel.RootNode = rootCombinedNode;
            return combinedViewModel;
        }

        /// <summary>
        /// Gets the root node of this observable view model.
        /// </summary>
        public IObservableNode RootNode { get { return rootNode; } private set { SetValue(ref rootNode, value); } }
        
        /// <summary>
        /// Gets or sets a function that will generate a message for the action stack when combined nodes are modified. The function will receive
        /// the modified combined node and the new value as parameters and should return a string corresponding to the message to add to the action stack.
        /// </summary>
        public Func<CombinedObservableNode, object, string> FormatCombinedUpdateMessage { get { return formatCombinedUpdateMessage; } set { if (value == null) throw new ArgumentException("The value cannot be null."); formatCombinedUpdateMessage = value; } }
        
        /// <summary>
        /// Gets the <see cref="ObservableViewModelService"/> associated to this view model.
        /// </summary>
        public ObservableViewModelService ObservableViewModelService { get; }

        ///// <summary>
        ///// Gets the <see cref="NodeContainer"/> used to store Quantum objects.
        ///// </summary>
        //public NodeContainer NodeContainer { get; }

        /// <summary>
        /// Gets the object providing properties for this view model.
        /// </summary>
        public IPropertiesProviderViewModel PropertiesProvider { get; }

        /// <summary>
        /// Gets the <see cref="Logger"/> associated to this view model.
        /// </summary>
        public Logger Logger { get; private set; }

        /// <summary>
        /// Gets the parent of this view model.
        /// </summary>
        public ObservableViewModel Parent => parent;

        /// <summary>
        /// Gets the children of this view model (in case of combined node).
        /// </summary>
        public IReadOnlyList<ObservableViewModel> Children => children;

        /// <summary>
        /// Raised when the value of an <see cref="IObservableNode"/> contained into this view model has changed.
        /// </summary>
        /// <remarks>If this view model contains <see cref="CombinedObservableNode"/> instances, this event will be raised only once, at the end of the transaction.</remarks>
        public event EventHandler<ObservableViewModelNodeValueChangedArgs> NodeValueChanged;

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
            parent?.combinedNodeChanges.Add(observableNodePath);
            NodeValueChanged?.Invoke(this, new ObservableViewModelNodeValueChangedArgs(this, observableNodePath));
        }

        internal CombinedActionsContext BeginCombinedAction(string actionName, string observableNodePath)
        {
            return new CombinedActionsContext(this, actionName, observableNodePath);
        }

        internal void EndCombinedAction(string observableNodePath)
        {
            var handler = NodeValueChanged;
            if (handler != null)
            {
                foreach (var nodeChange in combinedNodeChanges)
                {
                    handler(this, new ObservableViewModelNodeValueChangedArgs(this, nodeChange));
                }
            }
            combinedNodeChanges.Clear();
        }

        private static ObservableModelNode DefaultCreateNode(ObservableViewModel viewModel, string baseName, bool isPrimitive, IGraphNode modelNode, GraphNodePath graphNodePath, Type contentType, Index index)
        {
            return ObservableModelNode.Create(viewModel, baseName, isPrimitive, modelNode, graphNodePath, contentType, index);
        }
    }
}
