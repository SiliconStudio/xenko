// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Presentation.ViewModel.ActionStack;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public class ValueChangedActionItem : ViewModelActionItem
    {
        private ObservableViewModelService service;
        private ModelContainer modelContainer;
        private ModelNodePath nodePath;
        private string observableNodePath;
        private readonly ObservableViewModelIdentifier identifier;

        private object index;
        private object previousValue;

        public ValueChangedActionItem(string name, ObservableViewModelService service, ModelNodePath nodePath, string observableNodePath, ObservableViewModelIdentifier identifier, object index, IEnumerable<IDirtiableViewModel> dirtiables, ModelContainer modelContainer, object previousValue)
            : base(name, dirtiables)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (!nodePath.IsValid) throw new InvalidOperationException("Unable to retrieve the path of the modified node.");
            this.service = service;
            this.modelContainer = modelContainer;
            this.nodePath = nodePath;
            this.index = index;
            this.observableNodePath = observableNodePath;
            this.identifier = identifier;
            this.previousValue = previousValue;
        }

        public string ObservableNodePath { get { return observableNodePath; } }

        protected override void FreezeMembers()
        {
            service = null;
            modelContainer = null;
            nodePath = null;
            observableNodePath = null;
            index = null;
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            var node = nodePath.GetNode();
            if (node == null)
                throw new InvalidOperationException("Unable to retrieve the node to modify in this undo process.");

            var currentValue = GetValue(node, index);
            bool setByObservableNode = false;

            var observableViewModel = service.ViewModelProvider != null ? service.ViewModelProvider(identifier) : null;
            if (observableViewModel != null && !observableViewModel.MatchRootNode(nodePath.RootNode))
                observableViewModel = null;

            if (observableViewModel != null)
            {
                SingleObservableNode observableNode = observableViewModel.ResolveObservableModelNode(observableNodePath, nodePath.RootNode);
                if (observableNode != null)
                {
                    observableNode.Value = previousValue;
                    setByObservableNode = true;
                }
            }

            if (!setByObservableNode)
            {
                SetValue(node, index, previousValue);
            }
            
            previousValue = currentValue;        
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            // Once we have un-done, the previous value is updated so Redo is just Undoing the Undo
            UndoAction();
        }

        private static void SetValue(IModelNode node, object index, object value)
        {
            // Index should be used only for items in collection of primitive type, where the whole list is fully reresented by a single node. So we test that we're not in another root object
            if (index != null)
            {
                var collectionDescriptor = node.Content.Descriptor as CollectionDescriptor;
                var dictionaryDescriptor = node.Content.Descriptor as DictionaryDescriptor;
                if (collectionDescriptor != null)
                {
                    collectionDescriptor.SetValue(node.Content.Value, (int)index, value);
                }
                else if (dictionaryDescriptor != null)
                {
                    dictionaryDescriptor.SetValue(node.Content.Value, index, value);
                }
                else
                    throw new NotSupportedException("Unable to undo the change, the collection is unsupported");
            }
            else
            {
                node.Content.Value = value;
            }
        }

        private static object GetValue(IModelNode node, object index)
        {
            if (index != null)
            {
                var collectionDescriptor = node.Content.Descriptor as CollectionDescriptor;
                var dictionaryDescriptor = node.Content.Descriptor as DictionaryDescriptor;
                if (collectionDescriptor != null)
                {
                    return collectionDescriptor.GetValue(node.Content.Value, (int)index);
                }
                if (dictionaryDescriptor != null)
                {
                    return dictionaryDescriptor.GetValue(node.Content.Value, index);
                }

                throw new NotSupportedException("Unable to undo the change, the collection is unsupported");
            }

            return node.Content.Value;
        }
    }
}
