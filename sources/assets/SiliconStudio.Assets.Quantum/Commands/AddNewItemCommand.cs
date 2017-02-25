// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Assets.Quantum.Commands
{
    /// <summary>
    /// This command construct a new item and add it to the list contained in the value of the node. In order to be used,
    /// the node owning this command must contains a non-null value of type IList{T}. An new item of type T will be created,
    /// or an exception will be thrown if T could not be determinated or has no parameterless constructor.
    /// </summary>
    /// <remarks>No parameter is required when invoking this command.</remarks>
    public class AddNewItemCommand : SyncNodeCommandBase
    {
        public const string CommandName = "AddNewItem";

        /// <inheritdoc/>
        public override string Name => CommandName;

        /// <inheritdoc/>
        public override CombineMode CombineMode => CombineMode.DoNotCombine;

        /// <inheritdoc/>
        public override bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor)
        {
            if (memberDescriptor != null)
            {
                var attrib = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<MemberCollectionAttribute>(memberDescriptor.MemberInfo);
                if (attrib?.ReadOnly == true)
                    return false;
            }
            
            var collectionDescriptor = typeDescriptor as CollectionDescriptor;
            if (collectionDescriptor == null)
                return false;

            var elementType = collectionDescriptor.ElementType;
            return collectionDescriptor.HasAdd && (CanConstruct(elementType) || elementType.IsAbstract || elementType.IsNullable() || IsReferenceType(elementType));
        }

        protected override void ExecuteSync(IGraphNode node, Index index, object parameter)
        {
            var value = node.Retrieve(index);
            var collectionDescriptor = (CollectionDescriptor)TypeDescriptorFactory.Default.Find(value.GetType());

            object itemToAdd;

            // First, check if parameter is an AbstractNodeEntry
            var abstractNodeEntry = parameter as AbstractNodeEntry;
            if (abstractNodeEntry != null)
            {
                itemToAdd = abstractNodeEntry.GenerateValue(null);
            }
            // Otherwise, assume it's an object
            else
            {
                var elementType = collectionDescriptor.ElementType;
                itemToAdd = parameter;
                if (itemToAdd == null)
                {
                    var propertyGraph = (node as IAssetNode)?.PropertyGraph;
                    var instance = ObjectFactoryRegistry.NewInstance(elementType);
                    if (!IsReferenceType(elementType) && (propertyGraph == null || !propertyGraph.IsObjectReference(node, index, instance)))
                        itemToAdd = instance;
                }
            }

            var objectNode = node as IObjectNode ?? ((IMemberNode)node).Target;
            if (index.IsEmpty)
            {
                objectNode.Add(itemToAdd);
            }
            else
            {
                // Handle collections in collections
                // TODO: this is not working on the observable node side
                var collectionNode = objectNode.ItemReferences[index].TargetNode;
                collectionNode.Add(itemToAdd);
            }
        }

        private static bool CanConstruct(Type elementType) => !elementType.IsClass || elementType.GetConstructor(Type.EmptyTypes) != null || elementType == typeof(string);

        private static bool IsReferenceType(Type elementType) => AssetRegistry.IsContentType(elementType) || typeof(AssetReference).IsAssignableFrom(elementType);
    }
}
