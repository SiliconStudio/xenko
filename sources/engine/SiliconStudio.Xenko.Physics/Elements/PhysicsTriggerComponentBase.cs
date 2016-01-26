using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract("PhysicsTriggerComponentBase")]
    [Display("PhysicsTriggerComponentBase")]
    public abstract class PhysicsTriggerComponentBase : PhysicsComponent
    {
        private bool isTrigger;

        [DataMember(71)]
        public bool IsTrigger
        {
            get
            {
                return isTrigger;
            }
            set
            {
                isTrigger = value;

                if (NativeCollisionObject == null) return;

                if (isTrigger)
                {
                    NativeCollisionObject.CollisionFlags |= BulletSharp.CollisionFlags.NoContactResponse;
                }
                else
                {
                    if (NativeCollisionObject.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.NoContactResponse))
                    {
                        NativeCollisionObject.CollisionFlags ^= BulletSharp.CollisionFlags.NoContactResponse;
                    }
                }
            }
        }

        protected override void OnAttach()
        {
            base.OnAttach();
            //set pre-set post deserialization properties
            IsTrigger = isTrigger;
        }
    }
}