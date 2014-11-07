using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<CapsuleColliderShapeDesc>))]
    [DataContract("CapsuleColliderShapeDesc")]
    public class CapsuleColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public bool Is2D;

        [DataMember(20)]
        public Vector3 LocalOffset;

        [DataMember(30)]
        public float Radius;

        [DataMember(40)]
        public float Height;

        [DataMember(50)]
        public Vector3 UpAxis = Vector3.UnitY;
    }
}