using System;
using System.Collections;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Visitors
{
    public abstract class AssetMemberVisitorBase : AssetVisitorBase
    {
        private readonly MemberPath memberPath;

        protected AssetMemberVisitorBase(MemberPath path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            memberPath = path;
        }

        /// <inheritdoc/>
        public override void VisitArrayItem(Array array, ArrayDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
        {
            if (CurrentPath.Match(memberPath))
            {
                base.VisitArrayItem(array, descriptor, index, item, itemDescriptor);
            }
            else
            {
                VisitAssetMember(item, itemDescriptor);
            }
        }

        /// <inheritdoc/>
        public override void VisitCollectionItem(IEnumerable collection, CollectionDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
        {
            if (CurrentPath.Match(memberPath))
            {
                base.VisitCollectionItem(collection, descriptor, index, item, itemDescriptor);
            }
            else
            {
                VisitAssetMember(item, itemDescriptor);
            }
        }

        /// <inheritdoc/>
        public override void VisitDictionaryKeyValue(object dictionary, DictionaryDescriptor descriptor, object key, ITypeDescriptor keyDescriptor, object value, ITypeDescriptor valueDescriptor)
        {
            if (CurrentPath.Match(memberPath))
            {
                base.VisitDictionaryKeyValue(dictionary, descriptor, key, keyDescriptor, value, valueDescriptor);
            }
            else
            {
                Visit(key, keyDescriptor);
                VisitAssetMember(value, valueDescriptor);
            }
        }

        /// <inheritdoc/>
        public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
        {
            if (CurrentPath.Match(memberPath))
            {
                base.VisitObjectMember(container, containerDescriptor, member, value);
            }
            else
            {
                VisitAssetMember(value, member.TypeDescriptor);
            }
        }

        /// <inheritdoc/>
        public override void VisitPrimitive(object primitive, PrimitiveDescriptor descriptor)
        {
            if (CurrentPath.Match(memberPath))
            {
                base.VisitPrimitive(primitive, descriptor);
            }
            else
            {
                VisitAssetMember(primitive, descriptor);
            }
        }

        protected abstract void VisitAssetMember(object value, ITypeDescriptor descriptor);
    }
}
