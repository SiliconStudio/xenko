using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<Box2DColliderShapeDesc>))]
    [DataContract("Box2DColliderShapeDesc")]
    [Display(50, "Box2DColliderShape")]
    public class Box2DColliderShapeDesc : IColliderShapeDesc
    {
        /// <userdoc>
        /// The offset with the real graphic mesh.
        /// </userdoc>
        [DataMember(10)]
        public Vector3 LocalOffset;

        /// <userdoc>
        /// The local rotation of the collider shape.
        /// </userdoc>
        [DataMember(20)] 
        public Quaternion LocalRotation = Quaternion.Identity;

        /// <userdoc>
        /// Half Extent size of the box.
        /// </userdoc>
        [DataMember(30)] 
        public Vector2 HalfExtent = Vector2.One;
    }
}