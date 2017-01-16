// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// A base abstract implementation of the <see cref="IContentNode"/> interface.
    /// </summary>
    public abstract class ContentBase : IContentNode
    {
        private readonly List<MemberContent> children = new List<MemberContent>();
        private readonly HybridDictionary<string, MemberContent> childrenMap = new HybridDictionary<string, MemberContent>();
        private readonly List<INodeCommand> commands = new List<INodeCommand>();
        private bool isSealed;

        protected ContentBase(string name, Guid guid, ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (guid == Guid.Empty) throw new ArgumentException(@"The guid must be different from Guid.Empty.", nameof(guid));
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            Name = name;
            Guid = guid;
            Reference = reference;
            Descriptor = descriptor;
            IsPrimitive = isPrimitive;
        }

        /// <inheritdoc/>
        public Type Type => Descriptor.Type;

        /// <inheritdoc/>
        public abstract object Value { get; }

        /// <inheritdoc/>
        public bool IsPrimitive { get; }

        /// <inheritdoc/>
        public ITypeDescriptor Descriptor { get; }

        /// <inheritdoc/>
        public bool IsReference => Reference != null;

        /// <inheritdoc/>
        public IReference Reference { get; }

        /// <inheritdoc/>
        public IEnumerable<Index> Indices => GetIndices();

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public Guid Guid { get; }

        /// <inheritdoc/>
        public IContentNode Content => this;

        /// <inheritdoc/>
        public IContentNode Parent { get; private set; }

        /// <inheritdoc/>
        public IContentNode Target { get { if (!(Content.Reference is ObjectReference)) throw new InvalidOperationException("This node does not contain an ObjectReference"); return Content.Reference.AsObject.TargetNode; } }

        /// <inheritdoc/>
        public IReadOnlyCollection<MemberContent> Children => children;

        /// <inheritdoc/>
        public IReadOnlyCollection<INodeCommand> Commands => commands;

        /// <inheritdoc/>
        public MemberContent this[string name] => childrenMap[name];

        /// <inheritdoc/>
        public event EventHandler<ContentChangeEventArgs> PrepareChange;

        /// <inheritdoc/>
        public event EventHandler<ContentChangeEventArgs> FinalizeChange;

        /// <inheritdoc/>
        public event EventHandler<ContentChangeEventArgs> Changing;

        /// <inheritdoc/>
        public event EventHandler<ContentChangeEventArgs> Changed;

        /// <inheritdoc/>
        public virtual object Retrieve()
        {
            return SiliconStudio.Quantum.Contents.Content.Retrieve(Value, Index.Empty, Descriptor);
        }

        /// <inheritdoc/>
        public virtual object Retrieve(Index index)
        {
            return SiliconStudio.Quantum.Contents.Content.Retrieve(Value, index, Descriptor);
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

        /// <inheritdoc/>
        public override string ToString()
        {
            string type = null;
            if (Content is MemberContent)
                type = "Member";
            else if (Content is ObjectContent)
                type = "Object";
            else if (Content is BoxedContent)
                type = "Boxed";

            return $"{{Node: {type} {Name} = [{Content.Value}]}}";
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

        /// <summary>
        /// Raises the <see cref="Changing"/> event with the given parameters.
        /// </summary>
        /// <param name="args">The arguments of the event.</param>
        protected void NotifyContentChanging(ContentChangeEventArgs args)
        {
            PrepareChange?.Invoke(this, args);
            Changing?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Changed"/> event with the given arguments.
        /// </summary>
        /// <param name="args">The arguments of the event.</param>
        protected void NotifyContentChanged(ContentChangeEventArgs args)
        {
            Changed?.Invoke(this, args);
            FinalizeChange?.Invoke(this, args);
        }

        private IEnumerable<Index> GetIndices()
        {
            var enumRef = Reference as ReferenceEnumerable;
            if (enumRef != null)
                return enumRef.Indices;

            var collectionDescriptor = Descriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
            {
                return Enumerable.Range(0, collectionDescriptor.GetCollectionCount(Value)).Select(x => new Index(x));
            }
            var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
            return dictionaryDescriptor?.GetKeys(Value).Cast<object>().Select(x => new Index(x));
        }

        /// <inheritdoc/>
        public IContentNode IndexedTarget(Index index)
        {
            if (index == Index.Empty) throw new ArgumentException(@"index cannot be Index.Empty when invoking this method.", nameof(index));
            if (!(Content.Reference is ReferenceEnumerable)) throw new InvalidOperationException(@"The node does not contain enumerable references.");
            return Content.Reference.AsEnumerable[index].TargetNode;
        }

        /// <summary>
        /// Add a child to this node. The node must not have been sealed yet.
        /// </summary>
        /// <param name="child">The child node to add.</param>
        /// <param name="allowIfReference">if set to <c>false</c> throw an exception if <see cref="IContentNode.Reference"/> is not null.</param>
        public void AddChild(MemberContent child, bool allowIfReference = false)
        {
            if (isSealed)
                throw new InvalidOperationException("Unable to add a child to a GraphNode that has been sealed");

            if (child.Parent != null)
                throw new ArgumentException(@"This node has already been registered to a different parent", nameof(child));

            if (Content.Reference != null && !allowIfReference)
                throw new InvalidOperationException("A GraphNode cannot have children when its content hold a reference.");

            child.Parent = this;
            children.Add(child);
            childrenMap.Add(child.Name, child);
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

        /// <inheritdoc/>
        public MemberContent TryGetChild(string name)
        {
            MemberContent child;
            childrenMap.TryGetValue(name, out child);
            return child;
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
