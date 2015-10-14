// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Rendering
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