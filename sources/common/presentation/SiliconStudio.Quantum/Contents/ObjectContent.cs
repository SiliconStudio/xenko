// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// An implementation of <see cref="IContentNode"/> that gives access to an object or a boxed struct.
    /// </summary>
    /// <remarks>This content is not serialized by default.</remarks>
    public class ObjectContent : ContentNode, IInitializingObjectNode
    {
        private readonly HybridDictionary<string, IMemberNode> childrenMap = new HybridDictionary<string, IMemberNode>();
        private readonly List<IMemberNode> children = new List<IMemberNode>();
        private object value;

        public ObjectContent(object value, Guid guid, ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
            : base(guid, descriptor, isPrimitive)
        {
            if (reference is ObjectReference)
                throw new ArgumentException($"An {nameof(ObjectContent)} cannot contain an {nameof(ObjectReference)}");
            this.value = value;
            ItemReferences = reference as ReferenceEnumerable;
        }

        /// <inheritdoc/>
        public IMemberNode this[string name] => childrenMap[name];

        /// <inheritdoc/>
        public IReadOnlyCollection<IMemberNode> Members => children;

        /// <inheritdoc/>
        public IEnumerable<Index> Indices => GetIndices();

        /// <inheritdoc/>
        public override bool IsReference => ItemReferences != null;

        /// <inheritdoc/>
        public ReferenceEnumerable ItemReferences { get; }

        /// <inheritdoc/>
        protected sealed override object Value => value;

        /// <inheritdoc/>
        [CanBeNull]
        public IMemberNode TryGetChild([NotNull] string name)
        {
            IMemberNode child;
            childrenMap.TryGetValue(name, out child);
            return child;
        }

        /// <inheritdoc/>
        public IObjectNode IndexedTarget(Index index)
        {
            if (index == Index.Empty) throw new ArgumentException(@"index cannot be Index.Empty when invoking this method.", nameof(index));
            if (ItemReferences == null) throw new InvalidOperationException(@"The node does not contain enumerable references.");
            return ItemReferences[index].TargetNode;
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

        private IEnumerable<Index> GetIndices()
        {
            var enumRef = ItemReferences;
            if (enumRef != null)
                return enumRef.Indices;

            return GetIndices(this);
        }

        public override string ToString()
        {
            return $"{{Node: Object {Type.Name} = [{Value}]}}";
        }

        /// <inheritdoc/>
        void IInitializingObjectNode.AddMember(IInitializingMemberNode member, bool allowIfReference)
        {
            if (isSealed)
                throw new InvalidOperationException("Unable to add a child to a GraphNode that has been sealed");

            // ReSharper disable once HeuristicUnreachableCode - this code is reachable only at the specific moment we call this method!
            if (ItemReferences != null && !allowIfReference)
                throw new InvalidOperationException("A GraphNode cannot have children when its content hold a reference.");

            children.Add(member);
            childrenMap.Add(member.Name, (MemberContent)member);
        }
    }
}
