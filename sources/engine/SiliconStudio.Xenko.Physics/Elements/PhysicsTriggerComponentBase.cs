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
                return InternalCollider?.IsTrigger ?? isTrigger;
            }
            set
            {
                if (InternalCollider == null)
                {
                    isTrigger = value;
                }
                else
                {
                    InternalCollider.IsTrigger = value;
                }
            }
        }

        protected override void OnColliderUpdated()
        {
            base.OnColliderUpdated();
            IsTrigger = isTrigger;
        }
    }
}