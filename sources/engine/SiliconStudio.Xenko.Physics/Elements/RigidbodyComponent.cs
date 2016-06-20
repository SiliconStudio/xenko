// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract("RigidbodyComponent")]
    [Display("Rigidbody")]
    public sealed class RigidbodyComponent : PhysicsSkinnedComponentBase
    {
        [DataMemberIgnore]
        internal BulletSharp.RigidBody InternalRigidBody;

        internal delegate void GetWorldTransformDelegate(out Matrix transform);

        [DataMemberIgnore]
        internal GetWorldTransformDelegate GetWorldTransformCallback;

        internal delegate void SetWorldTransformDelegate(Matrix transform);

        [DataMemberIgnore]
        internal SetWorldTransformDelegate SetWorldTransformCallback;

        [DataMemberIgnore]
        internal XenkoMotionState MotionState;

        /// <summary>
        /// Gets the linked constraints.
        /// </summary>
        /// <value>
        /// The linked constraints.
        /// </value>
        [DataMemberIgnore]
        public List<Constraint> LinkedConstraints { get; }

        public RigidbodyComponent()
        {
            LinkedConstraints = new List<Constraint>();
            MotionState = new XenkoMotionState(this);
        }

        private bool isKinematic;

        [DataMember(75)]
        public bool IsKinematic
        {
            get { return isKinematic; }
            set
            {
                isKinematic = value;

                if (InternalRigidBody == null) return;
                RigidBodyType = value ? RigidBodyTypes.Kinematic : RigidBodyTypes.Dynamic;
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
                return mass;
            }
            set
            {
                mass = value;

                if(InternalRigidBody == null) return;

                var inertia = ColliderShape.InternalShape.CalculateLocalInertia(value);
                InternalRigidBody.SetMassProps(value, inertia);
                InternalRigidBody.UpdateInertiaTensor(); //this was the major headache when I had to debug Slider and Hinge constraint
            }
        }

        /// <summary>
        /// Gets the collider shape.
        /// </summary>
        /// <value>
        /// The collider shape.
        /// </value>
        [DataMemberIgnore]
        public override ColliderShape ColliderShape
        {
            get
            {
                return ProtectedColliderShape;
            }
            set
            {
                ProtectedColliderShape = value;

                if (InternalRigidBody == null) return;

                NativeCollisionObject.CollisionShape = value.InternalShape;

                var inertia = ProtectedColliderShape.InternalShape.CalculateLocalInertia(mass);
                InternalRigidBody.SetMassProps(mass, inertia);
                InternalRigidBody.UpdateInertiaTensor(); //this was the major headache when I had to debug Slider and Hinge constraint
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
                return linearDamping;
            }
            set
            {
                linearDamping = value;

                InternalRigidBody?.SetDamping(value, AngularDamping);
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
                return angularDamping;
            }
            set
            {
                angularDamping = value;

                InternalRigidBody?.SetDamping(LinearDamping, value);
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
                return overrideGravity;
            }
            set
            {
                overrideGravity = value;

                if (InternalRigidBody == null) return;

                if (value)
                {
                    if (((int)InternalRigidBody.Flags & (int)BulletSharp.RigidBodyFlags.DisableWorldGravity) != 0) return;
                    // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                    InternalRigidBody.Flags |= BulletSharp.RigidBodyFlags.DisableWorldGravity;
                }
                else
                {
                    if (((int)InternalRigidBody.Flags & (int)BulletSharp.RigidBodyFlags.DisableWorldGravity) == 0) return;
                    // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                    InternalRigidBody.Flags ^= BulletSharp.RigidBodyFlags.DisableWorldGravity;
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
                return gravity;
            }
            set
            {
                gravity = value;

                if (InternalRigidBody != null)
                {
                    InternalRigidBody.Gravity = value;
                }
            }
        }

        private RigidBodyTypes type;

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [DataMemberIgnore]
        public RigidBodyTypes RigidBodyType
        {
            get
            {
                return type;
            }
            set
            {
                type = value;

                if (InternalRigidBody == null) return;

                switch (value)
                {
                    case RigidBodyTypes.Dynamic:
                        if (((int)InternalRigidBody.CollisionFlags & (int)BulletSharp.CollisionFlags.StaticObject) != 0) InternalRigidBody.CollisionFlags ^= BulletSharp.CollisionFlags.StaticObject;
                        if (((int)InternalRigidBody.CollisionFlags & (int)BulletSharp.CollisionFlags.KinematicObject) != 0) InternalRigidBody.CollisionFlags ^= BulletSharp.CollisionFlags.KinematicObject;
                        if (InternalRigidBody != null && Simulation != null && !OverrideGravity) InternalRigidBody.Gravity = Simulation.Gravity;
                        if (InternalRigidBody != null)
                        {
                            InternalRigidBody.InterpolationAngularVelocity = Vector3.Zero;
                            InternalRigidBody.LinearVelocity = Vector3.Zero;
                            InternalRigidBody.InterpolationAngularVelocity = Vector3.Zero;
                            InternalRigidBody.AngularVelocity = Vector3.Zero;
                        }
                        break;

                    case RigidBodyTypes.Static:
                        if (((int)InternalRigidBody.CollisionFlags & (int)BulletSharp.CollisionFlags.KinematicObject) != 0) InternalRigidBody.CollisionFlags ^= BulletSharp.CollisionFlags.KinematicObject;
                        InternalRigidBody.CollisionFlags |= BulletSharp.CollisionFlags.StaticObject;
                        if (InternalRigidBody != null && !OverrideGravity) InternalRigidBody.Gravity = Vector3.Zero;
                        if (InternalRigidBody != null)
                        {
                            InternalRigidBody.InterpolationAngularVelocity = Vector3.Zero;
                            InternalRigidBody.LinearVelocity = Vector3.Zero;
                            InternalRigidBody.InterpolationAngularVelocity = Vector3.Zero;
                            InternalRigidBody.AngularVelocity = Vector3.Zero;
                        }
                        break;

                    case RigidBodyTypes.Kinematic:
                        if (((int)InternalRigidBody.CollisionFlags & (int)BulletSharp.CollisionFlags.StaticObject) != 0) InternalRigidBody.CollisionFlags ^= BulletSharp.CollisionFlags.StaticObject;
                        InternalRigidBody.CollisionFlags |= BulletSharp.CollisionFlags.KinematicObject;
                        if (InternalRigidBody != null && !OverrideGravity) InternalRigidBody.Gravity = Vector3.Zero;
                        if (InternalRigidBody != null)
                        {
                            InternalRigidBody.InterpolationAngularVelocity = Vector3.Zero;
                            InternalRigidBody.LinearVelocity = Vector3.Zero;
                            InternalRigidBody.InterpolationAngularVelocity = Vector3.Zero;
                            InternalRigidBody.AngularVelocity = Vector3.Zero;
                        }
                        break;
                }
            }
        }

        protected override void OnAttach()
        {
            InternalRigidBody = new BulletSharp.RigidBody(0.0f, MotionState, ColliderShape.InternalShape, Vector3.Zero)
            {
                UserObject = this
            };

            NativeCollisionObject = InternalRigidBody;

            NativeCollisionObject.ContactProcessingThreshold = !Simulation.CanCcd ? 1e18f : 1e30f;

            if (ColliderShape.NeedsCustomCollisionCallback)
            {
                NativeCollisionObject.CollisionFlags |= BulletSharp.CollisionFlags.CustomMaterialCallback;
            }

            if (ColliderShape.Is2D) //set different defaults for 2D shapes
            {
                InternalRigidBody.LinearFactor = new Vector3(1.0f, 1.0f, 0.0f);
                InternalRigidBody.AngularFactor = new Vector3(0.0f, 0.0f, 1.0f);
            }

            var inertia = ColliderShape.InternalShape.CalculateLocalInertia(mass);
            InternalRigidBody.SetMassProps(mass, inertia);
            InternalRigidBody.UpdateInertiaTensor(); //this was the major headache when I had to debug Slider and Hinge constraint

            base.OnAttach();

            Mass = mass;
            LinearDamping = linearDamping;
            AngularDamping = angularDamping;
            OverrideGravity = overrideGravity;
            Gravity = gravity;
            RigidBodyType = IsKinematic ? RigidBodyTypes.Kinematic : RigidBodyTypes.Dynamic;

            GetWorldTransformCallback = (out Matrix transform) => RigidBodyGetWorldTransform(out transform);
            SetWorldTransformCallback = transform => RigidBodySetWorldTransform(ref transform);

            UpdatePhysicsTransformation(); //this will set position and rotation of the collider

            Simulation.AddRigidBody(this, (CollisionFilterGroupFlags)CollisionGroup, CanCollideWith);
        }

        protected override void OnDetach()
        {
            if (NativeCollisionObject == null) return;

            //Remove constraints safely
            var toremove = new FastList<Constraint>();
            foreach (var c in LinkedConstraints)
            {
                toremove.Add(c);                
            }

            foreach (var disposable in toremove)
            {
                disposable.Dispose();
            }

            LinkedConstraints.Clear();
            //~Remove constraints

            Simulation.RemoveRigidBody(this);

            base.OnDetach();
        }

        protected internal override void OnUpdateDraw()
        {
            base.OnUpdateDraw();

            //write to ModelViewHierarchy
            var model = Data.ModelComponent;
            if (type != RigidBodyTypes.Dynamic) return;

            model.Skeleton.NodeTransformations[BoneIndex].WorldMatrix = BoneWorldMatrixOut;

            if (DebugEntity == null) return;

            Vector3 scale, pos;
            Quaternion rot;
            BoneWorldMatrixOut.Decompose(out scale, out rot, out pos);
            DebugEntity.Transform.Position = pos;
            DebugEntity.Transform.Rotation = rot;
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

        /// <summary>
        /// Gets the total torque.
        /// </summary>
        /// <value>
        /// The total torque.
        /// </value>
        public Vector3 TotalTorque => InternalRigidBody?.TotalTorque ?? Vector3.Zero;

        /// <summary>
        /// Applies the impulse.
        /// </summary>
        /// <param name="impulse">The impulse.</param>
        public void ApplyImpulse(Vector3 impulse)
        {
            InternalRigidBody?.ApplyCentralImpulse(impulse);
        }

        /// <summary>
        /// Applies the impulse.
        /// </summary>
        /// <param name="impulse">The impulse.</param>
        /// <param name="localOffset">The local offset.</param>
        public void ApplyImpulse(Vector3 impulse, Vector3 localOffset)
        {
            InternalRigidBody?.ApplyImpulse(impulse, localOffset);
        }

        /// <summary>
        /// Applies the force.
        /// </summary>
        /// <param name="force">The force.</param>
        public void ApplyForce(Vector3 force)
        {
            InternalRigidBody?.ApplyCentralForce(force);
        }

        /// <summary>
        /// Applies the force.
        /// </summary>
        /// <param name="force">The force.</param>
        /// <param name="localOffset">The local offset.</param>
        public void ApplyForce(Vector3 force, Vector3 localOffset)
        {
            InternalRigidBody?.ApplyForce(force, localOffset);
        }

        /// <summary>
        /// Applies the torque.
        /// </summary>
        /// <param name="torque">The torque.</param>
        public void ApplyTorque(Vector3 torque)
        {
            InternalRigidBody?.ApplyTorque(torque);
        }

        /// <summary>
        /// Applies the torque impulse.
        /// </summary>
        /// <param name="torque">The torque.</param>
        public void ApplyTorqueImpulse(Vector3 torque)
        {
            InternalRigidBody?.ApplyTorqueImpulse(torque);
        }

        /// <summary>
        /// Clears all forces being applied to this rigidbody
        /// </summary>
        public void ClearForces()
        {
            InternalRigidBody?.ClearForces();
        }

        /// <summary>
        /// Gets or sets the angular velocity.
        /// </summary>
        /// <value>
        /// The angular velocity.
        /// </value>
        [DataMemberIgnore]
        public Vector3 AngularVelocity
        {
            get { return InternalRigidBody?.AngularVelocity ?? Vector3.Zero; }
            set { if(InternalRigidBody != null) InternalRigidBody.AngularVelocity = value; }
        }

        /// <summary>
        /// Gets or sets the linear velocity.
        /// </summary>
        /// <value>
        /// The linear velocity.
        /// </value>
        [DataMemberIgnore]
        public Vector3 LinearVelocity
        {
            get { return InternalRigidBody?.LinearVelocity ?? Vector3.Zero; }
            set { if(InternalRigidBody != null) InternalRigidBody.LinearVelocity = value; }
        }

        /// <summary>
        /// Gets the total force.
        /// </summary>
        /// <value>
        /// The total force.
        /// </value>
        public Vector3 TotalForce => InternalRigidBody?.TotalForce ?? Vector3.Zero;

        /// <summary>
        /// Gets or sets the angular factor.
        /// </summary>
        /// <value>
        /// The angular factor.
        /// </value>
        [DataMemberIgnore]
        public Vector3 AngularFactor
        {
            get { return InternalRigidBody?.AngularFactor ?? Vector3.Zero; }
            set { if(InternalRigidBody != null) InternalRigidBody.AngularFactor = value; }
        }

        /// <summary>
        /// Gets or sets the linear factor.
        /// </summary>
        /// <value>
        /// The linear factor.
        /// </value>
        [DataMemberIgnore]
        public Vector3 LinearFactor
        {
            get { return InternalRigidBody?.LinearFactor ?? Vector3.Zero; }
            set { if(InternalRigidBody != null) InternalRigidBody.LinearFactor = value; }
        }
    }
}
