using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract("CharacterComponent")]
    [Display(40, "Character")]
    public sealed class CharacterComponent : PhysicsComponent
    {
        public CharacterComponent()
        {
            StepHeight = 0.1f;
        }

        [DataMemberIgnore]
        public new Character Collider
        {
            get { return (Character)base.Collider; }
            set { base.Collider = value; }
        }

        /// <summary>
        /// Gets or sets the height of the character step.
        /// </summary>
        /// <value>
        /// The height of the character step.
        /// </value>
        /// <userdoc>
        /// Only valid for CharacterController type, describes the max slope height a character can climb. Cannot change during run-time.
        /// </userdoc>
        [DataMember(75)]
        [DefaultValue(0.1f)]
        public float StepHeight { get; set; }

        private float fallSpeed = 10.0f;

        /// <summary>
        /// Gets or sets if this character element fall speed
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The fall speed of this character
        /// </userdoc>
        [DataMember(80)]
        public float FallSpeed
        {
            get
            {
                var c = (Character)InternalCollider;
                return c?.FallSpeed ?? fallSpeed;
            }
            set
            {
                var c = (Character)InternalCollider;
                if (c != null)
                {
                    c.FallSpeed = value;
                }
                else
                {
                    fallSpeed = value;
                }
            }
        }

        private float maxSlope;

        /// <summary>
        /// Gets or sets if this character element max slope
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The max slope this character can climb
        /// </userdoc>
        [DataMember(85)]
        public float MaxSlope
        {
            get
            {
                var c = (Character)InternalCollider;
                return c?.MaxSlope ?? maxSlope;
            }
            set
            {
                var c = (Character)InternalCollider;
                if (c != null)
                {
                    c.MaxSlope = value;
                }
                else
                {
                    maxSlope = value;
                }
            }
        }

        private float jumpSpeed = 5.0f;

        /// <summary>
        /// Gets or sets if this character element max slope
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The max slope this character can climb
        /// </userdoc>
        [DataMember(90)]
        public float JumpSpeed
        {
            get
            {
                var c = (Character)InternalCollider;
                return c?.JumpSpeed ?? jumpSpeed;
            }
            set
            {
                var c = (Character)InternalCollider;
                if (c != null)
                {
                    c.JumpSpeed = value;
                }
                else
                {
                    jumpSpeed = value;
                }
            }
        }

        private float gravity = -10.0f;

        /// <summary>
        /// Gets or sets if this character gravity
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The gravity force applied to this character
        /// </userdoc>
        [DataMember(95)]
        public float Gravity
        {
            get
            {
                var c = (Character)InternalCollider;
                return c?.Gravity ?? gravity;
            }
            set
            {
                var c = (Character)InternalCollider;
                if (c != null)
                {
                    c.Gravity = value;
                }
                else
                {
                    gravity = value;
                }
            }
        }


        protected override void OnColliderUpdated()
        {
            base.OnColliderUpdated();
            FallSpeed = fallSpeed;
            MaxSlope = maxSlope;
            JumpSpeed = jumpSpeed;
            Gravity = gravity;
        }

        protected override void OnAttach()
        {
            base.OnAttach();

            var ch = Simulation.CreateCharacter(ColliderShape, StepHeight);

            Collider = ch;
            Collider.Entity = Entity;
            UpdatePhysicsTransformation(); //this will set position and rotation of the collider

            if (IsDefaultGroup)
            {
                Simulation.AddCharacter(ch, CollisionFilterGroupFlags.DefaultFilter, CollisionFilterGroupFlags.AllFilter);
            }
            else
            {
                Simulation.AddCharacter(ch, (CollisionFilterGroupFlags)CollisionGroup, CanCollideWith);
            }
        }

        protected override void OnDetach()
        {
            base.OnDetach();
            Simulation.RemoveCharacter(Collider);
        }
    }
}