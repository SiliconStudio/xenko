// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Contents
{
    public class BoxedContent : ObjectContent
    {
        private ContentNode boxedStructureOwner;
        private Index boxedStructureOwnerIndex;

        public BoxedContent(object value, Guid guid, ITypeDescriptor descriptor, bool isPrimitive)
            : base(value, guid, descriptor, isPrimitive, null)
        {
        }

        protected internal override void UpdateFromMember(object newValue, Index index)
        {
            Update(newValue, index, true);
        }

        internal void UpdateFromOwner(object newValue)
        {
            Update(newValue, Index.Empty, false);
        }

        internal void SetOwnerContent(IContentNode ownerContent, Index index)
        {
            boxedStructureOwner = (ContentNode)ownerContent;
            boxedStructureOwnerIndex = index;
        }

        private void Update(object newValue, Index index, bool updateStructureOwner)
        {
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
                if (updateStructureOwner)
                {
                    boxedStructureOwner?.UpdateFromMember(newValue, boxedStructureOwnerIndex);
                }
            }
        }

        public override string ToString()
        {
            return $"{{Node: Boxed {Type.Name} = [{Value}]}}";
        }
    }
}
