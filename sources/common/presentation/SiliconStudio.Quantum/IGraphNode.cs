// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    public interface IObjectNode : IContentNode
    {
        /// <summary>
        /// Gets the collection of members of this node.
        /// </summary>
        IReadOnlyCollection<IMemberNode> Members { get; }
    }

    public interface IMemberNode : IContentNode
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

        /// <summary>
        /// Gets the member descriptor corresponding to this member node.
        /// </summary>
        [NotNull]
        IMemberDescriptor MemberDescriptor { get; }
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
        /// <param name="allowIfReference">if set to <c>false</c> throw an exception if <see cref="IContentNode.Reference"/> is not null.</param>
        void AddMember(IInitializingMemberNode member, bool allowIfReference = false);
    }

    public interface IInitializingMemberNode : IInitializingGraphNode, IMemberNode
    {
        /// <summary>
        /// Sets the <see cref="IObjectNode"/> containing this member node.
        /// </summary>
        /// <param name="parent">The parent node containing this member node.</param>
        /// <seealso cref="IMemberNode.Parent"/>
        void SetParent(IObjectNode parent);
    }
}
