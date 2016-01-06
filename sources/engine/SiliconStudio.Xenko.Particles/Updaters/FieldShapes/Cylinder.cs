// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;

namespace SiliconStudio.Xenko.Particles.Updaters.FieldShapes
{
    [DataContract("FieldShapeCylinder")]
    public class Cylinder : FieldShape
    {
        public override DebugDrawShape GetDebugDrawShape(out Vector3 pos, out Quaternion rot, out Vector3 scl)
        {
            pos = new Vector3(0, 0, 0);
            rot = new Quaternion(0, 0, 0, 1);
            scl = new Vector3(radius * 2, halfHeight * 2, radius * 2);
            return DebugDrawShape.Cylinder;
        }

        [DataMemberIgnore]
        private Vector3 fieldPosition;

        [DataMemberIgnore]
        private Quaternion fieldRotation;

        [DataMemberIgnore]
        private Quaternion inverseRotation;

        [DataMemberIgnore]
        private Vector3 fieldSize;

        [DataMemberIgnore]
        private Vector3 mainAxis;


        /// <summary>
        /// The maximum distance from the origin along the Y axis. The height is twice as big.
        /// </summary>
        /// <userdoc>
        /// The maximum distance from the origin along the Y axis. The height is twice as big.
        /// </userdoc>
        [DataMember(10)]
        [Display("Half height")]
        public float HalfHeight { get { return halfHeight; } set { halfHeight = (value > MathUtil.ZeroTolerance) ? value : MathUtil.ZeroTolerance; } }
        private float halfHeight = 1f;

        /// <summary>
        /// The maximum distance from the central axis.
        /// </summary>
        /// <userdoc>
        /// The maximum distance from the central axis.
        /// </userdoc>
        [DataMember(20)]
        [Display("Radius")]
        public float Radius { get { return radius; } set { radius = (value > MathUtil.ZeroTolerance) ? value : MathUtil.ZeroTolerance; } }
        private float radius = 1f;


        public override void PreUpdateField(Vector3 fieldPosition, Quaternion fieldRotation, Vector3 fieldSize)
        {
            this.fieldSize = fieldSize;
            this.fieldPosition = fieldPosition;
            this.fieldRotation = fieldRotation;
            inverseRotation = new Quaternion(-fieldRotation.X, -fieldRotation.Y, -fieldRotation.Z, fieldRotation.W);

            mainAxis = new Vector3(0, 1, 0);
            fieldRotation.Rotate(ref mainAxis);
        }


        public override float GetDistanceToCenter(
                Vector3 particlePosition, Vector3 particleVelocity,
                out Vector3 alongAxis, out Vector3 aroundAxis, out Vector3 awayAxis)
        {
            // Along - following the main axis
            alongAxis = mainAxis;

            // Toward - tawards the main axis
            awayAxis = particlePosition - fieldPosition;
            awayAxis.Y = 0; // In case of cylinder the away vector should be flat (away from the axis rather than just a point)
            awayAxis.Normalize();

            // Around - around the main axis, following the right hand rule
            aroundAxis = Vector3.Cross(alongAxis, awayAxis);

            particlePosition -= fieldPosition;
            var inverseRotation = new Quaternion(-fieldRotation.X, -fieldRotation.Y, -fieldRotation.Z, fieldRotation.W);
            inverseRotation.Rotate(ref particlePosition);
            particlePosition /= fieldSize;

            // Start of code for Cylinder
            if (Math.Abs(particlePosition.Y) >= halfHeight)
                return 1;

            particlePosition.Y = 0;

            particlePosition.X /= radius;
            particlePosition.Z /= radius;

            var maxDist = particlePosition.Length();
            // End of code for Cylinder

            return maxDist;
        }
    }
}
