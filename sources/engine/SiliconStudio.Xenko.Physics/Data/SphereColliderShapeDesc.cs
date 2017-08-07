// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using System.ComponentModel;

namespace SiliconStudio.Xenko.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<SphereColliderShapeDesc>))]
    [DataContract("SphereColliderShapeDesc")]
    [Display(50, "Sphere")]
    public class SphereColliderShapeDesc : IInlineColliderShapeDesc
    {
        /// <userdoc>
        /// Select this if this shape will represent a Circle 2D shape
        /// </userdoc>
        [DataMember(10)]
        public bool Is2D;

        /// <userdoc>
        /// The radius of the sphere/circle.
        /// </userdoc>
        [DataMember(20)]
        [DefaultValue(0.5f)]
        public float Radius = 0.5f;

        /// <userdoc>
        /// The offset with the real graphic mesh.
        /// </userdoc>
        [DataMember(30)]
        public Vector3 LocalOffset;

        public bool Match(object obj)
        {
            var other = obj as SphereColliderShapeDesc;
            return other?.Is2D == Is2D && Math.Abs(other.Radius - Radius) < float.Epsilon && other.LocalOffset == LocalOffset;
        }
    }
}
