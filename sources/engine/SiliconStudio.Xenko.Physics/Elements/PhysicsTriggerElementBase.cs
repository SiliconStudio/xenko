using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("PhysicsTriggerElementBase")]
    [Display(40, "PhysicsTriggerElementBase")]
    public abstract class PhysicsTriggerElementBase : PhysicsElementBase
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

        [DataMemberIgnore]
        public override Collider Collider
        {
            get { return base.Collider; }
            internal set
            {
                base.Collider = value;
                IsTrigger = isTrigger;
            }
        }
    }
}