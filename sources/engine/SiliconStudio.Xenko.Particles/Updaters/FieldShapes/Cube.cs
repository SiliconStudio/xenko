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
            scl = new Vector3(1, 1, 1);
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

        public override void PreUpdateField(Vector3 fieldPosition, Quaternion fieldRotation, Vector3 fieldSize)
        {
            this.fieldSize = fieldSize;
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
            var inverseRotation = new Quaternion(-fieldRotation.X, fieldRotation.Y, fieldRotation.Z, fieldRotation.W);
            inverseRotation.Rotate(ref particlePosition);
            particlePosition /= fieldSize;

            // Start of code for Cube

            // End of code for Cube


            return 1;
        }

    }
}
