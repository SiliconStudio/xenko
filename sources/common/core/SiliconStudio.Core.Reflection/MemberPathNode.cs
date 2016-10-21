// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Represent a node in a <see cref="MemberPath"/>.
    /// </summary>
    public struct MemberPathNode
    {
        /// <summary>
        /// The node object.
        /// </summary>
        public object Object;

        /// <summary>
        /// The descriptor to the next node.
        /// </summary>
        public IMemberDescriptor Descriptor;
    }
}