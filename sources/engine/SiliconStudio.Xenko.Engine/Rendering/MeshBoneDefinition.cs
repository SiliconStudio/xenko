// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Describes a bone cluster inside a <see cref="Mesh"/>.
    /// </summary>
    [DataContract]
    public struct MeshBoneDefinition
    {
        /// <summary>
        /// The node index in <see cref="SkeletonUpdater.NodeTransformations"/>.
        /// </summary>
        public int NodeIndex;
        
        /// <summary>
        /// The matrix to transform from mesh space to local space of this bone.
        /// </summary>
        public Matrix LinkToMeshMatrix;
    }
}
