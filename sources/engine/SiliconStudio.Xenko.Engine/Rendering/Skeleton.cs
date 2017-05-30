// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Describes hiderarchical nodes in a flattened array.
    /// </summary>
    /// <remarks>
    /// Nodes are ordered so that parents always come first, allowing for hierarchical updates in a simple loop.
    /// </remarks>
    [ReferenceSerializer, DataSerializerGlobal(typeof(ReferenceSerializer<Skeleton>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializer<Skeleton>))]
    [DataContract]
    public class Skeleton
    {
        /// <summary>
        /// The nodes in this hierarchy.
        /// </summary>
        public ModelNodeDefinition[] Nodes;
    }
}
