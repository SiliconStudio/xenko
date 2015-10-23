// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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