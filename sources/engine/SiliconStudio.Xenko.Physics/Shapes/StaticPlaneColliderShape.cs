// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Physics
{
    public class StaticPlaneColliderShape : ColliderShape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticPlaneColliderShape"/> class.
        /// A static plane that is solid to infinity on one side.
        /// Several of these can be used to confine a convex space in a manner that completely prevents tunneling to the outside.
        /// The plane itself is specified with a normal and distance as is standard in mathematics.
        /// </summary>
        /// <param name="normal">The normal.</param>
        /// <param name="offset">The offset.</param>
        public StaticPlaneColliderShape(Vector3 normal, float offset)
        {
            Type = ColliderShapeTypes.StaticPlane;
            Is2D = false;

            CachedScaling = Vector3.One;

            InternalShape = new BulletSharp.StaticPlaneShape(normal, offset)
            {
                LocalScaling = CachedScaling
            };
        }
    }
}
