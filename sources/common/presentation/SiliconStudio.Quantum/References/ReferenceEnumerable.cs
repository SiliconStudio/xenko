// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Extensions;
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
        private List<ObjectReference> references;
        private List<Index> indices;

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
        public int Count => references?.Count ?? 0;

        /// <summary>
        /// Gets the indices of each reference in this instance.
        /// </summary>
        public IReadOnlyList<Index> Indices => indices;

        /// <inheritdoc/>
        public ObjectReference this[Index index] { get { return references.Single(x => Equals(x.Index, index)); } }

        /// <inheritdoc/>
        public bool HasIndex(Index index)
        {
            return indices?.Any(x => x.Equals(index)) ?? false;
        }

        /// <summary>
        /// Indicates whether this instance of <see cref="ReferenceEnumerable"/> contains an element which as the given index.
        /// </summary>
        /// <param name="index">The index to look for.</param>
        /// <returns><c>true</c> if an object with the given index exists in this instance, <c>false</c> otherwise.</returns>
        public bool ContainsIndex(object index)
        {
            return references != null && references.Any(x => Equals(x.Index, index));
        }

        public void Refresh(IGraphNode ownerNode, NodeContainer nodeContainer, NodeFactoryDelegate nodeFactory)
        {
            var newObjectValue = ownerNode.Content.Value;
            if (!(newObjectValue is IEnumerable)) throw new ArgumentException(@"The object is not an IEnumerable", nameof(newObjectValue));

            ObjectValue = newObjectValue;

            // First, let's build a new list of uninitialized references
            var newReferences = new List<ObjectReference>(IsDictionary
                ? ((IEnumerable)ObjectValue).Cast<object>().Select(x => (ObjectReference)Reference.CreateReference(GetValue(x), elementType, GetKey(x)))
                : ((IEnumerable)ObjectValue).Cast<object>().Select((x, i) => (ObjectReference)Reference.CreateReference(x, elementType, new Index(i))));

            // The reference need to be updated if it has never been initialized, if the number of items is different, or if any index or any value is different.
            var needUpdate = references == null || newReferences.Count != references.Count || newReferences.Zip(references).Any(x => !x.Item1.Index.Equals(x.Item2.Index) || !Equals(x.Item1.ObjectValue, x.Item2.ObjectValue));
            if (needUpdate)
            {
                // We create a dictionary that maps values of the old list of references to their corresponding target node.
                var dictionary = references?.Where(x => x.ObjectValue != null && !(x.TargetNode?.Content is BoxedContent)).ToDictionary(x => x.ObjectValue) ?? new Dictionary<object, ObjectReference>();
                foreach (var newReference in newReferences)
                {
                    if (newReference.ObjectValue != null)
                    {
                        ObjectReference oldReference;
                        if (dictionary.TryGetValue(newReference.ObjectValue, out oldReference))
                        {
                            // If this value was already present in the old list of reference, just use the same target node in the new list.
                            newReference.SetTarget(oldReference.TargetNode);
                        }
                        else
                        {
                            // Otherwise, do a full update that will properly initialize the new reference.
                            newReference.Refresh(ownerNode, nodeContainer, nodeFactory, newReference.Index);
                        }
                    }
                }
                references = newReferences;
                indices = newReferences.Select(x => x.Index).ToList();
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
            return references.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return references.GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Equals(IReference other)
        {
            var otherEnumerable = other as ReferenceEnumerable;
            return otherEnumerable != null && DesignExtensions.Equals<IReference>(references, otherEnumerable.references);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string text = "(" + references.Count + " references";
            if (references.Count > 0)
            {
                text += ": ";
                text += string.Join(", ", references);
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
    }
}
