using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("RigidbodyElement")]
    [Display(40, "Rigidbody")]
    public class RigidbodyElement : PhysicsSkinnedElementBase, IPhysicsElement
    {
        public override Types Type => InternalCollider != null
            ? (((RigidBody)InternalCollider).Type == RigidBodyTypes.Kinematic ? Types.KinematicRigidBody : Types.DynamicRigidBody)
            : (isKinematic ? Types.KinematicRigidBody : Types.DynamicRigidBody);

        private bool isKinematic;

        [DataMember(60)]
        public bool IsKinematic
        {
            get { return InternalCollider != null ? ((RigidBody)InternalCollider).Type == RigidBodyTypes.Kinematic : isKinematic; }
            set
            {
                if (InternalCollider != null)
                {
                    ((RigidBody)InternalCollider).Type = value ? RigidBodyTypes.Kinematic : RigidBodyTypes.Dynamic;
                }
                else
                {
                    isKinematic = value;
                }
            }
        }
    }
}