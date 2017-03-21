// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Presentation.Quantum.ViewModels;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// A class that provides various services to <see cref="GraphViewModel"/> objects
    /// </summary>
    public class GraphViewModelService
    {
        private readonly List<IPropertyNodeUpdater> propertyNodeUpdaters = new List<IPropertyNodeUpdater>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphViewModelService"/> class.
        /// </summary>
        public GraphViewModelService([NotNull] NodeContainer nodeContainer)
        {
            if (nodeContainer == null) throw new ArgumentNullException(nameof(nodeContainer));
            NodePresenterFactory = new NodePresenterFactory(nodeContainer.NodeBuilder, AvailableCommands, AvailableUpdaters);
            NodeViewModelFactory = new NodeViewModelFactory();
        }

        public INodePresenterFactory NodePresenterFactory { get; set; }

        public INodeViewModelFactory NodeViewModelFactory { get; set; }

        public List<INodePresenterCommand> AvailableCommands { get; } = new List<INodePresenterCommand>();

        // TODO: pass the collection of updaters to the factory, too
        public List<INodePresenterUpdater> AvailableUpdaters { get; } = new List<INodePresenterUpdater>();

        /// <summary>
        /// Registers a <see cref="IPropertyNodeUpdater"/> to this service.
        /// </summary>
        /// <param name="propertyNodeUpdater">The node updater to register.</param>
        [Obsolete]
        public void RegisterPropertyNodeUpdater(IPropertyNodeUpdater propertyNodeUpdater)
        {
            propertyNodeUpdaters.Add(propertyNodeUpdater);
        }

        /// <summary>
        /// Unregisters a <see cref="IPropertyNodeUpdater"/> from this service.
        /// </summary>
        /// <param name="propertyNodeUpdater">The node updater to unregister.</param>
        [Obsolete]
        public void UnregisterPropertyNodeUpdater(IPropertyNodeUpdater propertyNodeUpdater)
        {
            propertyNodeUpdaters.Remove(propertyNodeUpdater);
        }

        public void NotifyNodeInitialized(SingleNodeViewModel node)
        {
            foreach (var updater in propertyNodeUpdaters)
            {
                updater.UpdateNode(node);
            }
        }
    }
}
