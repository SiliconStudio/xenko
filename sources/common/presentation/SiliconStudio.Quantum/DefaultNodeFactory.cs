// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// This class is an implementation of the <see cref="INodeFactory"/> interface that can construct <see cref="ObjectNode"/>, <see cref="BoxedNode"/>
    /// and <see cref="MemberNode"/> instances.
    /// </summary>
    public class DefaultNodeFactory : INodeFactory
    {
        /// <inheritdoc/>
        public virtual IGraphNode CreateObjectContent(INodeBuilder nodeBuilder, Guid guid, object obj, ITypeDescriptor descriptor, bool isPrimitive)
        {
            if (nodeBuilder == null) throw new ArgumentNullException(nameof(nodeBuilder));
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            var reference = nodeBuilder.CreateReferenceForNode(descriptor.Type, obj, false) as ReferenceEnumerable;
            return new ObjectNode(nodeBuilder, obj, guid, descriptor, isPrimitive, reference);
        }

        /// <inheritdoc/>
        public virtual IGraphNode CreateBoxedContent(INodeBuilder nodeBuilder, Guid guid, object structure, ITypeDescriptor descriptor, bool isPrimitive)
        {
            if (nodeBuilder == null) throw new ArgumentNullException(nameof(nodeBuilder));
            if (structure == null) throw new ArgumentNullException(nameof(structure));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            return new BoxedNode(nodeBuilder, structure, guid, descriptor, isPrimitive);
        }

        /// <inheritdoc/>
        public virtual IGraphNode CreateMemberContent(INodeBuilder nodeBuilder, Guid guid, IObjectNode parent, IMemberDescriptor member, bool isPrimitive, object value)
        {
            if (nodeBuilder == null) throw new ArgumentNullException(nameof(nodeBuilder));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (member == null) throw new ArgumentNullException(nameof(member));
            var reference = nodeBuilder.CreateReferenceForNode(member.Type, value, true);
            return new MemberNode(nodeBuilder, guid, parent, member, isPrimitive, reference);
        }
    }
}
