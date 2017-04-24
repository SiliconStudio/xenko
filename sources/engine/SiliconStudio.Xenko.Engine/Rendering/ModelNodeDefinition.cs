// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Describes a single transformation node, usually in a <see cref="Model"/> node hierarchy.
    /// </summary>
    [DataContract]
    public struct ModelNodeDefinition
    {
        /// <summary>
        /// The parent node index.
        /// </summary>
        public int ParentIndex;

        /// <summary>
        /// The local transform.
        /// </summary>
        public TransformTRS Transform;

        /// <summary>
        /// The name of this node.
        /// </summary>
        public string Name;

        /// <summary>
        /// The flags of this node.
        /// </summary>
        public ModelNodeFlags Flags;

        public override string ToString()
        {
            return string.Format("Parent: {0} Name: {1}", ParentIndex, Name);
        }
    }
}
