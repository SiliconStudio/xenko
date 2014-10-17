// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;

using SiliconStudio.Presentation.Quantum.ComponentModel;

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
            : this(x => new DefaultObservableNodePropertyProvider(x), x => null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModelService"/> class.
        /// </summary>
        /// <param name="nodePropertyProviderFactory">A function that returns an <see cref="IObservableNodePropertyProvider"/> for a given <see cref="IObservableNode"/>.</param>
        public ObservableViewModelService(Func<IObservableNode, IObservableNodePropertyProvider> nodePropertyProviderFactory)
            : this(nodePropertyProviderFactory, x => null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModelService"/> class.
        /// </summary>
        /// <param name="viewModelProvider">A function that returns an <see cref="ObservableViewModel"/> for an given <see cref="ObservableViewModelIdentifier"/>.</param>
        public ObservableViewModelService(Func<ObservableViewModelIdentifier, ObservableViewModel> viewModelProvider)
            : this(x => new DefaultObservableNodePropertyProvider(x), viewModelProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModelService"/> class.
        /// </summary>
        /// <param name="nodePropertyProviderFactory">A function that returns an <see cref="IObservableNodePropertyProvider"/> for a given <see cref="IObservableNode"/>.</param>
        /// <param name="viewModelProvider">A function that returns an <see cref="ObservableViewModel"/> for an given <see cref="ObservableViewModelIdentifier"/>.</param>
        public ObservableViewModelService(Func<IObservableNode, IObservableNodePropertyProvider> nodePropertyProviderFactory, Func<ObservableViewModelIdentifier, ObservableViewModel> viewModelProvider)
        {
            if (nodePropertyProviderFactory == null) throw new ArgumentNullException("nodePropertyProviderFactory");
            if (viewModelProvider == null) throw new ArgumentNullException("viewModelProvider");
            NodePropertyProviderFactory = nodePropertyProviderFactory;
            ViewModelProvider = viewModelProvider;
        }

        /// <summary>
        /// Gets or sets a method that retrieves the currently active <see cref="ObservableViewModel"/>. This method is used to get the current observable
        /// view model matching a Quantum object when using undo/redo features, since observable objects can be destroyed and recreated frequently.
        /// </summary>
        public Func<ObservableViewModelIdentifier, ObservableViewModel> ViewModelProvider { get; set; }

        /// <summary>
        /// Gets or sets the factory that provides instances of <see cref="IObservableNodePropertyProvider"/> for each <see cref="IObservableNode"/> of the
        /// view models. The node property provider is used by each <see cref="ObservableNode"/> to provide a collection of member properties when the
        /// view model is used as a <see cref="ICustomTypeDescriptor"/>.
        /// </summary>
        /// <remarks>By default, this factory provides a new instance of <see cref="DefaultObservableNodePropertyProvider"/> for each node.</remarks>
        public Func<IObservableNode, IObservableNodePropertyProvider> NodePropertyProviderFactory { get; set; }
    }
}
