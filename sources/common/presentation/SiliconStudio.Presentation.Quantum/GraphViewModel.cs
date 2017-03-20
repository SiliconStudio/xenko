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
using SiliconStudio.Presentation.Quantum.ViewModels;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
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
            if (rootNodes == null) throw new ArgumentNullException(nameof(rootNode));
            var viewModelFactory = serviceProvider.Get<GraphViewModelService>().NodeViewModelFactory;
            foreach (var root in rootNodes)
            {
                propertiesProviderMap.Add(root.Item1, root.Item2);
            }
            rootNode = viewModelFactory.CreateGraph(this, type, propertiesProviderMap.Keys);
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

        /// <summary>
        /// Gets the root node of this <see cref="GraphViewModel"/>.
        /// </summary>
        public INodeViewModel RootNode { get { return rootNode; } set { SetValue(ref rootNode, value); } }
                
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
    }
}
