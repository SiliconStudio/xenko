// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// A class that provides various services to <see cref="ObservableViewModel"/> objects
    /// </summary>
    public class ObservableViewModelService
    {
        private readonly List<Action<ObservableNode>> associatedDataProviders = new List<Action<ObservableNode>>();

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
            if (viewModelProvider == null) throw new ArgumentNullException("viewModelProvider");
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
        public Func<ObservableViewModelIdentifier, ObservableViewModel> ViewModelProvider { get; private set; }

        /// <summary>
        /// Register a method that will associate additional data to an instance of <see cref="IObservableNode"/>.
        /// </summary>
        /// <param name="provider">The method that will associate additional data to an instance of <see cref="IObservableNode"/>.</param>
        public void RegisterAssociatedDataProvider(Action<ObservableNode> provider)
        {
            associatedDataProviders.Add(provider);
        }

        /// <summary>
        /// Unregister a previoulsy registered method that was associating additional data to an instance of <see cref="IObservableNode"/>.
        /// </summary>
        /// <param name="provider">The previoulsy registered method that was associating additional additional data to an instance of <see cref="IObservableNode"/>.</param>
        public void UnregisterAssociatedDataProvider(Action<ObservableNode> provider)
        {
            associatedDataProviders.Remove(provider);
        }

        internal void RequestAssociatedData(ObservableNode node, bool updatingData)
        {
            foreach (var provider in associatedDataProviders)
            {
                provider(node);
            }
        }
    }
}
