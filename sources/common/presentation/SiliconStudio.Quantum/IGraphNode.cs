// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    public interface INotifyContentValueChange
    {
        /// <summary>
        /// Raised just before a change to this node occurs.
        /// </summary>
        event EventHandler<MemberNodeChangeEventArgs> Changing;

        /// <summary>
        /// Raised when a change to this node has occurred.
        /// </summary>
        event EventHandler<MemberNodeChangeEventArgs> Changed;

    }

    public interface INotifyItemChange
    {
        event EventHandler<ItemChangeEventArgs> ItemChanging;

        event EventHandler<ItemChangeEventArgs> ItemChanged;
    }

    public interface IObjectNode : IContentNode, INotifyItemChange
    {
        /// <summary>
        /// Gets the member corresponding to the given name.
        /// </summary>
        /// <param name="name">The name of the member to retrieve.</param>
        /// <returns>The member corresponding to the given name.</returns>
        /// <exception cref="KeyNotFoundException">This node has no member that matches the given name.</exception>
        IMemberNode this[string name] { get; }

        /// <summary>
        /// Gets the collection of members of this node.
        /// </summary>
        IReadOnlyCollection<IMemberNode> Members { get; }

        ReferenceEnumerable ItemReferences { get; }

        /// <summary>
        /// Gets all the indices in the value of this content, if it is a collection. Otherwise, this property returns null.
        /// </summary>
        IEnumerable<Index> Indices { get; }

        /// <summary>
        /// Gets the target of this node corresponding to the given index, if this node contains a sequence of references to some other nodes. 
        /// </summary>
        /// <exception cref="InvalidOperationException">The node does not contain a sequence of references to some other nodes.</exception>
        /// <exception cref="ArgumentException">The index is empty.</exception>
        /// <exception cref="KeyNotFoundException">The index does not exist.</exception>
        IObjectNode IndexedTarget(Index index);

        /// <summary>
        /// Attempts to retrieve the child node of this <see cref="IContentNode"/> that matches the given name.
        /// </summary>
        /// <param name="name">The name of the child to retrieve.</param>
        /// <returns>The child node that matches the given name, or <c>null</c> if no child matches.</returns>
        IMemberNode TryGetChild(string name);

        /// <summary>
        /// Updates the value of this content at the given index with the given value.
        /// </summary>
        /// <param name="newValue">The new value to set.</param>
        /// <param name="index">The index where to update the value.</param>
        void Update(object newValue, Index index);

        /// <summary>
        /// Adds a new item to this content, assuming the content is a collection.
        /// </summary>
        /// <param name="newItem">The new item to add to the collection.</param>
        void Add(object newItem);

        /// <summary>
        /// Adds a new item at the given index to this content, assuming the content is a collection.
        /// </summary>
        /// <param name="newItem">The new item to add to the collection.</param>
        /// <param name="itemIndex">The index at which the new item must be added.</param>
        void Add(object newItem, Index itemIndex);

        /// <summary>
        /// Removes an item from this content, assuming the content is a collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="itemIndex">The index from which the item must be removed.</param>
        void Remove(object item, Index itemIndex);
    }

    public interface IMemberNode : IContentNode, INotifyContentValueChange
    {
        /// <summary>
        /// Gets the member name.
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Gets the <see cref="IObjectNode"/> containing this member node.
        /// </summary>
        [NotNull]
        IObjectNode Parent { get; }

        ObjectReference TargetReference { get; }

        /// <summary>
        /// Gets the target of this node, if this node contains a reference to another node. 
        /// </summary>
        /// <exception cref="InvalidOperationException">The node does not contain a reference to another node.</exception>
        IObjectNode Target { get; }

        /// <summary>
        /// Gets the member descriptor corresponding to this member node.
        /// </summary>
        [NotNull]
        IMemberDescriptor MemberDescriptor { get; }

        /// <summary>
        /// Updates the value of this content with the given value.
        /// </summary>
        /// <param name="newValue">The new value to set.</param>
        void Update(object newValue);
    }

    public interface IInitializingGraphNode : IContentNode
    {
        /// <summary>
        /// Seal the node, indicating its construction is finished and that no more children or commands will be added.
        /// </summary>
        void Seal();

        /// <summary>
        /// Add a command to this node. The node must not have been sealed yet.
        /// </summary>
        /// <param name="command">The node command to add.</param>
        void AddCommand(INodeCommand command);
    }

    public interface IInitializingObjectNode : IInitializingGraphNode, IObjectNode
    {
        /// <summary>
        /// Add a member to this node. This node and the member node must not have been sealed yet.
        /// </summary>
        /// <param name="member">The member to add to this node.</param>
        /// <param name="allowIfReference">if set to <c>false</c> throw an exception if <see cref="IContentNode.TargetReference"/> or <see cref="IContentNode.ItemReferences"/> is not null.</param>
        void AddMember(IInitializingMemberNode member, bool allowIfReference = false);
    }

    public interface IInitializingMemberNode : IInitializingGraphNode, IMemberNode
    {
        event EventHandler<INodeChangeEventArgs> PrepareChange;
        event EventHandler<INodeChangeEventArgs> FinalizeChange;
    }
}
