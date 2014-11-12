using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<StaticPlaneColliderShapeDesc>))]
    [DataContract("StaticPlaneColliderShapeDesc")]
    public class StaticPlaneColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public Vector3 Normal = Vector3.UnitY;

        [DataMember(20)]
        public float Offset;
    }
}