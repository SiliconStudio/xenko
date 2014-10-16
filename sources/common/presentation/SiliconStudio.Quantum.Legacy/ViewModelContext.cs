// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.Quantum.Legacy
{
    /// <summary>
    /// Handles a group of <see cref="IViewModelNode"/> with its associated information.
    /// </summary>
    public class ViewModelContext
    {
        private readonly Dictionary<Guid, IViewModelNode> viewModelByGuid = new Dictionary<Guid, IViewModelNode>();
        private readonly HashSet<IViewModelNode> visibleViewModels = new HashSet<IViewModelNode>();
        private readonly Dictionary<Guid[], IViewModelNode> combinedViewModelByGuid = new Dictionary<Guid[], IViewModelNode>(new ListEqualityComparer<Guid>());
        private readonly ViewModelGuidContainer guidContainer;
        private PropertyContainer tags;

        // Async nodes to be uploaded, with their root Guid
        internal Queue<KeyValuePair<Guid, IViewModelNode>> PendingAsyncNodes = new Queue<KeyValuePair<Guid, IViewModelNode>>();

        internal bool ContextLocked;

        /// <summary>
        /// Gets the view model by GUID.
        /// </summary>
        public IDictionary<Guid, IViewModelNode> ViewModelByGuid
        {
            get { return viewModelByGuid; }
        }

        // TODO: Currently unused, but it should be to avoid transferring unused nodes across network.
        public HashSet<IViewModelNode> VisibleViewModels
        {
            get { return visibleViewModels; }
        }

        /// <summary>
        /// Gets or sets the current list of active GUIDs.
        /// As some items are removed through serialization, they are not directly removed from ViewModelByGuid (for buffering purposes).
        /// During UI synchronization this list can be checked to know what should be removed.
        /// </summary>
        public HashSet<Guid> CurrentGuids { get; set; }

        /// <summary>
        /// Gets enumerator, which will be used to generate <see cref="IViewModelNode"/> children.
        /// </summary>
        public List<IChildrenPropertyEnumerator> ChildrenPropertyEnumerators { get; private set; }

        /// <summary>
        /// Gets or sets the root node.
        /// </summary>
        public IViewModelNode Root
        {
            get
            {
                return root;
            }
            set
            {
                if (value != null && (!ViewModelByGuid.ContainsKey(value.Guid) || ViewModelByGuid[value.Guid] != value))
                    throw new InvalidOperationException("Try to set the root of ViewModelContext to a ViewModelNode that is not registered in the context. Use RegisterViewModel or GetOrCreateModelView.");

                root = value;
            }
        }
        private IViewModelNode root;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelContext"/> class.
        /// </summary>
        /// <param name="guidContainer">The global context to use.</param>
        public ViewModelContext(ViewModelGuidContainer guidContainer = null)
        {
            this.guidContainer = guidContainer ?? new ViewModelGuidContainer();
            ChildrenPropertyEnumerators = new List<IChildrenPropertyEnumerator>();
            tags = new PropertyContainer(this);
        }


        public T Get<T>(PropertyKey<T> key)
        {
            return tags.Get(key);
        }

        public void Set<T>(PropertyKey<T> key, T value)
        {
            tags.SetObject(key, value);
        }

        public IViewModelNode GetNextPendingAsyncNode()
        {
            while (PendingAsyncNodes.Count > 0)
            {
                var nextItem = PendingAsyncNodes.Dequeue();

                // Check if item is still alive
                if (viewModelByGuid.ContainsKey(nextItem.Key))
                    return nextItem.Value;
            }
            return null;
        }

        /// <summary>
        /// Gets the <see cref="IViewModelNode"/> associated to the given <see cref="Guid"/> that exists in the current <see cref="ViewModelContext"/>.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/>.</param>
        /// <returns>The <see cref="IViewModelNode"/> associated to the given <see cref="Guid"/> if available, <c>null</c> otherwise.</returns>
        public IViewModelNode GetViewModelNode(Guid guid)
        {
            IViewModelNode viewModelNode;
            viewModelByGuid.TryGetValue(guid, out viewModelNode);
            return viewModelNode;
        }

        public void RegisterViewModel(IViewModelNode viewModelNode)
        {
            if (ContextLocked)
                throw new InvalidOperationException("ViewModelByGuid should not be modified during enumeration.");

            viewModelByGuid[viewModelNode.Guid] = viewModelNode;
            if (viewModelNode.Content.Value != null)
                guidContainer.RegisterGuid(viewModelNode.Guid, viewModelNode.Content.Value);
        }

        //public IViewModelNode GetOrCreateCombinedViewModel(string name, object[] models, IChildrenPropertyEnumerator[] additionalEnumerators = null)
        //{
        //    Guid[] guids = models.Select(GetOrCreateGuid).ToArray();
        //    IViewModelNode viewModelNode;
        //    if (!combinedViewModelByGuid.TryGetValue(guids, out viewModelNode))
        //    {
        //        IEnumerable<IViewModelNode> modelViews = models.Select(x => GetOrCreateModelView(name, x, additionalEnumerators));
        //        viewModelNode = ViewModelController.Combine(modelViews.ToArray());
        //        combinedViewModelByGuid.Add(guids, viewModelNode);
        //        RegisterViewModel(viewModelNode);
        //    }
        //    return viewModelNode;
        //}

        public IViewModelNode GetOrCreateCombinedViewModel(IViewModelNode[] viewModels, IChildrenPropertyEnumerator[] additionalEnumerators = null)
        {
            Guid[] guids = viewModels.Select(x => x.Guid).ToArray();
            IViewModelNode viewModelNode;
            if (!combinedViewModelByGuid.TryGetValue(guids, out viewModelNode))
            {
                viewModelNode = ViewModelController.Combine(viewModels);
                combinedViewModelByGuid.Add(guids, viewModelNode);
                RegisterViewModel(viewModelNode);
            }
            return viewModelNode;
        }

        public int ClearCombinedModelViews(IViewModelNode[] combinedViewModelToKeep = null)
        {
            int count;
            if (combinedViewModelToKeep == null || combinedViewModelToKeep.Length == 0)
            {
                count = combinedViewModelByGuid.Count;
                combinedViewModelByGuid.Clear();
            }
            else
            {
                count = combinedViewModelByGuid.RemoveWhere(x => !combinedViewModelToKeep.Contains(x.Value));
            }
            return count;
        }

        public IViewModelNode GetOrCreateModelView(object model, string name = null, IChildrenPropertyEnumerator[] additionalEnumerators = null)
        {
            if (model == null) throw new ArgumentNullException("model");

            var guid = GetOrCreateGuid(model);
            IViewModelNode viewModelNode;
            if (!viewModelByGuid.TryGetValue(guid, out viewModelNode))
            {
                viewModelNode = new ViewModelNode(name ?? model.ToString(), new ObjectContent(model, model.GetType(), null)) { Guid = guid };
                viewModelNode.GenerateChildren(this, additionalEnumerators);
                RegisterViewModel(viewModelNode);
            }
            return viewModelNode;
        }

        public Guid GetOrCreateGuid(object model)
        {
            return guidContainer.GetOrCreateGuid(model);
        }

        public void GenerateChildren(IViewModelNode viewModelNode, IChildrenPropertyEnumerator[] additionalEnumerators = null)
        {
            var childrenPropertyEnumerators = additionalEnumerators != null
                ? additionalEnumerators.Concat(ChildrenPropertyEnumerators)
                : ChildrenPropertyEnumerators;

            foreach (var childrenPropertyEnumerator in childrenPropertyEnumerators)
            {
                bool handled = false;
                childrenPropertyEnumerator.GenerateChildren(this, viewModelNode, ref handled);
                if (handled)
                    return;
            }
        }
    }
}