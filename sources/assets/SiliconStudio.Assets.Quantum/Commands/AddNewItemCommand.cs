// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.Contents;

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
            return collectionDescriptor.HasAdd && (CanConstruct(elementType) || CanAddNull(elementType) || IsReferenceType(elementType));
        }

        protected override void ExecuteSync(IContent content, Index index, object parameter)
        {
            var value = content.Retrieve(index);
            var collectionDescriptor = (CollectionDescriptor)TypeDescriptorFactory.Default.Find(value.GetType());

            object itemToAdd = null;
            var elementType = collectionDescriptor.ElementType;
            if (CanAddNull(elementType) || IsReferenceType(elementType))
            {
                // If the parameter is a type instead of an instance, try to construct an instance of this type
                var type = parameter as Type;
                if (type?.GetConstructor(Type.EmptyTypes) != null)
                    itemToAdd = ObjectFactoryRegistry.NewInstance(type);
            }
            else if (collectionDescriptor.ElementType == typeof(string))
            {
                itemToAdd = parameter ?? "";
            }
            else
            {
                itemToAdd = parameter ?? ObjectFactoryRegistry.NewInstance(collectionDescriptor.ElementType);
            }
            if (index.IsEmpty)
            {
                content.Add(itemToAdd);
            }
            else
            {
                // Handle collections in collections
                // TODO: this is not working on the observable node side
                var collectionNode = content.Reference.AsEnumerable[index].TargetNode;
                collectionNode.Content.Add(itemToAdd);
            }
        }

        private static bool CanConstruct(Type elementType) => !elementType.IsClass || elementType.GetConstructor(Type.EmptyTypes) != null || elementType == typeof(string);

        private static bool CanAddNull(Type elementType) => elementType.IsAbstract || elementType.IsNullable();

        private static bool IsReferenceType(Type elementType) => AssetRegistry.IsContentType(elementType) || typeof(AssetReference).IsAssignableFrom(elementType);
    }
}
