// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Editor
{
    /// <summary>
    /// Result of a the <see cref="PickingRenderFeature"/>
    /// </summary>
    public struct EntityPickingResult
    {
        /// <summary>
        /// The entity picked. May be null if not found.
        /// </summary>
        public Entity Entity;

        /// <summary>
        /// The component identifier
        /// </summary>
        public int ComponentId;

        /// <summary>
        /// The mesh node index
        /// </summary>
        public int MeshNodeIndex;

        /// <summary>
        /// The material index
        /// </summary>
        public int MaterialIndex;

        public override string ToString()
        {
            return $"ComponentId: {ComponentId}, MeshNodeIndex: {MeshNodeIndex}, MaterialIndex: {MaterialIndex}";
        }
    }
}
