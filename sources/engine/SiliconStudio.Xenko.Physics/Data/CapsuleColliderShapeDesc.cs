// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using System.ComponentModel;

namespace SiliconStudio.Xenko.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<CapsuleColliderShapeDesc>))]
    [DataContract("CapsuleColliderShapeDesc")]
    [Display(50, "Capsule")]
    public class CapsuleColliderShapeDesc : IInlineColliderShapeDesc
    {
        /// <userdoc>
        /// Select this if this shape will represent a 2D shape
        /// </userdoc>
        [DataMember(10)]
        [DefaultValue(false)]
        public bool Is2D;

        /// <userdoc>
        /// The length of the capsule (distance between the center of the two sphere centers).
        /// </userdoc>
        [DataMember(20)]
        [DefaultValue(0.5f)]
        public float Length = 0.5f;

        /// <userdoc>
        /// The radius of the capsule.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(0.25f)]
        public float Radius = 0.25f;

        /// <userdoc>
        /// The orientation of the capsule.
        /// </userdoc>
        [DataMember(40)]
        [DefaultValue(ShapeOrientation.UpY)]
        public ShapeOrientation Orientation = ShapeOrientation.UpY;

        /// <userdoc>
        /// The offset with the real graphic mesh.
        /// </userdoc>
        [DataMember(50)]
        public Vector3 LocalOffset;

        /// <userdoc>
        /// The local rotation of the collider shape.
        /// </userdoc>
        [DataMember(60)]
        public Quaternion LocalRotation = Quaternion.Identity;

        public int CompareTo(object obj)
        {
            var other = obj as CapsuleColliderShapeDesc;
            if (other == null) return -1;

            if (other.Is2D == Is2D &&
                Math.Abs(other.Length - Length) < float.Epsilon &&
                Math.Abs(other.Radius - Radius) < float.Epsilon &&
                other.Orientation == Orientation &&
                other.LocalOffset == LocalOffset &&
                other.LocalRotation == LocalRotation) return 0;

            return 1;
        }
    }
}