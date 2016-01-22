// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public class RemoveItemCommand : ActionItemNodeCommand
    {
        public const string StaticName = "RemoveItem";

        private class RemoveItemActionItem : SimpleNodeCommandActionItem
        {
            private object indexToRemove;
            private object removedObject;

            public RemoveItemActionItem(string name, IContent content, object index, object indexToRemove, IEnumerable<IDirtiable> dirtiables)
                : base(name, content, index, dirtiables)
            {
                this.indexToRemove = indexToRemove;
            }

            public override bool Do()
            {
                var value = Content.Retrieve(Index);
                removedObject = RemoveItem(value, indexToRemove);
                Content.Update(value, Index);
                return true;
            }

            protected override void FreezeMembers()
            {
                base.FreezeMembers();
                indexToRemove = null;
                removedObject = null;
            }

            protected override void UndoAction()
            {
                var value = Content.Retrieve(Index);
                InsertItem(value, indexToRemove, removedObject);
                Content.Update(value, Index);
            }
        }

        /// <inheritdoc/>
        public override string Name => StaticName;

        /// <inheritdoc/>
        public override CombineMode CombineMode => CombineMode.AlwaysCombine;

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

        public static object RemoveItem(object container, object indexToRemove)
        {
            var descriptor = TypeDescriptorFactory.Default.Find(container.GetType());
            var collectionDescriptor = descriptor as CollectionDescriptor;
            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            object removedObject;
            if (collectionDescriptor != null)
            {
                removedObject = collectionDescriptor.GetValue(container, indexToRemove);
                collectionDescriptor.RemoveAt(container, (int)indexToRemove);
            }
            else if (dictionaryDescriptor != null)
            {
                removedObject = dictionaryDescriptor.GetValue(container, indexToRemove);
                dictionaryDescriptor.Remove(container, indexToRemove);
            }
            else
                throw new InvalidOperationException("This command cannot be executed on the given object.");

            return removedObject;
        }

        public static void InsertItem(object container, object index, object item)
        {
            var descriptor = TypeDescriptorFactory.Default.Find(container.GetType());
            var collectionDescriptor = descriptor as CollectionDescriptor;
            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            if (collectionDescriptor != null)
            {
                if (collectionDescriptor.HasInsert)
                    collectionDescriptor.Insert(container, (int)index, item);
                else
                    collectionDescriptor.Add(container, item);
            }
            else if (dictionaryDescriptor != null)
            {
                if (dictionaryDescriptor.ContainsKey(container, index))
                    throw new InvalidOperationException("Unable to insert item: the dictionary already contains the key.");

                dictionaryDescriptor.SetValue(container, index, item);
            }
        }

        protected override NodeCommandActionItem CreateActionItem(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            return new RemoveItemActionItem(Name, content, index, parameter, dirtiables);
        }
    }
}