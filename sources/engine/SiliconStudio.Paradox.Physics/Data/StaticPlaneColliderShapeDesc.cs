using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<StaticPlaneColliderShapeDesc>))]
    [DataContract("StaticPlaneColliderShapeDesc")]
    [Display(50, "StaticPlaneColliderShape")]
    public class StaticPlaneColliderShapeDesc : IColliderShapeDesc
    {
        /// <userdoc>
        /// The normal of the infinite plane.
        /// </userdoc>
        [DataMember(10)]
        public Vector3 Normal = Vector3.UnitY;

        /// <userdoc>
        /// The distance offset.
        /// </userdoc>
        [DataMember(20)]
        public float Offset;
    }
}