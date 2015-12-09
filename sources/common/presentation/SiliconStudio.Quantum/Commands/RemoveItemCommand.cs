// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Commands
{
    public class RemoveItemCommand : SimpleNodeCommand
    {
        private struct UndoTokenData
        {
            private readonly object indexer;
            private readonly object item;
            public UndoTokenData(object indexer, object item)
            {
                this.indexer = indexer;
                this.item = item;
            }

            public object Indexer { get { return indexer; } }

            public object Item { get { return item; } }
        }

        /// <inheritdoc/>
        public override string Name { get { return "RemoveItem"; } }

        /// <inheritdoc/>
        public override CombineMode CombineMode { get { return CombineMode.AlwaysCombine; } }

        /// <inheritdoc/>
        public override bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor)
        {
            if (memberDescriptor != null)
            {
                var attrib = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<MemberCollectionAttribute>(memberDescriptor.MemberInfo);
                if (attrib != null && attrib.ReadOnly)
                    return false;
            }
            
            var collectionDescriptor = typeDescriptor as CollectionDescriptor;
            var dictionaryDescriptor = typeDescriptor as DictionaryDescriptor;
            if (collectionDescriptor != null)
            {
                var elementType = collectionDescriptor.ElementType;
                // We also add the same conditions that for AddNewItem
                return collectionDescriptor.HasRemoveAt && (!elementType.IsClass || elementType.GetConstructor(Type.EmptyTypes) != null || elementType.IsAbstract || elementType.IsNullable() || elementType == typeof(string));
            }
            // TODO: add a HasRemove in the dictionary descriptor and test it!
            return dictionaryDescriptor != null;
        }

        /// <inheritdoc/>
        protected override object Do(object currentValue, object parameter, out UndoToken undoToken)
        {
            var descriptor = TypeDescriptorFactory.Default.Find(currentValue.GetType());
            var collectionDescriptor = descriptor as CollectionDescriptor;
            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            if (collectionDescriptor != null)
            {
                var position = (int)parameter;
                var removedObject = collectionDescriptor.GetValue(currentValue, position);
                undoToken = new UndoToken(true, new UndoTokenData(parameter, removedObject));
                collectionDescriptor.RemoveAt(currentValue, position);
            }
            else if (dictionaryDescriptor != null)
            {
                var removedObject = dictionaryDescriptor.GetValue(currentValue, parameter);
                undoToken = new UndoToken(true, new UndoTokenData(parameter, removedObject));
                dictionaryDescriptor.Remove(currentValue, parameter);
            }
            else
                throw new InvalidOperationException("This command cannot be executed on the given object.");

            return currentValue;
        }

        /// <inheritdoc/>
        protected override object Undo(object currentValue, UndoToken undoToken)
        {
            var descriptor = TypeDescriptorFactory.Default.Find(currentValue.GetType());
            var collectionDescriptor = descriptor as CollectionDescriptor;
            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            var undoData = (UndoTokenData)undoToken.TokenValue;
            if (collectionDescriptor != null)
            {
                var position = (int)undoData.Indexer;
                if (collectionDescriptor.HasInsert)
                    collectionDescriptor.Insert(currentValue, position, undoData.Item);
                else
                    collectionDescriptor.Add(currentValue, undoData.Item);
            }
            else if (dictionaryDescriptor != null)
            {
                if (dictionaryDescriptor.ContainsKey(currentValue, undoData.Indexer))
                    throw new InvalidOperationException("Unable to undo remove: the dictionary contains the key to re-add.");
                dictionaryDescriptor.SetValue(currentValue, undoData.Indexer, undoData.Item);
            }
            return currentValue;
        }
    }
}