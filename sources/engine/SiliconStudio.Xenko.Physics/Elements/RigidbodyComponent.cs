using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract("RigidbodyComponent")]
    [Display("Rigidbody")]
    public sealed class RigidbodyComponent : PhysicsSkinnedComponentBase
    {
        private bool isKinematic;

        [DataMemberIgnore]
        public new RigidBody Collider
        {
            get { return (RigidBody)base.Collider; }
            set { base.Collider = value; }
        }

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

        protected override void OnColliderUpdated()
        {
            base.OnColliderUpdated();
            Mass = mass;
            LinearDamping = linearDamping;
            AngularDamping = angularDamping;
            OverrideGravity = overrideGravity;
            Gravity = gravity;
        }

        protected override void OnAttach()
        {
            base.OnAttach();

            var rb = Simulation.CreateRigidBody(ColliderShape);

            rb.Entity = Entity;
            rb.GetWorldTransformCallback = (out Matrix transform) => RigidBodyGetWorldTransform(out transform);
            rb.SetWorldTransformCallback = transform => RigidBodySetWorldTransform(ref transform);
            Collider = rb;
            UpdatePhysicsTransformation(); //this will set position and rotation of the collider

            rb.Type = IsKinematic ? RigidBodyTypes.Kinematic : RigidBodyTypes.Dynamic;
            if (rb.Mass == 0.0f) rb.Mass = 1.0f;

            if (IsDefaultGroup)
            {
                Simulation.AddRigidBody(rb, CollisionFilterGroupFlags.DefaultFilter, CollisionFilterGroupFlags.AllFilter);
            }
            else
            {
                Simulation.AddRigidBody(rb, (CollisionFilterGroupFlags)CollisionGroup, CanCollideWith);
            }
        }

        protected override void OnDetach()
        {
            base.OnDetach();

            var rb = Collider;
            var constraints = rb.LinkedConstraints.ToArray();
            foreach (var c in constraints)
            {
                Simulation.RemoveConstraint(c);
                c.Dispose();
            }

            Simulation.RemoveRigidBody(rb);
        }

        protected internal override void OnUpdateDraw()
        {
            base.OnUpdateDraw();

            //write to ModelViewHierarchy
            var model = Data.ModelComponent;
            if (Collider != null && Collider.Type == RigidBodyTypes.Dynamic)
            {
                model.Skeleton.NodeTransformations[BoneIndex].WorldMatrix = BoneWorldMatrixOut;

                if (DebugEntity != null)
                {
                    Vector3 scale, pos;
                    Quaternion rot;
                    BoneWorldMatrixOut.Decompose(out scale, out rot, out pos);
                    DebugEntity.Transform.Position = pos;
                    DebugEntity.Transform.Rotation = rot;
                }
            }
        }
    
        //This is called by the physics engine to update the transformation of Dynamic rigidbodies.
        private void RigidBodySetWorldTransform(ref Matrix physicsTransform)
        {
            Data.PhysicsComponent.Simulation.SimulationProfiler.Mark();
            Data.PhysicsComponent.Simulation.UpdatedRigidbodies++;

            if (BoneIndex == -1)
            {
                UpdateTransformationComponent(ref physicsTransform);
            }
            else
            {
                UpdateBoneTransformation(ref physicsTransform);
            }

            if (DebugEntity == null) return;

            Vector3 scale, pos;
            Quaternion rot;
            physicsTransform.Decompose(out scale, out rot, out pos);
            DebugEntity.Transform.Position = pos;
            DebugEntity.Transform.Rotation = rot;
        }

        //This is valid for Dynamic rigidbodies (called once at initialization)
        //and Kinematic rigidbodies, called every simulation tick (if body not sleeping) to let the physics engine know where the kinematic body is.
        private void RigidBodyGetWorldTransform(out Matrix physicsTransform)
        {
            Data.PhysicsComponent.Simulation.SimulationProfiler.Mark();
            Data.PhysicsComponent.Simulation.UpdatedRigidbodies++;

            if (BoneIndex == -1)
            {
                DerivePhysicsTransformation(out physicsTransform);
            }
            else
            {
                DeriveBonePhysicsTransformation(out physicsTransform);
            }

            if (DebugEntity == null) return;

            Vector3 scale, pos;
            Quaternion rot;
            physicsTransform.Decompose(out scale, out rot, out pos);
            DebugEntity.Transform.Position = pos;
            DebugEntity.Transform.Rotation = rot;
        }

    }
}