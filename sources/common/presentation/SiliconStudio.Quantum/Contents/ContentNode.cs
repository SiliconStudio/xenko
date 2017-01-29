// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// A base abstract implementation of the <see cref="IContentNode"/> interface.
    /// </summary>
    public abstract class ContentNode : IInitializingGraphNode
    {
        private readonly List<INodeCommand> commands = new List<INodeCommand>();
        protected bool isSealed;

        protected ContentNode(Guid guid, ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
        {
            if (guid == Guid.Empty) throw new ArgumentException(@"The guid must be different from Guid.Empty.", nameof(guid));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            Guid = guid;
            Descriptor = descriptor;
            IsPrimitive = isPrimitive;
            TargetReference = reference as ObjectReference;
            ItemReferences = reference as ReferenceEnumerable;
        }

        /// <inheritdoc/>
        public Type Type => Descriptor.Type;

        /// <inheritdoc/>
        [Obsolete("Use method Retrieve()")] 
        public abstract object Value { get; }

        /// <inheritdoc/>
        public bool IsPrimitive { get; }

        /// <inheritdoc/>
        public ITypeDescriptor Descriptor { get; }

        /// <inheritdoc/>
        public bool IsReference => TargetReference != null || ItemReferences != null;

        public ObjectReference TargetReference { get; }

        public ReferenceEnumerable ItemReferences { get; }

        /// <inheritdoc/>
        public IEnumerable<Index> Indices => GetIndices();

        /// <inheritdoc/>
        public Guid Guid { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<INodeCommand> Commands => commands;

        /// <inheritdoc/>
        public object Retrieve() => Retrieve(Index.Empty);

        /// <inheritdoc/>
        public virtual object Retrieve(Index index)
        {
            return Content.Retrieve(Value, index, Descriptor);
        }

        /// <inheritdoc/>
        public virtual void Update(object newValue)
        {
            Update(newValue, Index.Empty);
        }

        /// <inheritdoc/>
        public abstract void Update(object newValue, Index index);

        /// <inheritdoc/>
        public abstract void Add(object newItem);

        /// <inheritdoc/>
        public abstract void Add(object newItem, Index itemIndex);

        /// <inheritdoc/>
        public abstract void Remove(object item, Index itemIndex);

        /// <summary>
        /// Updates this content from one of its member.
        /// </summary>
        /// <param name="newValue">The new value for this content.</param>
        /// <param name="index">new index of the value to update.</param>
        /// <remarks>
        /// This method is intended to update a boxed content when one of its member changes.
        /// It allows to properly update boxed structs.
        /// </remarks>
        protected internal abstract void UpdateFromMember(object newValue, Index index);

        private IEnumerable<Index> GetIndices()
        {
            var enumRef = ItemReferences;
            if (enumRef != null)
                return enumRef.Indices;

            return GetIndices(this);
        }

        public static IEnumerable<Index> GetIndices([NotNull] IContentNode node)
        {
            var collectionDescriptor = node.Descriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
            {
                return Enumerable.Range(0, collectionDescriptor.GetCollectionCount(node.Value)).Select(x => new Index(x));
            }
            var dictionaryDescriptor = node.Descriptor as DictionaryDescriptor;
            return dictionaryDescriptor?.GetKeys(node.Value).Cast<object>().Select(x => new Index(x));
        }

        /// <inheritdoc/>
        public IObjectNode IndexedTarget(Index index)
        {
            if (index == Index.Empty) throw new ArgumentException(@"index cannot be Index.Empty when invoking this method.", nameof(index));
            if (ItemReferences == null) throw new InvalidOperationException(@"The node does not contain enumerable references.");
            return ItemReferences[index].TargetNode;
        }

        /// <summary>
        /// Add a command to this node. The node must not have been sealed yet.
        /// </summary>
        /// <param name="command">The node command to add.</param>
        public void AddCommand(INodeCommand command)
        {
            if (isSealed)
                throw new InvalidOperationException("Unable to add a command to a GraphNode that has been sealed");

            commands.Add(command);
        }

        /// <summary>
        /// Remove a command from this node. The node must not have been sealed yet.
        /// </summary>
        /// <param name="command">The node command to remove.</param>
        public void RemoveCommand(INodeCommand command)
        {
            if (isSealed)
                throw new InvalidOperationException("Unable to remove a command from a GraphNode that has been sealed");

            commands.Remove(command);
        }

        /// <summary>
        /// Seal the node, indicating its construction is finished and that no more children or commands will be added.
        /// </summary>
        public void Seal()
        {
            isSealed = true;
        }
    }
}
