using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract("StaticColliderComponent")]
    [Display(40, "Static Collider")]
    public sealed class StaticColliderComponent : PhysicsTriggerComponentBase
    {
        protected override void OnAttach()
        {
            base.OnAttach();

            var c = Simulation.CreateCollider(ColliderShape);

            Collider = c; //required by the next call
            Collider.Entity = Entity; //required by the next call
            UpdatePhysicsTransformation(); //this will set position and rotation of the collider

            if (IsDefaultGroup)
            {
                Simulation.AddCollider(c, CollisionFilterGroupFlags.DefaultFilter, CollisionFilterGroupFlags.AllFilter);
            }
            else
            {
                Simulation.AddCollider(c, (CollisionFilterGroupFlags)CollisionGroup, CanCollideWith);
            }
        }

        protected override void OnDetach()
        {
            base.OnDetach();

            Simulation.RemoveCollider(Collider);
        }
    }
}