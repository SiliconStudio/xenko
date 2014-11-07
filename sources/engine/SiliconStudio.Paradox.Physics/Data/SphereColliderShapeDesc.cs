using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<SphereColliderShapeDesc>))]
    [DataContract("SphereColliderShapeDesc")]
    public class SphereColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public bool Is2D;

        [DataMember(20)]
        public Vector3 LocalOffset;

        [DataMember(30)]
        public float Radius;
    }
}