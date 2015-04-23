// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Rendering
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