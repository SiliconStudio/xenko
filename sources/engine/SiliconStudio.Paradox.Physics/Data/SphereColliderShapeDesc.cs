using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<SphereColliderShapeDesc>))]
    [DataContract("SphereColliderShapeDesc")]
    [Display(50, "SphereColliderShape")]
    public class SphereColliderShapeDesc : IColliderShapeDesc
    {
        /// <userdoc>
        /// Select this if this shape will represent a Circle 2D shape
        /// </userdoc>
        [DataMember(10)]
        public bool Is2D;

        /// <userdoc>
        /// The offset with the real graphic mesh.
        /// </userdoc>
        [DataMember(20)]
        public Vector3 LocalOffset;

        /// <userdoc>
        /// The radius of the sphere/circle.
        /// </userdoc>
        [DataMember(30)] 
        public float Radius = 1.0f;
    }
}