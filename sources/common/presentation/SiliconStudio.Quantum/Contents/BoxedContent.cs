// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Contents
{
    public class BoxedContent : ObjectContent
    {
        private IContent boxedStructureOwner;
        private Index boxedStructureOwnerIndex;

        public BoxedContent(object value, ITypeDescriptor descriptor, bool isPrimitive)
            : base(value, descriptor, isPrimitive, null)
        {
        }

        public override void Update(object newValue, Index index)
        {
            var oldValue = Retrieve(index);
            NotifyContentChanging(index, ContentChangeType.ValueChange, oldValue, Value);
            if (!index.IsEmpty)
            {
                var collectionDescriptor = Descriptor as CollectionDescriptor;
                var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
                if (collectionDescriptor != null)
                {
                    collectionDescriptor.SetValue(Value, index.Int, newValue);
                }
                else if (dictionaryDescriptor != null)
                {
                    dictionaryDescriptor.SetValue(Value, index, newValue);
                }
                else
                    throw new NotSupportedException("Unable to set the node value, the collection is unsupported");
            }
            else
            {
                SetValue(newValue);
                boxedStructureOwner?.Update(newValue, boxedStructureOwnerIndex);
            }
            NotifyContentChanged(index, ContentChangeType.ValueChange, oldValue, Value);
        }

        internal void SetOwnerContent(IContent ownerContent, Index index)
        {
            boxedStructureOwner = ownerContent;
            boxedStructureOwnerIndex = index;
        }
    }
}
