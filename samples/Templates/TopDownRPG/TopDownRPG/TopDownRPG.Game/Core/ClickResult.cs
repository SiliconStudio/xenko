using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;

namespace TopDownRPG.Core
{
    public enum ClickType
    {
        /// <summary>
        /// The result didn't hit anything
        /// </summary>
        Empty,

        /// <summary>
        /// The result hit a ground object
        /// </summary>
        Ground,

        /// <summary>
        /// The result hit a treasure chest object
        /// </summary>
        LootCrate,
    }

    /// <summary>
    /// Result of the user clicking/tapping on the world
    /// </summary>
    public struct ClickResult
    {
        /// <summary>
        /// The world-space position of the click, where the raycast hits the collision body
        /// </summary>
        public Vector3      WorldPosition;

        /// <summary>
        /// The Entity containing the collision body which was hit
        /// </summary>
        public Entity       ClickedEntity;

        /// <summary>
        /// What kind of object did we hit
        /// </summary>
        public ClickType    Type;

        public HitResult    HitResult;
    }
}
