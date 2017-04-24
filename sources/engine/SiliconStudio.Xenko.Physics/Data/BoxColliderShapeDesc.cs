// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<BoxColliderShapeDesc>))]
    [DataContract("BoxColliderShapeDesc")]
    [Display(50, "Box")]
    public class BoxColliderShapeDesc : IInlineColliderShapeDesc
    {
        /// <userdoc>
        /// Select this if this shape will represent a Circle 2D shape
        /// </userdoc>
        [DataMember(5)]
        public bool Is2D;

        /// <userdoc>
        /// The size of one edge of the box.
        /// </userdoc>
        [DataMember(10)]
        public Vector3 Size = Vector3.One;

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

        public bool Match(object obj)
        {
            var other = obj as BoxColliderShapeDesc;
            return other?.Is2D == Is2D && other.Size == Size && other.LocalOffset == LocalOffset && other.LocalRotation == LocalRotation;
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Is2D.GetHashCode();
                hashCode = (hashCode*397) ^ Size.GetHashCode();
                hashCode = (hashCode*397) ^ LocalOffset.GetHashCode();
                hashCode = (hashCode*397) ^ LocalRotation.GetHashCode();
                return hashCode;
            }
        }
    }
}
