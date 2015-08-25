using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("DynamicRigidbodyElement")]
    [Display(40, "Dynamic RigidBody")]
    public class DynamicRigidbodyElement : PhysicsElementBase, IPhysicsElement
    {
        public override Types Type
        {
            get { return Types.DynamicRigidBody; }
        }
    }
}