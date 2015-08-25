using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("StaticRigidbodyElement")]
    [Display(40, "Static RigidBody")]
    public class StaticRigidbodyElement : PhysicsSkinnedElementBase, IPhysicsElement
    {
        public override Types Type
        {
            get { return Types.StaticRigidBody; }
        }
    }
}