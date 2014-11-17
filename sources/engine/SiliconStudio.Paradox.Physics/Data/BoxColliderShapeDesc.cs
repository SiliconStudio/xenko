using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<BoxColliderShapeDesc>))]
    [DataContract("BoxColliderShapeDesc")]
    public class BoxColliderShapeDesc : IColliderShapeDesc
    {
        /// <userdoc>
        /// The offset with the real graphic mesh.
        /// </userdoc>
        [DataMember(10)]
        public Vector3 LocalOffset;

        /// <userdoc>
        /// Half Extent size of the box.
        /// </userdoc>
        [DataMember(20)]
        public Vector3 HalfExtents;
    }
}