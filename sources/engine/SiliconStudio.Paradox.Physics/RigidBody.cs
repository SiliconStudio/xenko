// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Physics
{
    public class RigidBody : Collider
    {
        internal delegate void GetWorldTransformDelegate(out Matrix transform);

        internal GetWorldTransformDelegate GetWorldTransformCallback;

        internal delegate void SetWorldTransformDelegate(Matrix transform);

        internal SetWorldTransformDelegate SetWorldTransformCallback;

        internal ParadoxMotionState MotionState;

        internal RigidBody(ColliderShape collider)
            : base(collider)
        {
            LinkedConstraints = new List<Constraint>();
            MotionState = new ParadoxMotionState(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public override void Dispose()
        {
            LinkedConstraints.Clear();
            MotionState.Dispose();
            base.Dispose();
        }

        private float mass;

        /// <summary>
        /// Gets or sets the mass.
        /// </summary>
        /// <value>
        /// The mass.
        /// </value>
        public float Mass
        {
            get
            {
                return mass;
            }
            set
            {
                mass = value;
                var inertia = ColliderShape.InternalShape.CalculateLocalInertia(value);
                InternalRigidBody.SetMassProps(value, inertia);
                InternalRigidBody.UpdateInertiaTensor(); //this was the major headache when I had to debug Slider and Hinge constraint
            }
        }

        /// <summary>
        /// Gets or sets the angular damping.
        /// </summary>
        /// <value>
        /// The angular damping.
        /// </value>
        public float AngularDamping
        {
            get
            {
                return InternalRigidBody.AngularDamping;
            }
            set
            {
                InternalRigidBody.SetDamping(LinearDamping, value);
            }
        }

        /// <summary>
        /// Gets or sets the linear damping.
        /// </summary>
        /// <value>
        /// The linear damping.
        /// </value>
        public float LinearDamping
        {
            get
            {
                return InternalRigidBody.LinearDamping;
            }
            set
            {
                InternalRigidBody.SetDamping(value, AngularDamping);
            }
        }

        /// <summary>
        /// Gets or sets the gravity for this single rigid body overriding the engine.
        /// </summary>
        /// <value>
        /// The gravity.
        /// </value>
        public Vector3 Gravity
        {
            get { return InternalRigidBody.Gravity; }
            set { InternalRigidBody.Gravity = value; }
        }

        /// <summary>
        /// If you want to override gravity using the Gravity property setter, first set this value to true
        /// </summary>
        /// <value>
        /// If this body should override the main simulation gravity or not
        /// </value>
        public bool OverrideGravity
        {
            get
            {
                return (InternalRigidBody.Flags.HasFlag(BulletSharp.RigidBodyFlags.DisableWorldGravity));
            }
            set
            {
                if (value)
                {
                    if (!InternalRigidBody.Flags.HasFlag(BulletSharp.RigidBodyFlags.DisableWorldGravity))
                    {
                        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                        InternalRigidBody.Flags |= BulletSharp.RigidBodyFlags.DisableWorldGravity;
                    }
                }
                else
                {
                    if (InternalRigidBody.Flags.HasFlag(BulletSharp.RigidBodyFlags.DisableWorldGravity))
                    {
                        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                        InternalRigidBody.Flags ^= BulletSharp.RigidBodyFlags.DisableWorldGravity;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the total torque.
        /// </summary>
        /// <value>
        /// The total torque.
        /// </value>
        public Vector3 TotalTorque
        {
            get { return InternalRigidBody.TotalTorque; }
        }

        /// <summary>
        /// Applies the impulse.
        /// </summary>
        /// <param name="impulse">The impulse.</param>
        public void ApplyImpulse(Vector3 impulse)
        {
            InternalRigidBody.ApplyCentralImpulse(impulse);
        }

        /// <summary>
        /// Applies the impulse.
        /// </summary>
        /// <param name="impulse">The impulse.</param>
        /// <param name="localOffset">The local offset.</param>
        public void ApplyImpulse(Vector3 impulse, Vector3 localOffset)
        {
            InternalRigidBody.ApplyImpulse(impulse, localOffset);
        }

        /// <summary>
        /// Applies the force.
        /// </summary>
        /// <param name="force">The force.</param>
        public void ApplyForce(Vector3 force)
        {
            InternalRigidBody.ApplyCentralForce(force);
        }

        /// <summary>
        /// Applies the force.
        /// </summary>
        /// <param name="force">The force.</param>
        /// <param name="localOffset">The local offset.</param>
        public void ApplyForce(Vector3 force, Vector3 localOffset)
        {
            InternalRigidBody.ApplyForce(force, localOffset);
        }

        /// <summary>
        /// Applies the torque.
        /// </summary>
        /// <param name="torque">The torque.</param>
        public void ApplyTorque(Vector3 torque)
        {
            InternalRigidBody.ApplyTorque(torque);
        }

        /// <summary>
        /// Applies the torque impulse.
        /// </summary>
        /// <param name="torque">The torque.</param>
        public void ApplyTorqueImpulse(Vector3 torque)
        {
            InternalRigidBody.ApplyTorqueImpulse(torque);
        }

        /// <summary>
        /// Gets or sets the angular velocity.
        /// </summary>
        /// <value>
        /// The angular velocity.
        /// </value>
        public Vector3 AngularVelocity
        {
            get { return InternalRigidBody.AngularVelocity; }
            set { InternalRigidBody.AngularVelocity = value; }
        }

        /// <summary>
        /// Gets or sets the linear velocity.
        /// </summary>
        /// <value>
        /// The linear velocity.
        /// </value>
        public Vector3 LinearVelocity
        {
            get { return InternalRigidBody.LinearVelocity; }
            set { InternalRigidBody.LinearVelocity = value; }
        }

        /// <summary>
        /// Gets the total force.
        /// </summary>
        /// <value>
        /// The total force.
        /// </value>
        public Vector3 TotalForce
        {
            get { return InternalRigidBody.TotalForce; }
        }

        /// <summary>
        /// Gets or sets the angular factor.
        /// </summary>
        /// <value>
        /// The angular factor.
        /// </value>
        public Vector3 AngularFactor
        {
            get { return InternalRigidBody.AngularFactor; }
            set { InternalRigidBody.AngularFactor = value; }
        }

        /// <summary>
        /// Gets or sets the linear factor.
        /// </summary>
        /// <value>
        /// The linear factor.
        /// </value>
        public Vector3 LinearFactor
        {
            get { return InternalRigidBody.LinearFactor; }
            set { InternalRigidBody.LinearFactor = value; }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public RigidBodyTypes Type
        {
            get
            {
                if (InternalRigidBody.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.StaticObject)) return RigidBodyTypes.Static;
                return InternalRigidBody.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.KinematicObject) ? RigidBodyTypes.Kinematic : RigidBodyTypes.Dynamic;
            }
            set
            {
                switch (value)
                {
                    case RigidBodyTypes.Dynamic:
                        if (InternalRigidBody.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.StaticObject)) InternalRigidBody.CollisionFlags ^= BulletSharp.CollisionFlags.StaticObject;
                        else if (InternalRigidBody.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.KinematicObject)) InternalRigidBody.CollisionFlags ^= BulletSharp.CollisionFlags.KinematicObject;
                        break;

                    case RigidBodyTypes.Static:
                        if (InternalRigidBody.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.KinematicObject)) InternalRigidBody.CollisionFlags ^= BulletSharp.CollisionFlags.KinematicObject;
                        InternalRigidBody.CollisionFlags |= BulletSharp.CollisionFlags.StaticObject;
                        break;

                    case RigidBodyTypes.Kinematic:
                        if (InternalRigidBody.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.StaticObject)) InternalRigidBody.CollisionFlags ^= BulletSharp.CollisionFlags.StaticObject;
                        InternalRigidBody.CollisionFlags |= BulletSharp.CollisionFlags.KinematicObject;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the linked constraints.
        /// </summary>
        /// <value>
        /// The linked constraints.
        /// </value>
        public List<Constraint> LinkedConstraints { get; private set; }

        internal BulletSharp.RigidBody InternalRigidBody;
    }
}