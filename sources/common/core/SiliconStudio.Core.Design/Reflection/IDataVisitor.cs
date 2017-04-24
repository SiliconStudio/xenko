// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Interface for visiting serializable data (binary, yaml and editor).
    /// </summary>
    public interface IDataVisitor
    {
        /// <summary>
        /// Visits a null.
        /// </summary>
        void VisitNull();

        /// <summary>
        /// Visits a primitive (int, float, string...etc.)
        /// </summary>
        /// <param name="primitive">The primitive.</param>
        /// <param name="descriptor">The descriptor.</param>
        void VisitPrimitive(object primitive, PrimitiveDescriptor descriptor);

        /// <summary>
        /// Visits an object (either a class or a struct)
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="visitMembers"></param>
        void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers);

        /// <summary>
        /// Visits an object member.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="containerDescriptor">The container descriptor.</param>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value);

        /// <summary>
        /// Visits an array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="descriptor">The descriptor.</param>
        void VisitArray(Array array, ArrayDescriptor descriptor);

        /// <summary>
        /// Visits an array item.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <param name="itemDescriptor">The item descriptor.</param>
        void VisitArrayItem(Array array, ArrayDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor);

        /// <summary>
        /// Visits a collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="descriptor">The descriptor.</param>
        void VisitCollection(IEnumerable collection, CollectionDescriptor descriptor);

        /// <summary>
        /// Visits a collection item.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <param name="itemDescriptor">The item descriptor.</param>
        void VisitCollectionItem(IEnumerable collection, CollectionDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor);

        /// <summary>
        /// Visits a dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="descriptor">The descriptor.</param>
        void VisitDictionary(object dictionary, DictionaryDescriptor descriptor);

        /// <summary>
        /// Visits a dictionary key-value.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="key">The key.</param>
        /// <param name="keyDescriptor">The key descriptor.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueDescriptor">The value descriptor.</param>
        void VisitDictionaryKeyValue(object dictionary, DictionaryDescriptor descriptor, object key, ITypeDescriptor keyDescriptor, object value, ITypeDescriptor valueDescriptor);
    }
}
