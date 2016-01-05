using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;

namespace SiliconStudio.Xenko.Particles.Updaters.FieldShapes
{
    [DataContract("FieldShapeTorus")]
    public class Torus : FieldShape
    {
        public override DebugDrawShape GetDebugDrawShape(out Vector3 pos, out Quaternion rot, out Vector3 scl)
        {
            pos = new Vector3(0, 0, 0);
            rot = new Quaternion(0, 0, 0, 1);
            scl = new Vector3(2 * BigRadius, 2 * BigRadius, 2 * BigRadius); // The default torus for drawing has a small radius of 0.5f
            return DebugDrawShape.Torus;
        }

        /// <summary>
        /// Big radius of the torus
        /// </summary>
        /// <userdoc>
        /// Big radius of the torus (defines the circle around which the torus is positioned)
        /// </userdoc>
        [DataMember(10)]
        [Display("Big radius")]
        public float BigRadius { get; set; } = 1f;

        [DataMemberIgnore]
        private float smallRadius { get; set; } = 0.33333f;

        [DataMemberIgnore]
        private float smallRadiusSquared = 0.11111f;

        /// <summary>
        /// Small radius of the torus, given as a relative size to the big radius
        /// </summary>
        /// <userdoc>
        /// Small radius of the torus, given as a relative to the big radius (percentage between 0 and 1)
        /// </userdoc>
        [DataMember(20)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Small radius")]
        public float SmallRadius
        {
            get { return smallRadius; }
            set { smallRadius = value; smallRadiusSquared = value * value; }
        }

        [DataMemberIgnore]
        private Vector3 fieldPosition;

        [DataMemberIgnore]
        private Quaternion fieldRotation;

        [DataMemberIgnore]
        private Quaternion inverseRotation;

        [DataMemberIgnore]
        private Vector3 fieldSize;

        public override void PreUpdateField(Vector3 fieldPosition, Quaternion fieldRotation, Vector3 fieldSize)
        {
            this.fieldSize = fieldSize * BigRadius;
            this.fieldPosition = fieldPosition;
            this.fieldRotation = fieldRotation;
            inverseRotation = new Quaternion(-fieldRotation.X, -fieldRotation.Y, -fieldRotation.Z, fieldRotation.W);
        }

        public override float GetFieldStrength(
            Vector3 particlePosition, Vector3 particleVelocity,
            out Vector3 alongAxis, out Vector3 aroundAxis, out Vector3 towardAxis)
        {
            aroundAxis = towardAxis = alongAxis = new Vector3(0, 1, 0);

            particlePosition -= fieldPosition;            
            inverseRotation.Rotate(ref particlePosition);
            particlePosition /= fieldSize;

            // Start of code for Cube

            // Start by positioning hte particle on the torus' plane
            var projectedPosition = new Vector3(particlePosition.X, 0, particlePosition.Z);
            var distanceFromOriginSquared = projectedPosition.LengthSquared();

            // If the particle is at the torus' center, it is considered to be outside the torus (provided small radius <= big radius)
           // if (distanceFromOriginSquared <= 0)
           //     return 0;

            var distanceFromOrigin = Math.Sqrt(distanceFromOriginSquared);
            var distSquared = 1 + distanceFromOriginSquared - 2 * distanceFromOrigin + particlePosition.Y * particlePosition.Y;

            //if (distSquared >= smallRadiusSquared)
            //    return 0;

            var totalStrength = (distSquared >= smallRadiusSquared) ? 0 : 1 - ((float) Math.Sqrt(distSquared) / smallRadius);
            // End of code for Cube

            // Fix the field's axis back to world space
            var forceAxis = Vector3.Cross(alongAxis, projectedPosition);
            fieldRotation.Rotate(ref forceAxis);
            forceAxis.Normalize();
            alongAxis = forceAxis;

            projectedPosition = (distanceFromOrigin > 0) ? (projectedPosition/(float)distanceFromOrigin) : projectedPosition;
            projectedPosition -= particlePosition;
            projectedPosition *= fieldSize;
            fieldRotation.Rotate(ref projectedPosition);
            towardAxis = -projectedPosition;
            towardAxis.Normalize();

            aroundAxis = Vector3.Cross(towardAxis, alongAxis);

            return totalStrength;
        }

    }
}
