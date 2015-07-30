// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<Box2DColliderShapeDesc>))]
    [DataContract("Box2DColliderShapeDesc")]
    [Display(50, "Box2DColliderShape")]
    public class Box2DColliderShapeDesc : IColliderShapeDesc
    {
        /// <userdoc>
        /// The size of one edge of the box.
        /// </userdoc>
        [DataMember(10)]
        public Vector2 Size = Vector2.One;

        /// <userdoc>
        /// The offset with the real graphic mesh.
        /// </userdoc>
        [DataMember(20)]
        public Vector3 LocalOffset;

        /// <userdoc>
        /// The local rotation of the collider shape.
        /// </userdoc>
        [DataMember(30)] 
        public Quaternion LocalRotation = Quaternion.Identity;
    }
}