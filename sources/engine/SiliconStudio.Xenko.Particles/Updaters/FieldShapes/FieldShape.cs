using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;

namespace SiliconStudio.Xenko.Particles.Updaters.FieldShapes
{
    [DataContract("FieldShape")]
    public abstract class FieldShape
    {
        public abstract DebugDrawShape GetDebugDrawShape(out Vector3 pos, out Quaternion rot, out Vector3 scl);

        public abstract void PreUpdateField(Vector3 fieldPosition, Quaternion fieldRotation, Vector3 fieldSize);

        public abstract float GetFieldStrength(
            Vector3 particlePosition, Vector3 particleVelocity,
            out Vector3 alongAxis, out Vector3 aroundAxis, out Vector3 towardAxis);
    }
}
