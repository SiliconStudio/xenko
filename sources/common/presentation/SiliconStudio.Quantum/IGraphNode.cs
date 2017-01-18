// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    public interface IObjectNode : IContentNode
    {

    }

    public interface IMemberNode : IContentNode
    {

    }

    public interface IInitializingObjectNode : IObjectNode
    {
        /// <summary>
        /// Add a child to this node. The node must not have been sealed yet.
        /// </summary>
        /// <param name="child">The child node to add.</param>
        /// <param name="allowIfReference">if set to <c>false</c> throw an exception if <see cref="IContentNode.Reference"/> is not null.</param>
        void AddMember(MemberContent child, bool allowIfReference = false);
    }
}
