// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<StaticPlaneColliderShapeDesc>))]
    [DataContract("StaticPlaneColliderShapeDesc")]
    [Display(50, "Infinite Plane")]
    public class StaticPlaneColliderShapeDesc : IInlineColliderShapeDesc
    {
        /// <userdoc>
        /// The normal of the infinite plane.
        /// </userdoc>
        [DataMember(10)]
        public Vector3 Normal = Vector3.UnitY;

        /// <userdoc>
        /// The distance offset.
        /// </userdoc>
        [DataMember(20)]
        public float Offset;

        public int CompareTo(object obj)
        {
            var other = obj as StaticPlaneColliderShapeDesc;
            if (other == null) return -1;
            if (other.Normal == Normal && Math.Abs(other.Offset - Offset) < float.Epsilon) return 0;
            return 1;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Normal.GetHashCode()*397) ^ Offset.GetHashCode();
            }
        }
    }
}