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
    public class RemoveItemCommand : SyncNodeCommand
    {
        public const string StaticName = "RemoveItem";

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

        protected override IActionItem ExecuteSync(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            content.Remove(index);
            return null;
        }
    }
}