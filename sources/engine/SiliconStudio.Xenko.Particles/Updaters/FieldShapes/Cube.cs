// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;

namespace SiliconStudio.Xenko.Particles.Updaters.FieldShapes
{
    [DataContract("FieldShapeCube")]
    public class Cube : FieldShape
    {
        public override DebugDrawShape GetDebugDrawShape(out Vector3 pos, out Quaternion rot, out Vector3 scl)
        {
            pos = new Vector3(0, 0, 0);
            rot = new Quaternion(0, 0, 0, 1);
            scl = new Vector3(halfSideX * 2, halfSideY * 2, halfSideZ * 2);
            return DebugDrawShape.Cube;
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
        /// The maximum distance from the origin along the X axis. The X side is twice as big.
        /// </summary>
        /// <userdoc>
        /// The maximum distance from the origin along the X axis. The X side is twice as big.
        /// </userdoc>
        [DataMember(10)]
        [Display("Half X")]
        public float HalfSideX { get {return halfSideX; } set { halfSideX = (value > MathUtil.ZeroTolerance) ? value : MathUtil.ZeroTolerance; } }
        private float halfSideX = 1f;

        /// <summary>
        /// The maximum distance from the origin along the Y axis. The Y side is twice as big.
        /// </summary>
        /// <userdoc>
        /// The maximum distance from the origin along the Y axis. The Y side is twice as big.
        /// </userdoc>
        [DataMember(20)]
        [Display("Half Y")]
        public float HalfSideY { get { return halfSideY; } set { halfSideY = (value > MathUtil.ZeroTolerance) ? value : MathUtil.ZeroTolerance; } }
        private float halfSideY = 1f;

        /// <summary>
        /// The maximum distance from the origin along the Z axis. The Z side is twice as big.
        /// </summary>
        /// <userdoc>
        /// The maximum distance from the origin along the Z axis. The Z side is twice as big.
        /// </userdoc>
        [DataMember(30)]
        [Display("Half Z")]
        public float HalfSideZ { get { return halfSideZ; } set { halfSideZ = (value > MathUtil.ZeroTolerance) ? value : MathUtil.ZeroTolerance; } }
        private float halfSideZ = 1f;

        public override void PreUpdateField(Vector3 position, Quaternion rotation, Vector3 size)
        {
            this.fieldSize = size;
            this.fieldPosition = position;
            this.fieldRotation = rotation;
            inverseRotation = new Quaternion(-rotation.X, -rotation.Y, -rotation.Z, rotation.W);

            mainAxis = new Vector3(0, 1, 0);
            rotation.Rotate(ref mainAxis);
        }

        public override float GetDistanceToCenter(
            Vector3 particlePosition, Vector3 particleVelocity,
            out Vector3 alongAxis, out Vector3 aroundAxis, out Vector3 awayAxis)
        {
            // Along - following the main axis
            alongAxis = mainAxis;

            // Toward - tawards the main axis
            awayAxis = particlePosition - fieldPosition;
            awayAxis.Normalize();

            // Around - around the main axis, following the right hand rule
            aroundAxis = Vector3.Cross(alongAxis, awayAxis);

            particlePosition -= fieldPosition;
            var inverseRotation = new Quaternion(-fieldRotation.X, -fieldRotation.Y, -fieldRotation.Z, fieldRotation.W);
            inverseRotation.Rotate(ref particlePosition);
            particlePosition /= fieldSize;

            // Start of code for Cube
            var maxDist = Math.Max(Math.Abs(particlePosition.X) / halfSideX, Math.Abs(particlePosition.Y) / halfSideY);
            maxDist = Math.Max(maxDist, Math.Abs(particlePosition.Z) / halfSideZ);
            // End of code for Cube

            return maxDist;
        }

        public override bool IsPointInside(Vector3 particlePosition, out Vector3 surfacePoint, out Vector3 surfaceNormal)
        {
            particlePosition -= fieldPosition;
            inverseRotation.Rotate(ref particlePosition);
            particlePosition /= fieldSize;

            var maxX = Math.Abs(particlePosition.X);
            var maxY = Math.Abs(particlePosition.Y);
            var maxZ = Math.Abs(particlePosition.Z);

            bool isInside;

            if ((maxX / halfSideX >= maxY / halfSideY) && (maxX / halfSideX >= maxZ / halfSideZ))
            {
                // Biggest distance is on the X axis
                surfacePoint = particlePosition * (halfSideX / maxX);
                surfaceNormal = new Vector3(surfacePoint.X, 0, 0);
                isInside = (maxX <= halfSideX);
            }

            else
            if (maxY / halfSideY >= maxZ / halfSideZ)
            {
                // Biggest distance is on the Y axis
                surfacePoint = particlePosition * (halfSideY / maxY);
                surfaceNormal = new Vector3(0, surfacePoint.Y, 0);
                isInside = (maxY <= halfSideY);
            }

            else
            {
                // Biggest distance is on the Z axis
                surfacePoint = particlePosition * (halfSideZ / maxZ);
                surfaceNormal = new Vector3(0, 0, surfacePoint.Z);
                isInside = (maxZ <= halfSideZ);
            }

            // Fix the surface point and normal to world space
            fieldRotation.Rotate(ref surfaceNormal);
            surfaceNormal *= fieldSize;
            surfaceNormal.Normalize();

            fieldRotation.Rotate(ref surfacePoint);
            surfacePoint *= fieldSize;
            surfacePoint += fieldPosition;

            return isInside;
        }

    }
}
