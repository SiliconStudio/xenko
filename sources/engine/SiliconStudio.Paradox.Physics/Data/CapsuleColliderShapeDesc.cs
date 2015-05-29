using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<CapsuleColliderShapeDesc>))]
    [DataContract("CapsuleColliderShapeDesc")]
    [Display(50, "CapsuleColliderShape")]
    public class CapsuleColliderShapeDesc : IColliderShapeDesc
    {
        /// <userdoc>
        /// Select this if this shape will represent a 2D shape
        /// </userdoc>
        [DataMember(10)]
        public bool Is2D;

        /// <userdoc>
        /// The offset with the real graphic mesh.
        /// </userdoc>
        [DataMember(20)]
        public Vector3 LocalOffset;

        /// <userdoc>
        /// The local rotation of the collider shape.
        /// </userdoc>
        [DataMember(30)]
        public Quaternion LocalRotation = Quaternion.Identity;

        /// <userdoc>
        /// The radius of the capsule.
        /// </userdoc>
        [DataMember(40)] 
        public float Radius = 0.5f;

        /// <userdoc>
        /// The height of the capsule.
        /// </userdoc>
        [DataMember(50)] 
        public float Height = 1.0f;

        /// <userdoc>
        /// The up axis of the capsule, this must be either (1,0,0),(0,1,0),(0,0,1).
        /// </userdoc>
        [DataMember(60)]
        public Vector3 UpAxis = Vector3.UnitY;
    }
}