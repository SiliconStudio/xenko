// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Attributes;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// This command construct a new item and add it to the list contained in the value of the node. In order to be used,
    /// the node owning this command must contains a non-null value of type IList{T}. An new item of type T will be created,
    /// or an exception will be thrown if T could not be determinated or has no parameterless constructor.
    /// </summary>
    /// <remarks>No parameter is required when invoking this command.</remarks>
    public class AddNewItemCommand : INodeCommand
    {
        /// <inheritdoc/>
        public string Name { get { return "AddNewItem"; } }

        /// <inheritdoc/>
        public CombineMode CombineMode { get { return CombineMode.CombineOnlyForAll; } }

        /// <inheritdoc/>
        public bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor)
        {
            if (memberDescriptor != null)
            {
                var attrib = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<SealedCollectionAttribute>(memberDescriptor.MemberInfo);
                if (attrib != null && attrib.CollectionSealed)
                    return false;
            }
            
            var collectionDescriptor = typeDescriptor as CollectionDescriptor;
            if (collectionDescriptor == null)
                return false;

            var elementType = collectionDescriptor.ElementType;
            return collectionDescriptor.HasAdd && (!elementType.IsClass || elementType.GetConstructor(Type.EmptyTypes) != null || elementType.IsAbstract || elementType.IsNullable() || elementType == typeof(string));
        }

        /// <inheritdoc/>
        public object Invoke(object currentValue, ITypeDescriptor descriptor, object parameter, out UndoToken undoToken)
        {
            var collectionDescriptor = (CollectionDescriptor)descriptor;
            if (collectionDescriptor.ElementType.IsAbstract || collectionDescriptor.ElementType.IsNullable())
            {
                undoToken = new UndoToken(true, collectionDescriptor.GetCollectionCount(currentValue));
                collectionDescriptor.Add(currentValue, parameter);
            }
            else if (collectionDescriptor.ElementType == typeof(string))
            {
                undoToken = new UndoToken(true, collectionDescriptor.GetCollectionCount(currentValue));
                collectionDescriptor.Add(currentValue, parameter ?? "");
            }
            else
            {
                var newItem = Activator.CreateInstance(collectionDescriptor.ElementType);
                undoToken = new UndoToken(true, collectionDescriptor.GetCollectionCount(currentValue));
                collectionDescriptor.Add(currentValue, parameter ?? newItem);
            }
            return currentValue;
        }

        /// <inheritdoc/>
        public object Undo(object currentValue, ITypeDescriptor descriptor, UndoToken undoToken)
        {
            var index = (int)undoToken.TokenValue;
            var collectionDescriptor = (CollectionDescriptor)descriptor;
            collectionDescriptor.RemoveAt(currentValue, index);
            return currentValue;
        }
    }
}