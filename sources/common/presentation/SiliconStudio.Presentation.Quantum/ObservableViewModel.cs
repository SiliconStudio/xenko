// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public class ObservableViewModel : EditableViewModel
    {
        public const string DefaultLoggerName = "Quantum";
        public const string HasChildPrefix = "HasChild_";
        public const string HasCommandPrefix = "HasCommand_";
        public const string HasAssociatedDataPrefix = "HasAssociatedData_";
        
        private readonly ObservableViewModelService observableViewModelService;
        private readonly ModelContainer modelContainer;
        private IObservableNode rootNode;
        private bool singleNodeActionRegistered;

        private Func<SingleObservableNode, object, string> formatSingleUpdateMessage = (node, value) => string.Format("Update '{0}'", node.Name);
        private Func<CombinedObservableNode, object, string> formatCombinedUpdateMessage = (node, value) => string.Format("Update '{0}'", node.Name);

        private readonly Dictionary<ObservableModelNode, List<IDirtiableViewModel>> dirtiableViewModels = new Dictionary<ObservableModelNode, List<IDirtiableViewModel>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="ObservableViewModelService"/> to use for this view model.</param>
        /// <param name="modelContainer">A <see cref="ModelContainer"/> to use to build view model nodes.</param>
        private ObservableViewModel(IViewModelServiceProvider serviceProvider, ModelContainer modelContainer)
            : base(serviceProvider)
        {
            if (modelContainer == null) throw new ArgumentNullException("modelContainer");
            this.modelContainer = modelContainer;
            observableViewModelService = serviceProvider.Get<ObservableViewModelService>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="ObservableViewModelService"/> to use for this view model.</param>
        /// <param name="modelContainer">A <see cref="ModelContainer"/> to use to build view model nodes.</param>
        /// <param name="modelNode">The root model node of the view model to generate.</param>
        /// <param name="dirtiables">The list of <see cref="IDirtiableViewModel"/> objects linked to this view model.</param>
        public ObservableViewModel(IViewModelServiceProvider serviceProvider, ModelContainer modelContainer, IModelNode modelNode, IEnumerable<IDirtiableViewModel> dirtiables)
            : this(serviceProvider, modelContainer)
        {
            if (modelNode == null) throw new ArgumentNullException("modelNode");
            var node = ObservableModelNode.Create(this, "Root", modelNode.Content.IsPrimitive, null, modelNode, new ModelNodePath(modelNode), modelNode.Content.Type, null);
            Identifier = new ObservableViewModelIdentifier(node.ModelGuid);
            dirtiableViewModels.Add(node, dirtiables.ToList());
            node.Initialize();
            RootNode = node;
            node.CheckConsistency();
        }

        public static ObservableViewModel CombineViewModels(IViewModelServiceProvider serviceProvider, ModelContainer modelContainer, IEnumerable<ObservableViewModel> viewModels)
        {
            var combinedViewModel = new ObservableViewModel(serviceProvider, modelContainer);

            var rootNodes = new List<ObservableModelNode>();
            foreach (var viewModel in viewModels)
            {
                if (!(viewModel.RootNode is SingleObservableNode))
                    throw new ArgumentException(@"The view models to combine must contains SingleObservableNode.", "viewModels");

                foreach (var dirtiableViewModel in viewModel.dirtiableViewModels)
                    combinedViewModel.dirtiableViewModels.Add(dirtiableViewModel.Key, dirtiableViewModel.Value.ToList());

                var rootNode = (ObservableModelNode)viewModel.RootNode;
                rootNodes.Add(rootNode);
            }

            if (rootNodes.Count < 2)
                throw new ArgumentException(@"Called CombineViewModels with a collection of view models that is either empty or containt just a single item.", "viewModels");

            CombinedObservableNode rootCombinedNode = CombinedObservableNode.Create(combinedViewModel, "Root", null, typeof(object), rootNodes, null);
            combinedViewModel.Identifier = new ObservableViewModelIdentifier(rootNodes.Select(x => x.ModelGuid));
            rootCombinedNode.Initialize();
            combinedViewModel.RootNode = rootCombinedNode;
            return combinedViewModel;
        }

        internal IReadOnlyCollection<IDirtiableViewModel> GetDirtiableViewModels(ObservableModelNode node)
        {
            return dirtiableViewModels[(ObservableModelNode)node.Root];
        }

        /// <inheritdoc/>
        public override IEnumerable<IDirtiableViewModel> Dirtiables { get { return Enumerable.Empty<IDirtiableViewModel>(); } }

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
        public ObservableViewModelService ObservableViewModelService { get { return observableViewModelService; } }

        /// <summary>
        /// Gets an identifier for this view model.
        /// </summary>
        public ObservableViewModelIdentifier Identifier { get; private set; }

        /// <summary>
        /// Gets the <see cref="ModelContainer"/> used to store Quantum objects.
        /// </summary>
        internal ModelContainer ModelContainer { get { return modelContainer; } }

        public event EventHandler<NodeChangedArgs> NodeChanged;

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

        [Pure]
        public ObservableModelNode ResolveObservableModelNode(string path, IModelNode rootModelNode)
        {
            var members = path.Split('.');
            if (members[0] != RootNode.Name)
                return null;

            var currentNode = RootNode;
            var combinedNode = currentNode as CombinedObservableNode;
            if (combinedNode != null)
            {
                currentNode = combinedNode.CombinedNodes.OfType<ObservableModelNode>().Single(x => x.MatchNode(rootModelNode));
            }
            foreach (var member in members.Skip(1))
            {
                currentNode = currentNode.Children.FirstOrDefault(x => x.Name == member);
                if (currentNode == null)
                    return null;
            }
            return (ObservableModelNode)currentNode;
        }

        private bool MatchModelRootNode(IModelNode node)
        {
            return RootNode is ObservableModelNode && ((ObservableModelNode)RootNode).MatchNode(node);
        }

        internal bool MatchCombinedRootNode(IModelNode node)
        {
            return RootNode is CombinedObservableNode && ((CombinedObservableNode)RootNode).CombinedNodes.OfType<ObservableModelNode>().Any(x => x.MatchNode(node));
        }

        internal bool MatchRootNode(IModelNode node)
        {
            return MatchModelRootNode(node) || MatchCombinedRootNode(node);
        }

        internal void NotifyNodeChanged(string observableNodePath)
        {
            var handler = NodeChanged;
            if (handler != null)
            {
                handler(this, new NodeChangedArgs(this, observableNodePath));
            }
        }

        internal void RegisterAction(string displayName, ModelNodePath nodePath, string observableNodePath, object index, IReadOnlyCollection<IDirtiableViewModel> dirtiables, object newValue, object previousValue)
        {
            singleNodeActionRegistered = true;
            var actionItem = new ValueChangedActionItem(displayName, observableViewModelService, nodePath, observableNodePath, Identifier, index, dirtiables, modelContainer, previousValue);
            ActionStack.Add(actionItem);
            NotifyNodeChanged(observableNodePath);
        }

        internal void BeginCombinedAction()
        {
            ActionStack.BeginTransaction();
            singleNodeActionRegistered = false;
        }

        internal void EndCombinedAction(string displayName, string observableNodePath, object value)
        {
            bool shouldDiscard = true;
            foreach (var singleNode in dirtiableViewModels.Keys)
            {
                if (singleNode.Owner.singleNodeActionRegistered)
                    shouldDiscard = false;

                singleNode.Owner.singleNodeActionRegistered = false;
            }

            if (shouldDiscard)
            {
                ActionStack.DiscardTransaction();
            }
            else
            {
                ActionStack.EndTransaction(displayName, x => new CombinedValueChangedActionItem(displayName, observableViewModelService, observableNodePath, Identifier, x));
            }
        }
    }
}
