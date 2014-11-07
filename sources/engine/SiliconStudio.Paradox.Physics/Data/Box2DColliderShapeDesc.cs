using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<Box2DColliderShapeDesc>))]
    [DataContract("Box2DColliderShapeDesc")]
    public class Box2DColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public Vector3 LocalOffset;

        [DataMember(20)]
        public Vector2 HalfExtent;
    }
}