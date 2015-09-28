using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("StaticColliderElement")]
    [Display(40, "Static Collider")]
    public class StaticColliderElement : PhysicsElementBase, IPhysicsElement
    {
        public override Types Type => Types.StaticCollider;
    }
}