using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<BoxColliderShapeDesc>))]
    [DataContract("BoxColliderShapeDesc")]
    public class BoxColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public Vector3 LocalOffset;

        [DataMember(20)]
        public Vector3 HalfExtents;
    }
}