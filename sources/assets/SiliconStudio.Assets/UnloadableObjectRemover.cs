using System;
using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Removes objects implementing <see cref="IYamlProxy"/> from the object.
    /// </summary>
    public class UnloadableObjectRemover : AssetVisitorBase
    {
        private readonly List<MemberPath> memberPaths = new List<MemberPath>();

        public void Run(object obj)
        {
            Reset();
            memberPaths.Clear();
            Visit(obj);
            // We apply changes in opposite visit order so that indices remains valid when we remove objects while iterating
            for (int index = memberPaths.Count - 1; index >= 0; index--)
            {
                var memberPath = memberPaths[index];
                memberPath.Apply(obj, MemberPathAction.ValueClear, null);
            }
            memberPaths.Clear();
        }

        public override void VisitCollectionItem(IEnumerable collection, CollectionDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
        {
            if (ProcessObject(item, descriptor.ElementType)) return;

            base.VisitCollectionItem(collection, descriptor, index, item, itemDescriptor);
        }

        public override void VisitArrayItem(Array array, ArrayDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
        {
            if (ProcessObject(item, descriptor.ElementType)) return;

            base.VisitArrayItem(array, descriptor, index, item, itemDescriptor);
        }

        public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
        {
            if (ProcessObject(value, member.TypeDescriptor.Type)) return;

            base.VisitObjectMember(container, containerDescriptor, member, value);
        }

        public override void VisitDictionaryKeyValue(object dictionary, DictionaryDescriptor descriptor, object key, ITypeDescriptor keyDescriptor, object value, ITypeDescriptor valueDescriptor)
        {
            // TODO: CurrentPath is valid only for value, not key
            //if (ProcessObject(key, keyDescriptor.Type)) key = null;
            if (ProcessObject(value, valueDescriptor.Type)) return;

            Visit(value, valueDescriptor);
            //base.VisitDictionaryKeyValue(dictionary, descriptor, key, keyDescriptor, value, valueDescriptor);
        }

        private bool ProcessObject(object obj, Type expectedType)
        {
            if (obj is IUnloadable)
            {
                memberPaths.Add(CurrentPath.Clone());

                // Don't recurse inside
                return true;
            }
            return false;
        }
    }
}