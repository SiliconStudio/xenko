// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Provides a descriptor for a <see cref="System.Collections.ICollection"/>.
    /// </summary>
    public class CollectionDescriptor : ObjectDescriptor
    {
        private static readonly object[] EmptyObjects = new object[0];
        private static readonly List<string> ListOfMembersToRemove = new List<string> { "Capacity", "Count", "IsReadOnly", "IsFixedSize", "IsSynchronized", "SyncRoot", "Comparer" };

        private readonly Func<object, bool> IsReadOnlyFunction;
        private readonly Func<object, int> GetCollectionCountFunction;
        private readonly Func<object, int, object> GetIndexedItem;
        private readonly Action<object, int, object> SetIndexedItem;
        private readonly Action<object, object> CollectionAddFunction;
        private readonly Action<object, int, object> CollectionInsertFunction;
        private readonly Action<object, int> CollectionRemoveAtFunction;
        private readonly Action<object> CollectionClearFunction;
        private readonly bool hasIndexerAccessors;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionDescriptor" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentException">Expecting a type inheriting from System.Collections.ICollection;type</exception>
        public CollectionDescriptor(ITypeDescriptorFactory factory, Type type) : base(factory, type)
        {
            if (!IsCollection(type))
                throw new ArgumentException("Expecting a type inheriting from System.Collections.ICollection", "type");

            // Gets the element type
            var collectionType = type.GetInterface(typeof(IEnumerable<>));
            ElementType = (collectionType != null) ? collectionType.GetGenericArguments()[0] : typeof(object);
            Category = DescriptorCategory.Collection;
            bool typeSupported = false;

            // implements ICollection<T> 
            Type itype = type.GetInterface(typeof(ICollection<>));
            if (itype != null)
            {
                var add = itype.GetMethod("Add", new[] {ElementType});
                CollectionAddFunction = (obj, value) => add.Invoke(obj, new[] {value});
                var clear = itype.GetMethod("Clear", Type.EmptyTypes);
                CollectionClearFunction = obj => clear.Invoke(obj, EmptyObjects);
                var countMethod = itype.GetProperty("Count").GetGetMethod();
                GetCollectionCountFunction = o => (int)countMethod.Invoke(o, null);
                var isReadOnly = itype.GetProperty("IsReadOnly").GetGetMethod();
                IsReadOnlyFunction = obj => (bool)isReadOnly.Invoke(obj, null);
                typeSupported = true;
            }
            // implements IList<T>
            itype = type.GetInterface(typeof(IList<>));
            if (itype != null)
            {
                var insert = itype.GetMethod("Insert", new[] { typeof(int), ElementType });
                CollectionInsertFunction = (obj, index, value) => insert.Invoke(obj, new[] { index, value });
                var removeAt = itype.GetMethod("RemoveAt", new[] { typeof(int) });
                CollectionRemoveAtFunction = (obj, index) => removeAt.Invoke(obj, new object[] { index });
                var getItem = itype.GetMethod("get_Item", new[] { typeof(int) });
                var setItem = itype.GetMethod("set_Item", new[] { typeof(int), ElementType });
                GetIndexedItem = (obj, index) => getItem.Invoke(obj, new object[] { index });
                SetIndexedItem = (obj, index, value) => setItem.Invoke(obj, new[] { index, value });
                hasIndexerAccessors = true;
            }
            // implements IList
            if (!typeSupported && typeof(IList).IsAssignableFrom(type))
            {
                CollectionAddFunction = (obj, value) => ((IList)obj).Add(value);
                CollectionClearFunction = obj => ((IList)obj).Clear();
                CollectionInsertFunction = (obj, index, value) => ((IList)obj).Insert(index, value);
                CollectionRemoveAtFunction = (obj, index) => ((IList)obj).RemoveAt(index);
                GetCollectionCountFunction = o => ((IList)o).Count;
                GetIndexedItem = (obj, index) => ((IList)obj)[index];
                SetIndexedItem = (obj, index, value) => ((IList)obj)[index] = value;
                IsReadOnlyFunction = obj => ((IList)obj).IsReadOnly;
                hasIndexerAccessors = true;
                typeSupported = true;
            }

            if (!typeSupported)
            {
                throw new ArgumentException("Type [{0}] is not supported as a modifiable collection".ToFormat(type), "type");
            }
        }

        /// <summary>
        /// Gets or sets the type of the element.
        /// </summary>
        /// <value>The type of the element.</value>
        public Type ElementType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this collection type has add method.
        /// </summary>
        /// <value><c>true</c> if this instance has add; otherwise, <c>false</c>.</value>
        public bool HasAdd
        {
            get
            {
                return CollectionAddFunction != null;
            }
        }


        /// <summary>
        /// Gets a value indicating whether this collection type has insert method.
        /// </summary>
        /// <value><c>true</c> if this instance has insert; otherwise, <c>false</c>.</value>
        public bool HasInsert
        {
            get
            {
                return CollectionInsertFunction != null;
            }
        }
        /// <summary>
        /// Gets a value indicating whether this collection type has RemoveAt method.
        /// </summary>
        /// <value><c>true</c> if this instance has RemoveAt; otherwise, <c>false</c>.</value>
        public bool HasRemoveAt
        {
            get
            {
                return CollectionRemoveAtFunction != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this collection type has valid indexer accessors.
        /// If so, <see cref="SetValue"/> and <see cref="GetValue"/> can be invoked.
        /// </summary>
        /// <value><c>true</c> if this instance has a valid indexer setter; otherwise, <c>false</c>.</value>
        public bool HasIndexerAccessors
        {
            get
            {
                return hasIndexerAccessors;
            }
        }

        /// <summary>
        /// Returns the value matching the given index in the collection.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="index">The index.</param>
        public object GetValue(object list, object index)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (!(index is int)) throw new ArgumentException("The index must be an int.");
            return GetValue(list, (int)index);
        }

        /// <summary>
        /// Returns the value matching the given index in the collection.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="index">The index.</param>
        public object GetValue(object list, int index)
        {
            if (list == null) throw new ArgumentNullException("list");
            return GetIndexedItem(list, index);
        }

        public void SetValue(object list, object index, object value)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (!(index is int)) throw new ArgumentException("The index must be an int.");
            SetValue(list, (int)index, value);
        }

        public void SetValue(object list, int index, object value)
        {
            if (list == null) throw new ArgumentNullException("list");
            SetIndexedItem(list, index, value);
        }

        /// <summary>
        /// Clears the specified collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public void Clear(object collection)
        {
            CollectionClearFunction(collection);
        }

        /// <summary>
        /// Add to the collections of the same type than this descriptor.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="value">The value to add to this collection.</param>
        public void Add(object collection, object value)
        {
            CollectionAddFunction(collection, value);
        }

        /// <summary>
        /// Insert to the collections of the same type than this descriptor.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="index">The index of the insertion.</param>
        /// <param name="value">The value to insert to this collection.</param>
        public void Insert(object collection, int index, object value)
        {
            CollectionInsertFunction(collection, index, value);
        }

        /// <summary>
        /// Remove item at the given index from the collections of the same type.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="index">The index of the item to remove from this collection.</param>
        public void RemoveAt(object collection, int index)
        {
            CollectionRemoveAtFunction(collection, index);
        }

        /// <summary>
        /// Determines whether the specified collection is read only.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns><c>true</c> if the specified collection is read only; otherwise, <c>false</c>.</returns>
        public bool IsReadOnly(object collection)
        {
            return collection == null || IsReadOnlyFunction == null || IsReadOnlyFunction(collection);
        }

        /// <summary>
        /// Determines the number of elements of a collection, -1 if it cannot determine the number of elements.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>The number of elements of a collection, -1 if it cannot determine the number of elements.</returns>
        public int GetCollectionCount(object collection)
        {
            return collection == null || GetCollectionCountFunction == null ? -1 : GetCollectionCountFunction(collection);
        }

        /// <summary>
        /// Determines whether the specified type is collection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is collection; otherwise, <c>false</c>.</returns>
        public static bool IsCollection(Type type)
        {
            return TypeHelper.IsCollection(type);
        }

        protected override bool PrepareMember(MemberDescriptorBase member)
        {
            // Filter members
            if (member is PropertyDescriptor && ListOfMembersToRemove.Contains(member.Name))
            //if (member is PropertyDescriptor && (member.DeclaringType.Namespace ?? string.Empty).StartsWith(SystemCollectionsNamespace) && ListOfMembersToRemove.Contains(member.Name))
            {
                return false;
            }

            return !IsCompilerGenerated && base.PrepareMember(member);
        }
    }
}
