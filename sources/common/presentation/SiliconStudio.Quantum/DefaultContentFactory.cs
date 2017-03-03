// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// This class is an implementation of the <see cref="IContentFactory"/> interface that can construct <see cref="ObjectNode"/>, <see cref="BoxedNode"/>
    /// and <see cref="MemberNode"/> instances.
    /// </summary>
    public class DefaultContentFactory : IContentFactory
    {
        /// <inheritdoc/>
        public virtual IGraphNode CreateObjectContent(INodeBuilder nodeBuilder, Guid guid, object obj, ITypeDescriptor descriptor, bool isPrimitive)
        {
            var reference = nodeBuilder.CreateReferenceForNode(descriptor.Type, obj, false) as ReferenceEnumerable;
            return new ObjectNode(nodeBuilder, obj, guid, descriptor, isPrimitive, reference);
        }

        /// <inheritdoc/>
        public virtual IGraphNode CreateBoxedContent(INodeBuilder nodeBuilder, Guid guid, object structure, ITypeDescriptor descriptor, bool isPrimitive)
        {
            return new BoxedNode(nodeBuilder, structure, guid, descriptor, isPrimitive);
        }

        /// <inheritdoc/>
        public virtual IGraphNode CreateMemberContent(INodeBuilder nodeBuilder, Guid guid, IObjectNode parent, IMemberDescriptor member, bool isPrimitive, object value)
        {
            var reference = nodeBuilder.CreateReferenceForNode(member.Type, value, true);
            return new MemberNode(nodeBuilder, guid, parent, member, isPrimitive, reference);
        }
    }
}
