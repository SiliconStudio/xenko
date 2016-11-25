// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.References
{
    /// <summary>
    /// A class representing an enumeration of references to multiple objects.
    /// </summary>
    public sealed class ReferenceEnumerable : IReference, IEnumerable<ObjectReference>
    {
        private readonly Type elementType;

        private HybridDictionary<Index, ObjectReference> items;

        internal ReferenceEnumerable(IEnumerable enumerable, Type enumerableType, Index index)
        {
            Reference.CheckReferenceCreationSafeGuard();
            Type = enumerableType;
            Index = index;
            ObjectValue = enumerable;

            if (enumerableType.HasInterface(typeof(IDictionary<,>)))
                elementType = enumerableType.GetInterface(typeof(IDictionary<,>)).GetGenericArguments()[1];
            else if (enumerableType.HasInterface(typeof(IEnumerable<>)))
                elementType = enumerableType.GetInterface(typeof(IEnumerable<>)).GetGenericArguments()[0];
            else
                elementType = typeof(object);
        }

        /// <inheritdoc/>
        public object ObjectValue { get; private set; }

        /// <inheritdoc/>
        public Type Type { get; }

        /// <inheritdoc/>
        public Index Index { get; }

        /// <inheritdoc/>
        public ObjectReference AsObject { get { throw new InvalidCastException("This reference is not an ObjectReference"); } }

        /// <inheritdoc/>
        public ReferenceEnumerable AsEnumerable => this;

        /// <summary>
        /// Gets whether this reference enumerates a dictionary collection.
        /// </summary>
        public bool IsDictionary => ObjectValue is IDictionary || ObjectValue.GetType().HasInterface(typeof(IDictionary<,>));

        /// <inheritdoc/>
        public int Count => items?.Count ?? 0;

        /// <summary>
        /// Gets the indices of each reference in this instance.
        /// </summary>
        public IReadOnlyCollection<Index> Indices { get; private set; }

        /// <inheritdoc/>
        public ObjectReference this[Index index] => items[index];

        /// <inheritdoc/>
        public bool HasIndex(Index index)
        {
            return items?.ContainsKey(index) ?? false;
        }

        public void Refresh(IGraphNode ownerNode, NodeContainer nodeContainer, NodeFactoryDelegate nodeFactory)
        {
            var newObjectValue = ownerNode.Content.Value;
            if (!(newObjectValue is IEnumerable)) throw new ArgumentException(@"The object is not an IEnumerable", nameof(newObjectValue));

            ObjectValue = newObjectValue;

            var newReferences = new HybridDictionary<Index, ObjectReference>();
            if (IsDictionary)
            {
                foreach (var item in (IEnumerable)ObjectValue)
                {
                    var key = GetKey(item);
                    var value = (ObjectReference)Reference.CreateReference(GetValue(item), elementType, key);
                    newReferences.Add(key, value);
                }
            }
            else
            {
                var i = 0;
                foreach (var item in (IEnumerable)ObjectValue)
                {
                    var key = new Index(i);
                    var value = (ObjectReference)Reference.CreateReference(item, elementType, key);
                    newReferences.Add(key, value);
                    ++i;
                }
            }

            // The reference need to be updated if it has never been initialized, if the number of items is different, or if any index or any value is different.
            var needUpdate = items == null || newReferences.Count != items.Count || !AreItemsEqual(items, newReferences);
            if (needUpdate)
            {
                // We create a mapping values of the old list of references to their corresponding target node. We use a list because we can have multiple times the same target in items.
                var oldReferenceMapping = new List<KeyValuePair<object, ObjectReference>>();
                if (items != null)
                {
                    oldReferenceMapping.AddRange(items.Values.Where(x => x.ObjectValue != null && !(x.TargetNode?.Content is BoxedContent)).Select(x => new KeyValuePair<object, ObjectReference>(x.ObjectValue, x)));
                }

                foreach (var newReference in newReferences)
                {
                    if (newReference.Value.ObjectValue != null)
                    {
                        var found = false;
                        var i = 0;
                        foreach (var item in oldReferenceMapping)
                        {
                            if (Equals(newReference.Value.ObjectValue, item.Key))
                            {
                                // If this value was already present in the old list of reference, just use the same target node in the new list.
                                newReference.Value.SetTarget(item.Value.TargetNode);
                                // Remove consumed existing reference so if there is a second entry with the same "key", it will be the other reference that will be used.
                                oldReferenceMapping.RemoveAt(i);
                                found = true;
                                break;
                            }
                            ++i;
                        }
                        if (!found)
                        {
                            // Otherwise, do a full update that will properly initialize the new reference.
                            newReference.Value.Refresh(ownerNode, nodeContainer, nodeFactory, newReference.Key);
                        }
                    }
                }
                items = newReferences;
                // Remark: this works because both KeyCollection and List implements IReadOnlyCollection. Any internal change to HybridDictionary might break this!
                Indices = (IReadOnlyCollection<Index>)newReferences.Keys;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ObjectReference> Enumerate()
        {
            return this;
        }

        /// <inheritdoc/>
        public IEnumerator<ObjectReference> GetEnumerator()
        {
            return new ReferenceEnumerator(this);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Equals(IReference other)
        {
            var otherEnumerable = other as ReferenceEnumerable;
            if (otherEnumerable == null)
                return false;

            return ReferenceEquals(this, otherEnumerable) || AreItemsEqual(items, otherEnumerable.items);
        }

        private static bool AreItemsEqual(HybridDictionary<Index, ObjectReference> items1, HybridDictionary<Index, ObjectReference> items2)
        {
            if (ReferenceEquals(items1, items2))
                return true;

            if (items1 == null || items2 == null)
                return false;

            if (items1.Count != items2.Count)
                return false;

            foreach (var item in items1)
            {
                ObjectReference otherItem;
                if (!items2.TryGetValue(item.Key, out otherItem))
                    return false;

                if (!otherItem.Equals(item.Value))
                    return false;
            }

            return true;

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string text = "(" + items.Count + " references";
            if (items.Count > 0)
            {
                text += ": ";
                text += string.Join(", ", items.Values);
            }
            text += ")";
            return text;
        }

        private static Index GetKey(object keyValuePair)
        {
            var type = keyValuePair.GetType();
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(KeyValuePair<,>)) throw new ArgumentException("The given object is not a KeyValuePair.");
            var keyProperty = type.GetProperty(nameof(KeyValuePair<object, object>.Key));
            return new Index(keyProperty.GetValue(keyValuePair));
        }

        private static object GetValue(object keyValuePair)
        {
            var type = keyValuePair.GetType();
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(KeyValuePair<,>)) throw new ArgumentException("The given object is not a KeyValuePair.");
            var valueProperty = type.GetProperty(nameof(KeyValuePair<object, object>.Value));
            return valueProperty.GetValue(keyValuePair);
        }

        /// <summary>
        /// An enumerator for <see cref="ReferenceEnumerable"/> that enumerates in proper item order.
        /// </summary>
        private class ReferenceEnumerator : IEnumerator<ObjectReference>
        {
            private readonly IEnumerator<Index> indexEnumerator;
            private ReferenceEnumerable obj;

            public ReferenceEnumerator(ReferenceEnumerable obj)
            {
                this.obj = obj;
                indexEnumerator = obj.Indices.GetEnumerator();
            }

            public void Dispose()
            {
                obj = null;
                indexEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                return indexEnumerator.MoveNext();
            }

            public void Reset()
            {
                indexEnumerator.Reset();
            }

            public ObjectReference Current => obj.items[indexEnumerator.Current];

            object IEnumerator.Current => obj.items[indexEnumerator.Current];
        }
    }
}
