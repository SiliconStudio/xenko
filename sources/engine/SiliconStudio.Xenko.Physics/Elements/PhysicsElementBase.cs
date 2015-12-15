using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract("PhysicsElementBase")]
    [Display(40, "PhysicsElementBase")]
    public abstract class PhysicsElementBase
    {
        protected PhysicsElementBase()
        {
            CanScaleShape = true;

            ColliderShapes = new TrackingCollection<IInlineColliderShapeDesc>();
            ColliderShapes.CollectionChanged += (sender, args) =>
            {
                ColliderShapeChanged = true;
            };
        }

        public enum Types
        {
            /// <userdoc>
            /// A static trigger zone
            /// </userdoc>
            [Display("Trigger")]
            PhantomCollider,
            /// <userdoc>
            /// A static Collider
            /// </userdoc>
            [Display("Static Collider")]
            StaticCollider,
            /// <userdoc>
            /// A static RigidBody
            /// </userdoc>
            [Display("Static RigidBody")]
            StaticRigidBody,
            /// <userdoc>
            /// A dynamic RigidBody
            /// </userdoc>
            [Display("Dynamic RigidBody")]
            DynamicRigidBody,
            /// <userdoc>
            /// A kinematic RigidBody
            /// </userdoc>
            [Display("Kinematic RigidBody")]
            KinematicRigidBody,
            /// <userdoc>
            /// A Character Controller
            /// </userdoc>
            [Display("Character")]
            CharacterController
        };

        public abstract Types Type { get; }

        /// <userdoc>
        /// The reference to the collider Shape of this element.
        /// </userdoc>
        [DataMember(200)]
        [Category]
        public TrackingCollection<IInlineColliderShapeDesc> ColliderShapes { get; private set; }

        [DataMemberIgnore]
        public bool ColliderShapeChanged { get; private set; }

        [DataMemberIgnore]
        public ColliderShape ColliderShape { get; private set; }

        [DataMemberIgnore]
        public bool CanScaleShape { get; private set; }

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
        public CollisionFilterGroups CollisionGroup { get; set; }

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
        public CollisionFilterGroupFlags CanCollideWith { get; set; }

        private bool processCollisions = true;

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
        public virtual bool ProcessCollisions
        {
            get
            {
                return InternalCollider?.ContactsAlwaysValid ?? processCollisions;
            }
            set
            {
                if (InternalCollider != null)
                {
                    InternalCollider.ContactsAlwaysValid = value;
                }
                else
                {
                    processCollisions = value;
                }
            }
        }

        private bool enabled = true;

        /// <summary>
        /// Gets or sets if this element is enabled in the physics engine
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// If this element is enabled in the physics engine
        /// </userdoc>
        [DataMember(50)]
        public bool Enabled
        {
            get
            {
                return InternalCollider?.Enabled ?? enabled;
            }
            set
            {
                if (InternalCollider != null)
                {
                    InternalCollider.Enabled = value;
                }
                else
                {
                    enabled = value;
                }
            }
        }

        private bool canSleep = false;

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
                return InternalCollider?.CanSleep ?? canSleep;
            }
            set
            {
                if (InternalCollider != null)
                {
                    InternalCollider.CanSleep = value;
                }
                else
                {
                    canSleep = value;
                }
            }
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
                return InternalCollider?.Restitution ?? restitution;
            }
            set
            {
                if (InternalCollider != null)
                {
                    InternalCollider.Restitution = value;
                }
                else
                {
                    restitution = value;
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
                return InternalCollider?.Friction ?? friction;
            }
            set
            {
                if (InternalCollider != null)
                {
                    InternalCollider.Friction = value;
                }
                else
                {
                    friction = value;
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
        [DataMember(70)]
        public float RollingFriction
        {
            get
            {
                return InternalCollider?.RollingFriction ?? rollingFriction;
            }
            set
            {
                if (InternalCollider != null)
                {
                    InternalCollider.RollingFriction = value;
                }
                else
                {
                    rollingFriction = value;
                }
            }
        }

        #region Ignore or Private/Internal

        internal Collider InternalCollider;

        [DataMemberIgnore]
        public virtual Collider Collider
        {
            get
            {
                if (InternalCollider == null)
                {
                    throw new Exception("Collider is null, please make sure that you are trying to access this object after it is added to the game entities ( Entities.Add(entity) ).");
                }

                return InternalCollider;
            }
            internal set
            {
                InternalCollider = value;
                //set possibly pre-set properties
                ProcessCollisions = processCollisions;
                Enabled = enabled;
                CanSleep = canSleep;
                Restitution = restitution;
                Friction = friction;
                RollingFriction = rollingFriction;
            }
        }

        [DataMemberIgnore]
        public RigidBody RigidBody => Collider as RigidBody;

        [DataMemberIgnore]
        public Character Character => Collider as Character;

        [DataMemberIgnore]
        public Matrix BoneWorldMatrix;

        [DataMemberIgnore]
        public Matrix BoneWorldMatrixOut;

        [DataMemberIgnore]
        public int BoneIndex = -1;

        [DataMemberIgnore]
        public PhysicsProcessor.AssociatedData Data;

        [DataMemberIgnore]
        public Entity DebugEntity;

        public void AddDebugEntity(Scene scene)
        {
            if (DebugEntity != null) return;

            var entity = Data?.PhysicsComponent?.DebugShapeRendering?.CreateDebugEntity(this);
            DebugEntity = entity;
            if (DebugEntity != null)
            {
                scene.Entities.Add(entity);
            }
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
            var entity = Collider.Entity;

            Quaternion rotation;
            Vector3 translation;
            Vector3 scale;
            entity.Transform.WorldMatrix.Decompose(out scale, out rotation, out translation);

            derivedTransformation = Matrix.RotationQuaternion(rotation) * Matrix.Translation(translation);

            //handle dynamic scaling if allowed (aka not using assets)
            if (CanScaleShape)
            {
                if (scale != ColliderShape.Scaling)
                {
                    ColliderShape.Scaling = scale;
                    ColliderShape.UpdateLocalTransformations();

                    if (DebugEntity != null)
                    {
                        DebugEntity.Transform.Scale = scale;
                    }
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
                if (scale != ColliderShape.Scaling)
                {
                    ColliderShape.Scaling = scale;
                    ColliderShape.UpdateLocalTransformations();

                    if (DebugEntity != null)
                    {
                        DebugEntity.Transform.Scale = scale;
                    }
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
            var entity = Collider.Entity;

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
            Collider.PhysicsWorldTransform = t;
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
    }
}