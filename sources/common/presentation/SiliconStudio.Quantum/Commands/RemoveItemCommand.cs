// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public class RemoveItemCommand : SyncNodeCommandBase
    {
        public const string CommandName = "RemoveItem";

        /// <inheritdoc/>
        public override string Name => CommandName;

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

        protected override void ExecuteSync(IContent content, Index index, object parameter)
        {
            var item = content.Retrieve(index);
            content.Remove(item, index);
        }
    }
}
