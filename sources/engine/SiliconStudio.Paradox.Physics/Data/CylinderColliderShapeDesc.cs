using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<CylinderColliderShapeDesc>))]
    [DataContract("CylinderColliderShapeDesc")]
    public class CylinderColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public Vector3 LocalOffset;

        [DataMember(20)]
        public Vector3 HalfExtents;

        [DataMember(30)]
        public Vector3 UpAxis = Vector3.UnitY;
    }
}