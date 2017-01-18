// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    public interface IObjectNode : IContentNode
    {

    }

    public interface IMemberNode : IContentNode
    {
       /// <summary>
       /// Gets the <see cref="IObjectNode"/> containing this member node.
       /// </summary>
       [NotNull]
       IObjectNode Parent { get; }

        /// <summary>
        /// Gets the member descriptor corresponding to this member node.
        /// </summary>
        IMemberDescriptor MemberDescriptor { get; }
    }

    public interface IInitializingObjectNode : IObjectNode
    {
        /// <summary>
        /// Add a member to this node. This node and the member node must not have been sealed yet.
        /// </summary>
        /// <param name="member">The member to add to this node.</param>
        /// <param name="allowIfReference">if set to <c>false</c> throw an exception if <see cref="IContentNode.Reference"/> is not null.</param>
        void AddMember(IInitializingMemberNode member, bool allowIfReference = false);
    }

    public interface IInitializingMemberNode : IMemberNode
    {
        /// <summary>
        /// Sets the <see cref="IObjectNode"/> containing this member node.
        /// </summary>
        /// <param name="parent">The parent node containing this member node.</param>
        /// <seealso cref="IMemberNode.Parent"/>
        void SetParent(IObjectNode parent);
    }
}
