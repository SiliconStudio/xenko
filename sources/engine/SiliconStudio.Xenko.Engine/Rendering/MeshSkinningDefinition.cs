// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Describes skinning for a <see cref="Mesh"/>, through a collection of <see cref="MeshBoneDefinition"/>.
    /// </summary>
    [DataContract]
    public class MeshSkinningDefinition
    {
        /// <summary>
        /// The bones.
        /// </summary>
        public MeshBoneDefinition[] Bones;
    }
}
