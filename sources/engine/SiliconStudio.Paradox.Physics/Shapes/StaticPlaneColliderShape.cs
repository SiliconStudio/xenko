// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Physics
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

            Normal = normal;
            Offset = offset;

            InternalShape = new BulletSharp.StaticPlaneShape(normal, offset);
        }

        /// <summary>
        /// Gets the normal.
        /// </summary>
        /// <value>
        /// The normal.
        /// </value>
        public Vector3 Normal { get; private set; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public float Offset { get; private set; }
    }
}
