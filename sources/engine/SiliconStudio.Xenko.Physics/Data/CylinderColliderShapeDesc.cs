// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using System.ComponentModel;

namespace SiliconStudio.Xenko.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<CylinderColliderShapeDesc>))]
    [DataContract("CylinderColliderShapeDesc")]
    [Display(50, "Cylinder")]
    public class CylinderColliderShapeDesc : IInlineColliderShapeDesc
    {
        /// <userdoc>
        /// The height of the cylinder
        /// </userdoc>
        [DataMember(10)]
        [DefaultValue(1f)]
        public float Height = 1f;

        /// <userdoc>
        /// The radius of the cylinder
        /// </userdoc>
        [DataMember(20)]
        [DefaultValue(0.5f)]
        public float Radius = 0.5f;

        /// <userdoc>
        /// The orientation of the cylinder.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(ShapeOrientation.UpY)]
        public ShapeOrientation Orientation = ShapeOrientation.UpY;

        /// <userdoc>
        /// The offset with the real graphic mesh.
        /// </userdoc>
        [DataMember(40)]
        public Vector3 LocalOffset;

        /// <userdoc>
        /// The local rotation of the collider shape.
        /// </userdoc>
        [DataMember(50)]
        public Quaternion LocalRotation = Quaternion.Identity;

        public int CompareTo(object obj)
        {
            var other = obj as CylinderColliderShapeDesc;
            if (other == null) return -1;

            if (Math.Abs(other.Height - Height) < float.Epsilon &&
                Math.Abs(other.Radius - Radius) < float.Epsilon &&
                other.Orientation == Orientation &&
                other.LocalOffset == LocalOffset &&
                other.LocalRotation == LocalRotation) return 0;

            return 1;
        }
    }
}