// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// An implementation of <see cref="IContentNode"/> that gives access to an object or a boxed struct.
    /// </summary>
    /// <remarks>This content is not serialized by default.</remarks>
    public class ObjectContent : ContentNode, IObjectNode, IInitializingObjectNode
    {
        private object value;

        public ObjectContent(object value, Guid guid, ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
            : base(descriptor.Type.Name, guid, descriptor, isPrimitive, reference)
        {
            if (reference is ObjectReference)
                throw new ArgumentException($"An {nameof(ObjectContent)} cannot contain an {nameof(ObjectReference)}");
            this.value = value;
        }

        public override object Value => value;

        /// <summary>
        /// Add a child to this node. The node must not have been sealed yet.
        /// </summary>
        /// <param name="child">The child node to add.</param>
        /// <param name="allowIfReference">if set to <c>false</c> throw an exception if <see cref="IContentNode.Reference"/> is not null.</param>
        public void AddMember(MemberContent child, bool allowIfReference = false)
        {
            if (isSealed)
                throw new InvalidOperationException("Unable to add a child to a GraphNode that has been sealed");

            if (child.Parent != null)
                throw new ArgumentException(@"This node has already been registered to a different parent", nameof(child));

            if (Reference != null && !allowIfReference)
                throw new InvalidOperationException("A GraphNode cannot have children when its content hold a reference.");

            child.Parent = this;
            children.Add(child);
            childrenMap.Add(child.Name, child);
        }

        /// <inheritdoc/>
        public override void Update(object newValue, Index index)
        {
            throw new InvalidOperationException("An ObjectContent value cannot be modified after it has been constructed");
        }

        /// <inheritdoc/>
        public override void Add(object newItem)
        {
            throw new InvalidOperationException("An ObjectContent value cannot be modified after it has been constructed");
        }

        /// <inheritdoc/>
        public override void Add(object newItem, Index index)
        {
            throw new InvalidOperationException("An ObjectContent value cannot be modified after it has been constructed");
        }

        /// <inheritdoc/>
        public override void Remove(object item, Index index)
        {
            throw new InvalidOperationException("An ObjectContent value cannot be modified after it has been constructed");
        }

        /// <inheritdoc/>
        protected internal override void UpdateFromMember(object newValue, Index index)
        {
            throw new InvalidOperationException("An ObjectContent value cannot be modified after it has been constructed");
        }

        protected void SetValue(object newValue)
        {
            value = newValue;
        }
    }
}
