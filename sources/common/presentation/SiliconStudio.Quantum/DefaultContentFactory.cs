// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// This class is an implementation of the <see cref="IContentFactory"/> interface that can construct <see cref="ObjectContent"/>, <see cref="BoxedContent"/>
    /// and <see cref="MemberContent"/> instances.
    /// </summary>
    public class DefaultContentFactory : IContentFactory
    {
        /// <inheritdoc/>
        public virtual IContent CreateObjectContent(INodeBuilder nodeBuilder, object obj, ITypeDescriptor descriptor, bool isPrimitive, bool shouldProcessReference)
        {
            var reference = nodeBuilder.CreateReferenceForNode(descriptor.Type, obj) as ReferenceEnumerable;
            return new ObjectContent(obj, descriptor, isPrimitive, reference) { ShouldProcessReference = shouldProcessReference };
        }

        /// <inheritdoc/>
        public virtual IContent CreateBoxedContent(INodeBuilder nodeBuilder, object structure, ITypeDescriptor descriptor, bool isPrimitive)
        {
            return new BoxedContent(structure, descriptor, isPrimitive);
        }

        /// <inheritdoc/>
        public virtual IContent CreateMemberContent(INodeBuilder nodeBuilder, IContent container, IMemberDescriptor member, bool isPrimitive, object value, bool shouldProcessReference)
        {
            var reference = nodeBuilder.CreateReferenceForNode(member.Type, value);
            // TODO: OverridableMemberContent should not be in the DefaultContentFactory but in an asset-specific factory.
            return new OverridableMemberContent(nodeBuilder, container, member, isPrimitive, reference)
            {
                ShouldProcessReference = shouldProcessReference
            };
        }
    }
}