// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// A factory that creates a <see cref="CombinedNodeViewModel"/> from a set of parameters.
    /// </summary>
    /// <param name="viewModel">The <see cref="GraphViewModel"/> that owns the new <see cref="GraphNodeViewModel"/>.</param>
    /// <param name="baseName">The base name of this node. Can be null if <see paramref="index"/> is not. If so a name will be automatically generated from the index.</param>
    /// <param name="contentType">The type of content in the combined node.</param>
    /// <param name="combinedNodes">The nodes to combine.</param>
    /// <param name="index">The index of this node, when this node represent an item of a collection. <see cref="Index.Empty"/> must be passed otherwise</param>
    /// <returns>A new instance of <see cref="CombinedNodeViewModel"/> corresponding to the given parameters.</returns>
    public delegate CombinedNodeViewModel CreateCombinedNodeDelegate(GraphViewModel viewModel, string baseName, Type contentType, IEnumerable<SingleNodeViewModel> combinedNodes, Index index);

    /// <summary>
    /// A view model class to present a graph of <see cref="IContentNode"/> nodes to a view.
    /// </summary>
    public class GraphViewModel : DispatcherViewModel
    {
        public const string DefaultLoggerName = "Quantum";
        public const string HasChildPrefix = "HasChild_";
        public const string HasCommandPrefix = "HasCommand_";
        public const string HasAssociatedDataPrefix = "HasAssociatedData_";

        private readonly HashSet<string> combinedNodeChanges = new HashSet<string>();
        private readonly List<GraphViewModel> children = new List<GraphViewModel>();
        private readonly Dictionary<INodePresenter, IPropertyProviderViewModel> propertiesProviderMap = new Dictionary<INodePresenter, IPropertyProviderViewModel>();
        private INodeViewModel rootNode;

        private Func<CombinedNodeViewModel, object, string> formatCombinedUpdateMessage = (node, value) => $"Update property '{node.Name}'";

        public static readonly CreateCombinedNodeDelegate DefaultCombinedNodeViewModelFactory = DefaultCreateCombinedNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="GraphViewModelService"/> to use for this view model.</param>
        private GraphViewModel(IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            GraphViewModelService = serviceProvider.TryGet<GraphViewModelService>();
            if (GraphViewModelService == null) throw new InvalidOperationException($"{nameof(GraphViewModel)} requires a {nameof(GraphViewModelService)} in the service provider.");
            Logger = GlobalLogger.GetLogger(DefaultLoggerName);
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="GraphViewModel"/> class.
        ///// </summary>
        ///// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="GraphViewModelService"/> to use for this view model.</param>
        ///// <param name="propertyProvider">The object providing properties to display</param>
        ///// <param name="graphNode">The root node of the view model to generate.</param>
        //private GraphViewModel(IViewModelServiceProvider serviceProvider, IPropertyProviderViewModel propertyProvider, IGraphNode graphNode)
        //    : this(serviceProvider)
        //{
        //    if (graphNode == null) throw new ArgumentNullException(nameof(graphNode));
        //    PropertyProvider = propertyProvider;
        //    var node = GraphViewModelService.GraphNodeViewModelFactory(this, "Root", graphNode.IsPrimitive, graphNode, new GraphNodePath(graphNode), graphNode.Type, Index.Empty);
        //    RootNode = node;
        //    node.Initialize();
        //    node.FinalizeInitialization();
        //    node.CheckConsistency();
        //}

        private GraphViewModel(IViewModelServiceProvider serviceProvider, Type type, IEnumerable<Tuple<INodePresenter, IPropertyProviderViewModel>> rootNodes)
            : this(serviceProvider)
        {
            if (rootNode == null) throw new ArgumentNullException(nameof(rootNode));
            var viewModelFactory = new NodeViewModelFactory();
            foreach (var root in rootNodes)
            {
                propertiesProviderMap.Add(root.Item1, root.Item2);
            }
            viewModelFactory.CreateGraph(this, type, propertiesProviderMap.Keys);
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            RootNode.Children.SelectDeep(x => x.Children).ForEach(x => x.Destroy());
            RootNode.Destroy();
        }

        public static GraphViewModel Create(IViewModelServiceProvider serviceProvider, IPropertyProviderViewModel propertyProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            if (propertyProvider == null) throw new ArgumentNullException(nameof(propertyProvider));

            if (!propertyProvider.CanProvidePropertiesViewModel)
                return null;

            var rootNode = propertyProvider.GetRootNode();
            if (rootNode == null)
                return null;

            var factory = serviceProvider.Get<GraphViewModelService>().NodePresenterFactory;
            var node = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode), propertyProvider);
            return new GraphViewModel(serviceProvider, rootNode.Type, Tuple.Create(node, propertyProvider).Yield());
        }

        public static GraphViewModel Create(IViewModelServiceProvider serviceProvider, IReadOnlyCollection<IPropertyProviderViewModel> propertyProviders)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            if (propertyProviders == null) throw new ArgumentNullException(nameof(propertyProviders));
            if (propertyProviders.Count == 0) throw new ArgumentException($@"{nameof(propertyProviders)} cannot be empty.", nameof(propertyProviders));

            var rootNodes = new List<Tuple<INodePresenter, IPropertyProviderViewModel>>();
            Type type = null;
            var factory = serviceProvider.Get<GraphViewModelService>().NodePresenterFactory;
            foreach (var propertyProvider in propertyProviders)
            {
                if (!propertyProvider.CanProvidePropertiesViewModel)
                    return null;

                var rootNode = propertyProvider.GetRootNode();
                if (rootNode == null)
                    return null;

                if (type == null)
                    type = rootNode.Type;
                else if (type != rootNode.Type)
                    return null;


                var node = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode), propertyProvider);
                rootNodes.Add(Tuple.Create(node, propertyProvider));
            }
            return new GraphViewModel(serviceProvider, type, rootNodes);
        }

        public static GraphViewModel CombineViewModels(IViewModelServiceProvider serviceProvider, IReadOnlyCollection<GraphViewModel> viewModels)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));
            var combinedViewModel = new GraphViewModel(serviceProvider);

            var rootNodes = new List<NodeViewModel>();
            foreach (var viewModel in viewModels)
            {
                if (!(viewModel.RootNode is SingleNodeViewModel))
                    throw new ArgumentException(@"The view models to combine must contains SingleNodeViewModel.", nameof(viewModels));

                viewModel.Parent = combinedViewModel;
                combinedViewModel.children.Add(viewModel);
                var rootNode = (NodeViewModel)viewModel.RootNode;
                rootNodes.Add(rootNode);
            }

            if (rootNodes.Count < 2)
                throw new ArgumentException(@"Called CombineViewModels with a collection of view models that is either empty or containt just a single item.", nameof(viewModels));

            // Find best match for the root node type
            var rootNodeType = rootNodes.First().Root.Type;
            if (rootNodes.Skip(1).Any(x => x.Type != rootNodeType))
                rootNodeType = typeof(object);

            var service = serviceProvider.Get<GraphViewModelService>();
            var rootCombinedNode = service.CombinedNodeViewModelFactory(combinedViewModel, "Root", rootNodeType, rootNodes, Index.Empty);
            combinedViewModel.RootNode = rootCombinedNode;
            rootCombinedNode.Initialize();
            return combinedViewModel;
        }

        /// <summary>
        /// Gets the root node of this <see cref="GraphViewModel"/>.
        /// </summary>
        public INodeViewModel RootNode { get { return rootNode; } internal set { SetValue(ref rootNode, value); } }
        
        /// <summary>
        /// Gets or sets a function that will generate a message for the action stack when combined nodes are modified. The function will receive
        /// the modified combined node and the new value as parameters and should return a string corresponding to the message to add to the action stack.
        /// </summary>
        public Func<CombinedNodeViewModel, object, string> FormatCombinedUpdateMessage { get { return formatCombinedUpdateMessage; } set { if (value == null) throw new ArgumentException("The value cannot be null."); formatCombinedUpdateMessage = value; } }
        
        /// <summary>
        /// Gets the <see cref="GraphViewModelService"/> associated to this view model.
        /// </summary>
        public GraphViewModelService GraphViewModelService { get; }

        /// <summary>
        /// Gets the object providing properties for this view model.
        /// </summary>
        [Obsolete]
        public IPropertyProviderViewModel PropertyProvider { get; }

        /// <summary>
        /// Gets the <see cref="Logger"/> associated to this view model.
        /// </summary>
        public Logger Logger { get; private set; }

        /// <summary>
        /// Gets the parent of this view model.
        /// </summary>
        public GraphViewModel Parent { get; private set; }

        /// <summary>
        /// Gets the children of this view model (in case of combined node).
        /// </summary>
        public IReadOnlyList<GraphViewModel> Children => children;

        /// <summary>
        /// Raised when the value of an <see cref="INodeViewModel"/> contained into this view model has changed.
        /// </summary>
        /// <remarks>If this view model contains <see cref="CombinedNodeViewModel"/> instances, this event will be raised only once, at the end of the transaction.</remarks>
        public event EventHandler<GraphViewModelNodeValueChanged> NodeValueChanged;

        /// <summary>
        /// Retrieves the <see cref="IPropertyProviderViewModel"/> corresponding to the given node presenter.
        /// </summary>
        /// <param name="nodePresenter">The node presenter for which to retrieve the properties provider.</param>
        /// <returns>The properties provider of the given node presenter.</returns>
        [CanBeNull]
        public IPropertyProviderViewModel GetPropertyProvider([NotNull] INodePresenter nodePresenter)
        {
            if (nodePresenter == null) throw new ArgumentNullException(nameof(nodePresenter));
            IPropertyProviderViewModel result;
            propertiesProviderMap.TryGetValue(nodePresenter.Root, out result);
            return result;
        }

        [Pure]
        public INodeViewModel ResolveNode(string path)
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

        internal void NotifyNodeChanged(string nodePath)
        {
            Parent?.combinedNodeChanges.Add(nodePath);
            NodeValueChanged?.Invoke(this, new GraphViewModelNodeValueChanged(this, nodePath));
        }

        internal CombinedActionsContext BeginCombinedAction(string actionName, string nodePath)
        {
            return new CombinedActionsContext(this, actionName, nodePath);
        }

        internal void EndCombinedAction(string nodePath)
        {
            var handler = NodeValueChanged;
            if (handler != null)
            {
                foreach (var nodeChange in combinedNodeChanges)
                {
                    handler(this, new GraphViewModelNodeValueChanged(this, nodeChange));
                }
            }
            combinedNodeChanges.Clear();
        }

        private static CombinedNodeViewModel DefaultCreateCombinedNode(GraphViewModel ownerViewModel, string baseName, Type contentType, IEnumerable<SingleNodeViewModel> combinedNodes, Index index)
        {
            var node = new CombinedNodeViewModel(ownerViewModel, contentType, baseName, combinedNodes, index);
            return node;
        }
    }
}
