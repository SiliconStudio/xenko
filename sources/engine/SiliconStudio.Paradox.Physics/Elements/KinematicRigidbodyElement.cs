using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("KinematicRigidbodyElement")]
    [Display(40, "Kinematic RigidBody")]
    public class KinematicRigidbodyElement : PhysicsElementBase, IPhysicsElement
    {
        public override Types Type
        {
            get { return Types.KinematicRigidBody; }
        }
    }
}