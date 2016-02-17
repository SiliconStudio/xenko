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


        public override void PreUpdateField(Vector3 position, Quaternion rotation, Vector3 size)
        {
            fieldSize = size;
            fieldPosition = position;
            fieldRotation = rotation;
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
            awayAxis.Y = 0; // In case of cylinder the away vector should be flat (away from the axis rather than just a point)
            awayAxis.Normalize();

            // Around - around the main axis, following the right hand rule
            aroundAxis = Vector3.Cross(alongAxis, awayAxis);

            particlePosition -= fieldPosition;
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

        public override bool IsPointInside(Vector3 particlePosition, out Vector3 surfacePoint, out Vector3 surfaceNormal)
        {
            particlePosition -= fieldPosition;
            inverseRotation.Rotate(ref particlePosition);
            particlePosition /= fieldSize;

            var maxDist = (float)Math.Sqrt(particlePosition.X * particlePosition.X + particlePosition.Z * particlePosition.Z);
            var maxHeight = (float)Math.Abs(particlePosition.Y);

            if (maxHeight / halfHeight >= maxDist / radius)
            {
                // Closest surface point will hit the flat surfaces before the curved one
                surfacePoint = particlePosition * (halfHeight / maxHeight);
                surfaceNormal = new Vector3(0, surfacePoint.Y, 0);
            }
            else
            {
                // Closest surface point will hit the curved surface before the flat ones
                surfacePoint = particlePosition * (radius / maxDist);
                surfaceNormal = surfacePoint;
                surfaceNormal.Y = 0;
            }

            // Fix the surface point and normal to world space
            fieldRotation.Rotate(ref surfaceNormal);
            surfaceNormal *= fieldSize;
            surfaceNormal.Normalize();

            fieldRotation.Rotate(ref surfacePoint);
            surfacePoint *= fieldSize;
            surfacePoint += fieldPosition;


            // Start of code for Cylinder
            if (Math.Abs(particlePosition.Y) > halfHeight)
            {
                return false;
            }

            // The point is within -1 and +1 XZ-surafces - might collide with the curved surface of the cylinder

            // If the points lies on the central axis, it is inside the cylinder
            if (maxDist <= MathUtil.ZeroTolerance)
            {
                return true;
            }

            return (maxDist <= radius);
        }
    }
}
