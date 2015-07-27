using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<CylinderColliderShapeDesc>))]
    [DataContract("CylinderColliderShapeDesc")]
    [Display(50, "CylinderColliderShape")]
    public class CylinderColliderShapeDesc : IColliderShapeDesc
    {
        /// <userdoc>
        /// The height of the cylinder
        /// </userdoc>
        [DataMember(10)]
        [DefaultValue(1f)]
        public float Height = 1f;

        /// <userdoc>
        /// The diameter of the cylinder
        /// </userdoc>
        [DataMember(20)]
        [DefaultValue(1f)]
        public float Diameter = 1f;

        /// <userdoc>
        /// The orientation of the cylinder.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(ShapeOrientation.UpY)]
        public ShapeOrientation Orientation = ShapeOrientation.UpY;

        /// <userdoc>
        /// The offset with the real graphic mesh.
        /// </userdoc>
        [DataMember(40)]
        public Vector3 LocalOffset;

        /// <userdoc>
        /// The local rotation of the collider shape.
        /// </userdoc>
        [DataMember(50)]
        public Quaternion LocalRotation = Quaternion.Identity;
    }
}