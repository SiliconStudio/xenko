// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Quantum
{
    internal interface IInitializingObjectNode : IInitializingGraphNode, IObjectNode
    {
        /// <summary>
        /// Add a member to this node. This node and the member node must not have been sealed yet.
        /// </summary>
        /// <param name="member">The member to add to this node.</param>
        /// <param name="allowIfReference">if set to <c>false</c> throw an exception if <see cref="IMemberNode.TargetReference"/> or <see cref="IObjectNode.ItemReferences"/> is not null.</param>
        void AddMember([NotNull] IMemberNode member, bool allowIfReference = false);
    }
}
