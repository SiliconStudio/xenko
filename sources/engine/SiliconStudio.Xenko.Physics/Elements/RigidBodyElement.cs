using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract("RigidbodyElement")]
    [Display(40, "Rigidbody")]
    public class RigidbodyElement : PhysicsSkinnedElementBase
    {
        public override Types Type => InternalCollider != null
            ? (((RigidBody)InternalCollider).Type == RigidBodyTypes.Kinematic ? Types.KinematicRigidBody : Types.DynamicRigidBody)
            : (isKinematic ? Types.KinematicRigidBody : Types.DynamicRigidBody);

        private bool isKinematic;

        [DataMember(75)]
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

        private float mass = 1.0f;

        /// <summary>
        /// Gets or sets the mass of this Rigidbody
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The mass of this Rigidbody
        /// </userdoc>
        [DataMember(80)]
        public float Mass
        {
            get
            {
                var c = (RigidBody)InternalCollider;
                return c?.Mass ?? mass;
            }
            set
            {
                var c = (RigidBody)InternalCollider;
                if (c != null)
                {
                    c.Mass = value;
                }
                else
                {
                    mass = value;
                }
            }
        }

        private float linearDamping;

        /// <summary>
        /// Gets or sets the linear damping of this rigidbody
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The linear damping of this Rigidbody
        /// </userdoc>
        [DataMember(85)]
        public float LinearDamping
        {
            get
            {
                var c = (RigidBody)InternalCollider;
                return c?.LinearDamping ?? linearDamping;
            }
            set
            {
                var c = (RigidBody)InternalCollider;
                if (c != null)
                {
                    c.LinearDamping = value;
                }
                else
                {
                    linearDamping = value;
                }
            }
        }

        private float angularDamping;

        /// <summary>
        /// Gets or sets the angular damping of this rigidbody
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The angular damping of this Rigidbody
        /// </userdoc>
        [DataMember(90)]
        public float AngularDamping
        {
            get
            {
                var c = (RigidBody)InternalCollider;
                return c?.AngularDamping ?? angularDamping;
            }
            set
            {
                var c = (RigidBody)InternalCollider;
                if (c != null)
                {
                    c.AngularDamping = value;
                }
                else
                {
                    angularDamping = value;
                }
            }
        }

        private bool overrideGravity;

        /// <summary>
        /// Gets or sets the angular damping of this rigidbody
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The angular damping of this Rigidbody
        /// </userdoc>
        [DataMember(95)]
        public bool OverrideGravity
        {
            get
            {
                var c = (RigidBody)InternalCollider;
                return c?.OverrideGravity ?? overrideGravity;
            }
            set
            {
                var c = (RigidBody)InternalCollider;
                if (c != null)
                {
                    c.OverrideGravity = value;
                }
                else
                {
                    overrideGravity = value;
                }
            }
        }

        private Vector3 gravity = Vector3.Zero;

        /// <summary>
        /// Gets or sets the angular damping of this rigidbody
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The angular damping of this Rigidbody
        /// </userdoc>
        [DataMember(100)]
        public Vector3 Gravity
        {
            get
            {
                var c = (RigidBody)InternalCollider;
                return c?.Gravity ?? gravity;
            }
            set
            {
                var c = (RigidBody)InternalCollider;
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

        [DataMemberIgnore]
        public override Collider Collider
        {
            get { return base.Collider; }
            internal set
            {
                base.Collider = value;
                Mass = mass;
                LinearDamping = linearDamping;
                AngularDamping = angularDamping;
                OverrideGravity = overrideGravity;
                Gravity = gravity;
            }
        }
    }
}