using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// Data associated with static colliders for incremental building of navigation meshes
    /// </summary>
    public class StaticColliderData
    {
        public StaticColliderComponent Component;
        internal int ParameterHash = 0;
        internal bool Processed = false;
        internal NavigationMeshInputBuilder InputBuilder;
        internal NavigationMeshCachedObject Previous;

        /// <remarks>
        /// Planes are an exceptions to normal geometry since their size depends on the size of the bounding boxes in the scene, however we don't want to rebuild the whole scene, unless the actual shape of the plane changes
        /// </remarks>
        internal readonly List<Plane> Planes = new List<Plane>();
    }
}