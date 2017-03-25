// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// A base abstract implementation of the <see cref="IGraphNode"/> interface.
    /// </summary>
    public abstract class GraphNodeBase : IInitializingGraphNode
    {
        protected readonly NodeContainer NodeContainer;

        protected GraphNodeBase(NodeContainer nodeContainer, Guid guid, [NotNull] ITypeDescriptor descriptor, bool isPrimitive)
        {
            if (guid == Guid.Empty) throw new ArgumentException(@"The guid must be different from Guid.Empty.", nameof(guid));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            NodeContainer = nodeContainer;
            Guid = guid;
            Descriptor = descriptor;
            IsPrimitive = isPrimitive;
        }

        /// <inheritdoc/>
        public Type Type => Descriptor.Type;

        /// <inheritdoc/>
        public bool IsPrimitive { get; }

        /// <inheritdoc/>
        public ITypeDescriptor Descriptor { get; }

        /// <inheritdoc/>
        public abstract bool IsReference { get; }

        /// <inheritdoc/>
        public Guid Guid { get; }

        /// <inheritdoc/>
        protected abstract object Value { get; }

        /// <summary>
        /// Gets whether this node has been sealed.
        /// </summary>
        protected bool IsSealed { get; private set; }

        /// <inheritdoc/>
        public object Retrieve() => Retrieve(Index.Empty);

        /// <inheritdoc/>
        public virtual object Retrieve(Index index)
        {
            return Content.Retrieve(Value, index, Descriptor);
        }

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

        public static IEnumerable<Index> GetIndices([NotNull] IGraphNode node)
        {
            var collectionDescriptor = node.Descriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
            {
                return Enumerable.Range(0, collectionDescriptor.GetCollectionCount(node.Retrieve())).Select(x => new Index(x));
            }
            var dictionaryDescriptor = node.Descriptor as DictionaryDescriptor;
            return dictionaryDescriptor?.GetKeys(node.Retrieve()).Cast<object>().Select(x => new Index(x));
        }

        /// <summary>
        /// Seal the node, indicating its construction is finished and that no more children or commands will be added.
        /// </summary>
        public void Seal()
        {
            IsSealed = true;
        }
    }
}
