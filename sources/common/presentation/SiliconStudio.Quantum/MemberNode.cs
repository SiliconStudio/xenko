// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using System.Reflection;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// An implementation of <see cref="IGraphNode"/> that gives access to a member of an object.
    /// </summary>
    public class MemberNode : GraphNodeBase, IMemberNode, IGraphNodeInternal
    {
        public MemberNode([NotNull] INodeBuilder nodeBuilder, Guid guid, [NotNull] IObjectNode parent, [NotNull] IMemberDescriptor memberDescriptor, bool isPrimitive, IReference reference)
            : base(nodeBuilder.SafeArgument(nameof(nodeBuilder)).NodeContainer, guid, nodeBuilder.TypeDescriptorFactory.Find(memberDescriptor.Type), isPrimitive)
        {
            if (nodeBuilder == null) throw new ArgumentNullException(nameof(nodeBuilder));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (memberDescriptor == null) throw new ArgumentNullException(nameof(memberDescriptor));
            Parent = parent;
            MemberDescriptor = memberDescriptor;
            Name = memberDescriptor.Name;
            TargetReference = reference as ObjectReference;
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IObjectNode Parent { get; }

        /// <summary>
        /// The <see cref="IMemberDescriptor"/> used to access the member of the container represented by this content.
        /// </summary>
        public IMemberDescriptor MemberDescriptor { get; protected set; }

        public override bool IsReference => TargetReference != null;

        /// <inheritdoc/>
        public ObjectReference TargetReference { get; }

        /// <inheritdoc/>
        public IObjectNode Target => TargetReference?.TargetNode;

        /// <inheritdoc/>
        public event EventHandler<INodeChangeEventArgs> PrepareChange;

        /// <inheritdoc/>
        public event EventHandler<INodeChangeEventArgs> FinalizeChange;

        /// <inheritdoc/>
        public event EventHandler<MemberNodeChangeEventArgs> Changing;

        /// <inheritdoc/>
        public event EventHandler<MemberNodeChangeEventArgs> Changed;

        /// <inheritdoc/>
        protected sealed override object Value { get { if (Parent.Retrieve() == null) throw new InvalidOperationException("Container's value is null"); return MemberDescriptor.Get(Parent.Retrieve()); } }

        /// <inheritdoc/>
        public void Update(object newValue)
        {
            Update(newValue, true);
        }

        /// <summary>
        /// Raises the <see cref="Changing"/> event with the given parameters.
        /// </summary>
        /// <param name="args">The arguments of the event.</param>
        protected void NotifyContentChanging(MemberNodeChangeEventArgs args)
        {
            PrepareChange?.Invoke(this, args);
            Changing?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Changed"/> event with the given arguments.
        /// </summary>
        /// <param name="args">The arguments of the event.</param>
        protected void NotifyContentChanged(MemberNodeChangeEventArgs args)
        {
            Changed?.Invoke(this, args);
            FinalizeChange?.Invoke(this, args);
        }

        protected internal override void UpdateFromMember(object newValue, Index index)
        {
            if (index != Index.Empty) throw new ArgumentException(@"index must be Index.Empty.", nameof(Index));
            Update(newValue, false);
        }

        private void Update(object newValue, bool sendNotification)
        {
            var oldValue = Retrieve();
            MemberNodeChangeEventArgs args = null;
            if (sendNotification)
            {
                args = new MemberNodeChangeEventArgs(this, oldValue, newValue);
                NotifyContentChanging(args);
            }
            if (Parent.Retrieve() == null)
                throw new InvalidOperationException("Container's value is null");
            var containerValue = Parent.Retrieve();
            MemberDescriptor.Set(containerValue, newValue);

            if (Parent.Retrieve().GetType().GetTypeInfo().IsValueType)
                ((GraphNodeBase)Parent).UpdateFromMember(containerValue, Index.Empty);

            UpdateReferences();
            if (sendNotification)
            {
                NotifyContentChanged(args);
            }
        }

        private void UpdateReferences()
        {
            NodeContainer?.UpdateReferences(this);
        }

        public override string ToString()
        {
            return $"{{Node: Member {Name} = [{Value}]}}";
        }
    }
}
