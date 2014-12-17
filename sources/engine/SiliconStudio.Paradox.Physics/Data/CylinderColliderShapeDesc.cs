using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<CylinderColliderShapeDesc>))]
    [DataContract("CylinderColliderShapeDesc")]
    public class CylinderColliderShapeDesc : IColliderShapeDesc
    {
        /// <userdoc>
        /// The offset with the real graphic mesh.
        /// </userdoc>
        [DataMember(10)]
        public Vector3 LocalOffset;

        /// <userdoc>
        /// Half Extent size of the cylinder.
        /// </userdoc>
        [DataMember(20)]
        public Vector3 HalfExtents;

        /// <userdoc>
        /// The up axis of the cylinder, this must be either (1,0,0),(0,1,0),(0,0,1).
        /// </userdoc>
        [DataMember(30)]
        public Vector3 UpAxis = Vector3.UnitY;
    }
}