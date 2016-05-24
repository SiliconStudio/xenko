// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Xenko.Physics.Engine;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("PhysicsComponent", Inherited = true)]
    [Display("Physics", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(PhysicsProcessor))]
    [AllowMultipleComponents]
    [ComponentOrder(3000)]
    public abstract class PhysicsComponent : ActivableEntityComponent
    {
        protected static Logger Logger = GlobalLogger.GetLogger("PhysicsComponent");

        static PhysicsComponent()
        {
            // Preload proper libbulletc native library (depending on CPU type)
            NativeLibrary.PreloadLibrary("libbulletc.dll");
        }

        protected PhysicsComponent()
        {
            CanScaleShape = true;

            ColliderShapes = new TrackingCollection<IInlineColliderShapeDesc>();
            ColliderShapes.CollectionChanged += (sender, args) =>
            {
                ColliderShapeChanged = true;
            };

            NewPairChannel = new Channel<Collision> { Preference = ChannelPreference.PreferSender };
            PairEndedChannel = new Channel<Collision> { Preference = ChannelPreference.PreferSender };
        }

        [DataMemberIgnore]
        internal BulletSharp.CollisionObject NativeCollisionObject;

        /// <userdoc>
        /// The reference to the collider Shape of this element.
        /// </userdoc>
        [DataMember(200)]
        [Category]
        [NotNullItems]
        public TrackingCollection<IInlineColliderShapeDesc> ColliderShapes { get; }

        /// <summary>
        /// Gets or sets the collision group.
        /// </summary>
        /// <value>
        /// The collision group.
        /// </value>
        /// <userdoc>
        /// The collision group of this element, default is DefaultFilter. Cannot change during run-time.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(CollisionFilterGroups.DefaultFilter)]
        public CollisionFilterGroups CollisionGroup { get; set; } = CollisionFilterGroups.DefaultFilter;

        /// <summary>
        /// Gets or sets the can collide with.
        /// </summary>
        /// <value>
        /// The can collide with.
        /// </value>
        /// <userdoc>
        /// Which collider groups this element can collide with, when nothing is selected it will collide with all groups. Cannot change during run-time.
        /// </userdoc>
        [DataMember(40)]
        [DefaultValue(CollisionFilterGroupFlags.AllFilter)]
        public CollisionFilterGroupFlags CanCollideWith { get; set; } = CollisionFilterGroupFlags.AllFilter;

        /// <summary>
        /// Gets or sets if this element will store collisions
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// Unchecking this will help with performance, ideally if this entity has no need to access collisions information should be set to false
        /// </userdoc>
        [Display("Collision events")]
        [DataMember(45)]
        [DefaultValue(true)]
        public virtual bool ProcessCollisions { get; set; } = true;

        /// <summary>
        /// Gets or sets if this element is enabled in the physics engine
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// If this element is enabled in the physics engine
        /// </userdoc>
        [DataMember(-10)]
        [DefaultValue(true)]
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;

                if (NativeCollisionObject == null) return;

                if (value)
                {
                    //allow collisions
                    if ((NativeCollisionObject.CollisionFlags & BulletSharp.CollisionFlags.NoContactResponse) != 0)
                    {
                        NativeCollisionObject.CollisionFlags ^= BulletSharp.CollisionFlags.NoContactResponse;
                    }

                    //allow simulation
                    NativeCollisionObject.ForceActivationState(canSleep ? BulletSharp.ActivationState.ActiveTag : BulletSharp.ActivationState.DisableDeactivation);
                }
                else
                {
                    //prevent collisions
                    NativeCollisionObject.CollisionFlags |= BulletSharp.CollisionFlags.NoContactResponse;

                    //prevent simulation
                    NativeCollisionObject.ForceActivationState(BulletSharp.ActivationState.DisableSimulation);
                }
            }
        }

        private bool canSleep;

        /// <summary>
        /// Gets or sets if this element can enter sleep state
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// If this element can enter sleep state and skip physics simulation while sleeping
        /// </userdoc>
        [DataMember(55)]
        public bool CanSleep
        {
            get
            {
                return canSleep;
            }
            set
            {
                canSleep = value;

                if (NativeCollisionObject == null) return;

                if (Enabled)
                {
                    NativeCollisionObject.ActivationState = value ? BulletSharp.ActivationState.ActiveTag : BulletSharp.ActivationState.DisableDeactivation;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active (awake).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive => NativeCollisionObject?.IsActive ?? false;

        /// <summary>
        /// Attempts to awake the collider.
        /// </summary>
        /// <param name="forceActivation">if set to <c>true</c> [force activation].</param>
        public void Activate(bool forceActivation = false)
        {
            NativeCollisionObject?.Activate(forceActivation);
        }

        private float restitution;

        /// <summary>
        /// Gets or sets if this element restitution
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// This element restitution (can create bounce effects)
        /// </userdoc>
        [DataMember(60)]
        public float Restitution
        {
            get
            {
                return restitution;
            }
            set
            {
                restitution = value;

                if (NativeCollisionObject != null)
                {
                    NativeCollisionObject.Restitution = restitution;
                }
            }
        }

        private float friction = 0.5f;

        /// <summary>
        /// Gets or sets the friction of this element
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The friction of this element
        /// </userdoc>
        [DataMember(65)]
        public float Friction
        {
            get
            {
                return friction;
            }
            set
            {
                friction = value;

                if (NativeCollisionObject != null)
                {
                    NativeCollisionObject.Friction = friction;
                }
            }
        }

        private float rollingFriction;

        /// <summary>
        /// Gets or sets the rolling friction of this element
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The rolling friction of this element
        /// </userdoc>
        [DataMember(66)]
        public float RollingFriction
        {
            get
            {
                return rollingFriction;
            }
            set
            {
                rollingFriction = value;

                if (NativeCollisionObject != null)
                {
                    NativeCollisionObject.RollingFriction = rollingFriction;
                }
            }
        }

        private float ccdMotionThreshold;

        [DataMember(67)]
        public float CcdMotionThreshold
        {
            get
            {
                return ccdMotionThreshold;
            }
            set
            {
                ccdMotionThreshold = value;

                if (NativeCollisionObject != null)
                {
                    NativeCollisionObject.CcdMotionThreshold = ccdMotionThreshold;
                }
            }
        }

        private float ccdSweptSphereRadius;

        [DataMember(68)]
        public float CcdSweptSphereRadius
        {
            get
            {
                return ccdSweptSphereRadius;
            }
            set
            {
                ccdSweptSphereRadius = value;

                if (NativeCollisionObject != null)
                {
                    NativeCollisionObject.CcdSweptSphereRadius = ccdSweptSphereRadius;
                }
            }
        }

        #region Ignore or Private/Internal

        [DataMemberIgnore]
        public TrackingCollection<Collision> Collisions { get; } = new TrackingCollection<Collision>();

        [DataMemberIgnore]
        internal Channel<Collision> NewPairChannel;

        public ChannelMicroThreadAwaiter<Collision> NewCollision()
        {
            return NewPairChannel.Receive();
        }

        [DataMemberIgnore]
        internal Channel<Collision> PairEndedChannel;

        public ChannelMicroThreadAwaiter<Collision> CollisionEnded()
        {
            return PairEndedChannel.Receive();
        }

        [DataMemberIgnore]
        public Simulation Simulation { get; internal set; }

        [DataMemberIgnore]
        internal PhysicsShapesRenderingService DebugShapeRendering;

        [DataMemberIgnore]
        public bool ColliderShapeChanged { get; private set; }

        [DataMemberIgnore]
        protected ColliderShape ProtectedColliderShape;

        [DataMemberIgnore]
        public virtual ColliderShape ColliderShape
        {
            get
            {
                return ProtectedColliderShape;
            }
            set
            {
                ProtectedColliderShape = value;
                if (NativeCollisionObject != null) NativeCollisionObject.CollisionShape = value.InternalShape;               
            }
        }

        [DataMemberIgnore]
        public bool CanScaleShape { get; private set; }

        [DataMemberIgnore]
        public Matrix PhysicsWorldTransform
        {
            get
            {
                return NativeCollisionObject.WorldTransform;
            }
            set
            {
                NativeCollisionObject.WorldTransform = value;
            }
        }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        [DataMemberIgnore]
        public string Tag { get; set; }

        [DataMemberIgnore]
        public Matrix BoneWorldMatrix;

        [DataMemberIgnore]
        public Matrix BoneWorldMatrixOut;

        [DataMemberIgnore]
        public int BoneIndex = -1;

        [DataMemberIgnore]
        protected PhysicsProcessor.AssociatedData Data { get; set; }

        [DataMemberIgnore]
        public Entity DebugEntity { get; set; }

        public void AddDebugEntity(Scene scene)
        {
            if (DebugEntity != null) return;

            var entity = Data?.PhysicsComponent?.DebugShapeRendering?.CreateDebugEntity(this);
            DebugEntity = entity;

            if (DebugEntity == null) return;

            scene.Entities.Add(entity);
        }

        public void RemoveDebugEntity(Scene scene)
        {
            if (DebugEntity == null) return;

            scene.Entities.Remove(DebugEntity);
            DebugEntity = null;
        }

        #endregion Ignore or Private/Internal

        #region Utility

        /// <summary>
        /// Computes the physics transformation from the TransformComponent values
        /// </summary>
        /// <returns></returns>
        internal void DerivePhysicsTransformation(out Matrix derivedTransformation)
        {
            var entity = Entity;

            Quaternion rotation;
            Vector3 translation;
            Vector3 scale;
            entity.Transform.WorldMatrix.Decompose(out scale, out rotation, out translation);

            derivedTransformation = Matrix.RotationQuaternion(rotation) * Matrix.Translation(translation);

            //handle dynamic scaling if allowed (aka not using assets)
            if (CanScaleShape)
            {
                if (ColliderShape.Scaling != scale)
                {
                    ColliderShape.Scaling = scale;
                }
            }

            //Handle collider shape offset
            if (ColliderShape.LocalOffset != Vector3.Zero || ColliderShape.LocalRotation != Quaternion.Identity)
            {
                derivedTransformation = Matrix.Multiply(ColliderShape.PositiveCenterMatrix, derivedTransformation);
            }
        }

        internal void DeriveBonePhysicsTransformation(out Matrix derivedTransformation)
        {
            Quaternion rotation;
            Vector3 translation;

            //derive rotation and translation, scale is ignored for now
            Vector3 scale;
            BoneWorldMatrix.Decompose(out scale, out rotation, out translation);

            derivedTransformation = Matrix.RotationQuaternion(rotation) * Matrix.Translation(translation);

            //handle dynamic scaling if allowed (aka not using assets)
            if (CanScaleShape)
            {
                if (ColliderShape.Scaling != scale)
                {
                    ColliderShape.Scaling = scale;
                }
            }

            //Handle collider shape offset
            if (ColliderShape.LocalOffset != Vector3.Zero || ColliderShape.LocalRotation != Quaternion.Identity)
            {
                derivedTransformation = Matrix.Multiply(ColliderShape.PositiveCenterMatrix, derivedTransformation);
            }
        }

        /// <summary>
        /// Updades the graphics transformation from the given physics transformation
        /// </summary>
        /// <param name="physicsTransform"></param>
        internal void UpdateTransformationComponent(ref Matrix physicsTransform)
        {
            var entity = Entity;

            if (ColliderShape.LocalOffset != Vector3.Zero || ColliderShape.LocalRotation != Quaternion.Identity)
            {
                physicsTransform = Matrix.Multiply(ColliderShape.NegativeCenterMatrix, physicsTransform);
            }

            //extract from physics transformation matrix
            var physTranslation = physicsTransform.TranslationVector;
            var physRotation = Quaternion.RotationMatrix(physicsTransform);

            //we need to extract scale only..
            Vector3 scale;
            scale.X = (float)Math.Sqrt((entity.Transform.WorldMatrix.M11 * entity.Transform.WorldMatrix.M11) + (entity.Transform.WorldMatrix.M12 * entity.Transform.WorldMatrix.M12) + (entity.Transform.WorldMatrix.M13 * entity.Transform.WorldMatrix.M13));
            scale.Y = (float)Math.Sqrt((entity.Transform.WorldMatrix.M21 * entity.Transform.WorldMatrix.M21) + (entity.Transform.WorldMatrix.M22 * entity.Transform.WorldMatrix.M22) + (entity.Transform.WorldMatrix.M23 * entity.Transform.WorldMatrix.M23));
            scale.Z = (float)Math.Sqrt((entity.Transform.WorldMatrix.M31 * entity.Transform.WorldMatrix.M31) + (entity.Transform.WorldMatrix.M32 * entity.Transform.WorldMatrix.M32) + (entity.Transform.WorldMatrix.M33 * entity.Transform.WorldMatrix.M33));

            TransformComponent.CreateMatrixTRS(ref physTranslation, ref physRotation, ref scale, out entity.Transform.WorldMatrix);

            if (entity.Transform.Parent == null)
            {
                entity.Transform.LocalMatrix = entity.Transform.WorldMatrix;
            }
            else
            {
                //We are not root so we need to derive the local matrix as well
                var inverseParent = entity.Transform.Parent.WorldMatrix;
                inverseParent.Invert();
                entity.Transform.LocalMatrix = Matrix.Multiply(entity.Transform.WorldMatrix, inverseParent);
            }

            Vector3 translation;
            Quaternion rotation;
            entity.Transform.LocalMatrix.Decompose(out scale, out rotation, out translation);
            entity.Transform.Position = translation;
            entity.Transform.Rotation = rotation;
        }

        internal void UpdateBoneTransformation(ref Matrix physicsTransform)
        {
            if (ColliderShape.LocalOffset != Vector3.Zero || ColliderShape.LocalRotation != Quaternion.Identity)
            {
                physicsTransform = Matrix.Multiply(ColliderShape.NegativeCenterMatrix, physicsTransform);
            }

            var rotation = Quaternion.RotationMatrix(physicsTransform);
            var translation = physicsTransform.TranslationVector;

            Vector3 scale;
            scale.X = (float)Math.Sqrt((BoneWorldMatrix.M11 * BoneWorldMatrix.M11) + (BoneWorldMatrix.M12 * BoneWorldMatrix.M12) + (BoneWorldMatrix.M13 * BoneWorldMatrix.M13));
            scale.Y = (float)Math.Sqrt((BoneWorldMatrix.M21 * BoneWorldMatrix.M21) + (BoneWorldMatrix.M22 * BoneWorldMatrix.M22) + (BoneWorldMatrix.M23 * BoneWorldMatrix.M23));
            scale.Z = (float)Math.Sqrt((BoneWorldMatrix.M31 * BoneWorldMatrix.M31) + (BoneWorldMatrix.M32 * BoneWorldMatrix.M32) + (BoneWorldMatrix.M33 * BoneWorldMatrix.M33));

            TransformComponent.CreateMatrixTRS(ref translation, ref rotation, ref scale, out BoneWorldMatrixOut);
        }

        /// <summary>
        /// Forces an update from the TransformComponent to the Collider.PhysicsWorldTransform.
        /// Useful to manually force movements.
        /// In the case of dynamic rigidbodies a velocity reset should be applied first.
        /// </summary>
        public void UpdatePhysicsTransformation()
        {
            Matrix t;
            if (BoneIndex == -1)
            {
                DerivePhysicsTransformation(out t);
            }
            else
            {
                DeriveBonePhysicsTransformation(out t);
            }
            PhysicsWorldTransform = t;
        }

        public void ComposeShape()
        {
            ColliderShapeChanged = false;

            if (ColliderShape != null)
            {
                if (!ColliderShape.IsPartOfAsset)
                {
                    ColliderShape.Dispose();
                    ColliderShape = null;
                }
                else
                {
                    ColliderShape = null;
                }
            }

            CanScaleShape = true;

            if (ColliderShapes.Count == 1) //single shape case
            {
                if (ColliderShapes[0] == null) return;
                if (ColliderShapes[0].GetType() == typeof(ColliderShapeAssetDesc))
                {
                    CanScaleShape = false;
                }

                ColliderShape = PhysicsColliderShape.CreateShape(ColliderShapes[0]);

                ColliderShape?.UpdateLocalTransformations();
            }
            else if (ColliderShapes.Count > 1) //need a compound shape in this case
            {
                var compound = new CompoundColliderShape();
                foreach (var desc in ColliderShapes)
                {
                    if (desc == null) continue;
                    if (desc.GetType() == typeof(ColliderShapeAssetDesc))
                    {
                        CanScaleShape = false;
                    }

                    var subShape = PhysicsColliderShape.CreateShape(desc);
                    if (subShape != null)
                    {
                        compound.AddChildShape(subShape);
                    }
                }

                ColliderShape = compound;

                ColliderShape.UpdateLocalTransformations();
            }
        }

        #endregion Utility

        internal void Attach(PhysicsProcessor.AssociatedData data)
        {
            Data = data;

            //this is mostly required for the game studio gizmos
            if (Simulation.DisableSimulation)
            {
                return;
            }

            //this is not optimal as UpdateWorldMatrix will end up being called twice this frame.. but we need to ensure that we have valid data.
            Entity.Transform.UpdateWorldMatrix();

            if (ColliderShapes.Count == 0)
            {
                Logger.Error($"Entity {Entity.Name} has a PhysicsComponent without any collider shape.");
                return; //no shape no purpose
            }

            if (ColliderShape == null) ComposeShape();

            if (ColliderShape == null)
            {
                Logger.Error($"Entity {Entity.Name} has a PhysicsComponent but it failed to compose the collider shape.");
                return; //no shape no purpose
            }

            BoneIndex = -1;

            OnAttach();
        }

        internal void Detach()
        {
            Data = null;

            //this is mostly required for the game studio gizmos
            if (Simulation.DisableSimulation)
            {
                return;
            }

            // Actually call the detach
            OnDetach();

            if (ColliderShape != null && !ColliderShape.IsPartOfAsset)
            {
                ColliderShape.Dispose();
            }
        }

        protected virtual void OnAttach()
        {
            //set pre-set post deserialization properties
            Enabled = base.Enabled;
            CanSleep = canSleep;
            Restitution = restitution;
            Friction = friction;
            RollingFriction = rollingFriction;
            CcdMotionThreshold = ccdMotionThreshold;
            CcdSweptSphereRadius = ccdSweptSphereRadius;
        }

        protected virtual void OnDetach()
        {
            if (NativeCollisionObject == null) return;

            NativeCollisionObject.Dispose();
            NativeCollisionObject = null;
        }

        internal void UpdateBones()
        {
            if (!Enabled)
            {
                return;
            }

            OnUpdateBones();
        }

        internal void UpdateDraw()
        {
            if (!Enabled)
            {
                return;
            }

            OnUpdateDraw();
        }

        protected internal virtual void OnUpdateDraw()
        {
        }

        protected virtual void OnUpdateBones()
        {
            //read from ModelViewHierarchy
            var model = Data.ModelComponent;
            BoneWorldMatrix = model.Skeleton.NodeTransformations[BoneIndex].WorldMatrix;
        }
    }
}
