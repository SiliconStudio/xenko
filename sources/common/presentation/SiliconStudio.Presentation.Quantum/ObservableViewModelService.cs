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
        private readonly List<IPropertyNodeUpdater> propertyNodeUpdaters = new List<IPropertyNodeUpdater>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModelService"/> class.
        /// </summary>
        public ObservableViewModelService()
        {
            ObservableNodeFactory = ObservableViewModel.DefaultObservableNodeFactory;
        }

        /// <summary>
        /// Gets or sets the observable node factory.
        /// </summary>
        public CreateNodeDelegate ObservableNodeFactory { get; set; }

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
        /// Registers a <see cref="IPropertyNodeUpdater"/> to this service.
        /// </summary>
        /// <param name="propertyNodeUpdater">The node updater to register.</param>
        public void RegisterPropertyNodeUpdater(IPropertyNodeUpdater propertyNodeUpdater)
        {
            propertyNodeUpdaters.Add(propertyNodeUpdater);
        }

        /// <summary>
        /// Unregisters a <see cref="IPropertyNodeUpdater"/> from this service.
        /// </summary>
        /// <param name="propertyNodeUpdater">The node updater to unregister.</param>
        public void UnregisterPropertyNodeUpdater(IPropertyNodeUpdater propertyNodeUpdater)
        {
            propertyNodeUpdaters.Remove(propertyNodeUpdater);
        }

        /// <summary>
        /// Raise the <see cref="NodeInitialized"/> event.
        /// </summary>
        /// <param name="node">The node that has been modified.</param>
        internal void NotifyNodeInitialized(SingleObservableNode node)
        {
            foreach (var updater in propertyNodeUpdaters)
            {
                updater.UpdateNode(node);
            }
            NodeInitialized?.Invoke(this, new NodeInitializedEventArgs(node));
        }
    }
}
