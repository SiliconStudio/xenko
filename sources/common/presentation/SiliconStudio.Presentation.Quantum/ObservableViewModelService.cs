// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// A class that provides various services to <see cref="ObservableViewModel"/> objects
    /// </summary>
    public class ObservableViewModelService
    {
        private readonly List<Action<IObservableNode, IDictionary<string, object>>> associatedDataProviders = new List<Action<IObservableNode, IDictionary<string, object>>>();

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
        }

        /// <summary>
        /// Gets or sets a method that retrieves the currently active <see cref="ObservableViewModel"/>. This method is used to get the current observable
        /// view model matching a Quantum object when using undo/redo features, since observable objects can be destroyed and recreated frequently.
        /// </summary>
        public Func<ObservableViewModelIdentifier, ObservableViewModel> ViewModelProvider { get; set; }

        /// <summary>
        /// Register a method that will associate additional data to an instance of <see cref="IObservableNode"/>.
        /// </summary>
        /// <param name="provider">The method that will associate additional data to an instance of <see cref="IObservableNode"/>.</param>
        public void RegisterAssociatedDataProvider(Action<IObservableNode, IDictionary<string, object>> provider)
        {
            associatedDataProviders.Add(provider);
        }

        /// <summary>
        /// Unregister a previoulsy registered method that was associating additional data to an instance of <see cref="IObservableNode"/>.
        /// </summary>
        /// <param name="provider">The previoulsy registered method that was associating additional additional data to an instance of <see cref="IObservableNode"/>.</param>
        public void UnregisterAssociatedDataProvider(Action<IObservableNode, IDictionary<string, object>> provider)
        {
            associatedDataProviders.Remove(provider);
        }

        internal IDictionary<string, object> RequestAssociatedData(IObservableNode node, bool updatingData)
        {
            var mergedResult = new Dictionary<string, object>();
            foreach (var provider in associatedDataProviders)
            {
                var data = new Dictionary<string, object>();
                provider(node, data);
                // We use the Add method the first time to prevent unspotted key collision.
                if (updatingData)
                    data.ForEach(x => mergedResult.Add(x.Key, x.Value));
                else
                    data.ForEach(x => mergedResult[x.Key] = x.Value);
            }
            return mergedResult;
        }
    }
}
