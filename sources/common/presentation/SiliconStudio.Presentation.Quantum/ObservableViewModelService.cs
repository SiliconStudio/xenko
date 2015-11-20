// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// A class that provides various services to <see cref="ObservableViewModel"/> objects
    /// </summary>
    public class ObservableViewModelService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModelService"/> class.
        /// </summary>
        public ObservableViewModelService()
            : this(x => null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModelService"/> class.
        /// </summary>
        /// <param name="viewModelProvider">A function that returns an <see cref="ObservableViewModel"/> for an given <see cref="ObservableViewModelIdentifier"/>.</param>
        public ObservableViewModelService(Func<ObservableViewModelIdentifier, ObservableViewModel> viewModelProvider)
        {
            if (viewModelProvider == null) throw new ArgumentNullException(nameof(viewModelProvider));
            ViewModelProvider = viewModelProvider;
            ObservableNodeFactory = ObservableViewModel.DefaultObservableNodeFactory;
        }

        /// <summary>
        /// Gets or sets the observable node factory.
        /// </summary>
        public CreateNodeDelegate ObservableNodeFactory { get; set; }

        /// <summary>
        /// Gets or sets a method that retrieves the currently active <see cref="ObservableViewModel"/>. This method is used to get the current observable
        /// view model matching a Quantum object when using undo/redo features, since observable objects can be destroyed and recreated frequently.
        /// </summary>
        public Func<ObservableViewModelIdentifier, ObservableViewModel> ViewModelProvider { get; }

        /// <summary>
        /// Raised when a node is initialized, either during the construction of the <see cref="ObservableViewModel"/> or during the refresh of a
        /// node that has been modified. This event is raised once for each modified <see cref="SingleObservableNode"/> and their recursive children.
        /// </summary>
        /// <remarks>
        /// This event is intended to allow to customize nodes (by adding associated data, altering hierarchy, etc.). Subscribers should
        /// not retain any refrence to the given node since they can be destroyed and recreated arbitrarily.
        /// </remarks>
        public event EventHandler<NodeInitializedEventArgs> NodeInitialized;

        /// <summary>
        /// Attempts to resolve the given path on the observable view model corresponding to the given identifier. Returns <c>null</c>
        /// if it fails. This method does not throw exceptions.
        /// </summary>
        /// <param name="identifier">The identifier of the observable view model to resolve.</param>
        /// <param name="observableNodePath">The path of the node to resolve.</param>
        /// <returns>A reference to the <see cref="ObservableNode"/> corresponding to the given path of the given view model if available, <c>nulll</c> otherwise.</returns>
        public ObservableNode ResolveObservableNode(ObservableViewModelIdentifier identifier, string observableNodePath)
        {
            var observableViewModel = ViewModelProvider?.Invoke(identifier);
            return observableViewModel?.ResolveObservableNode(observableNodePath) as ObservableNode;
        }

        /// <summary>
        /// Raise the <see cref="NodeInitialized"/> event.
        /// </summary>
        /// <param name="node">The node that has been modified.</param>
        internal void NotifyNodeInitialized(SingleObservableNode node)
        {
            NodeInitialized?.Invoke(this, new NodeInitializedEventArgs(node));
        }
    }
}
