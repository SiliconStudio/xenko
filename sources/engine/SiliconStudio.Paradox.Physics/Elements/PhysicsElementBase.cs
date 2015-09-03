using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("PhysicsSkinnedElementBase")]
    [Display(40, "PhysicsSkinnedElementBase")]
    public abstract class PhysicsSkinnedElementBase : PhysicsElementBase
    {
        private string linkedBoneName;

        /// <summary>
        /// Gets or sets the link (usually a bone).
        /// </summary>
        /// <value>
        /// The mesh's linked bone name
        /// </value>
        /// <userdoc>
        /// In the case of skinned mesh this must be the bone node name linked with this element.
        /// </userdoc>
        [DataMember(50)]
        public string LinkedBoneName
        {
            get
            {
                return linkedBoneName;
            }
            set
            {
                if (InternalCollider != null)
                {
                    throw new Exception("Cannot change LinkedBoneName when the entity is already in the scene.");
                }

                linkedBoneName = value;
            }
        }
    }

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
        [DataMember(100)]
        [Category]
        public TrackingCollection<IInlineColliderShapeDesc> ColliderShapes { get; private set; }

        [DataMemberIgnore]
        public bool ColliderShapeChanged { get; private set; }

        [DataMemberIgnore]
        public ColliderShape ColliderShape { get; private set; }

        [DataMemberIgnore]
        public bool CanScaleShape { get; private set; }

        private CollisionFilterGroups collisionGroup;

        /// <summary>
        /// Gets or sets the collision group.
        /// </summary>
        /// <value>
        /// The collision group.
        /// </value>
        /// <userdoc>
        /// The collision group of this element, default is DefaultFilter.
        /// </userdoc>
        [DataMember(30)]
        public CollisionFilterGroups CollisionGroup
        {
            get
            {
                return collisionGroup;
            }
            set
            {
                if (InternalCollider != null)
                {
                    throw new Exception("Cannot change CollisionGroup when the entity is already in the scene.");
                }

                collisionGroup = value;
            }
        }

        private CollisionFilterGroupFlags canCollideWith;

        /// <summary>
        /// Gets or sets the can collide with.
        /// </summary>
        /// <value>
        /// The can collide with.
        /// </value>
        /// <userdoc>
        /// Which collider groups this element can collide with, when nothing is selected it will collide with all groups.
        /// </userdoc>
        [DataMember(40)]
        public CollisionFilterGroupFlags CanCollideWith
        {
            get
            {
                return canCollideWith;
            }
            set
            {
                if (InternalCollider != null)
                {
                    throw new Exception("Cannot change CanCollideWith when the entity is already in the scene.");
                }

                canCollideWith = value;
            }
        }

        #region Ignore or Private/Internal

        internal Collider InternalCollider;

        [DataMemberIgnore]
        public Collider Collider
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
            }
        }

        [DataMemberIgnore]
        public RigidBody RigidBody => (RigidBody)Collider;

        [DataMemberIgnore]
        public Character Character => (Character)Collider;

        internal Matrix BoneWorldMatrix;
        internal Matrix BoneWorldMatrixOut;

        internal int BoneIndex;

        internal PhysicsProcessor.AssociatedData Data;

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

            try
            {
                CanScaleShape = true;

                if (ColliderShapes.Count == 1) //single shape case
                {
                    if (ColliderShapes[0] != null)
                    {
                        if (ColliderShapes[0].GetType() == typeof(ColliderShapeAssetDesc))
                        {
                            CanScaleShape = false;
                        }

                        ColliderShape = CreateShape(ColliderShapes[0]);

                        if (ColliderShape == null) return;

                        ColliderShape.UpdateLocalTransformations();
                    }
                }
                else if (ColliderShapes.Count > 1) //need a compound shape in this case
                {
                    var compound = new CompoundColliderShape();
                    foreach (var desc in ColliderShapes)
                    {
                        if (desc != null)
                        {
                            if (desc.GetType() == typeof(ColliderShapeAssetDesc))
                            {
                                CanScaleShape = false;
                            }

                            var subShape = CreateShape(desc);
                            if (subShape != null)
                            {
                                compound.AddChildShape(subShape);
                            }
                        }
                    }

                    ColliderShape = compound;

                    ColliderShape.UpdateLocalTransformations();
                }
            }
            catch (DllNotFoundException)
            {
                //during pre process and build process we often don't have the physics native engine running.
            }
        }

        public ColliderShape CreateShape(IInlineColliderShapeDesc desc)
        {
            ColliderShape shape = null;

            var shapeType = desc.GetType();
            if (shapeType == typeof(BoxColliderShapeDesc))
            {
                var boxDesc = (BoxColliderShapeDesc)desc;
                if (boxDesc.Is2D)
                {
                    shape = new Box2DColliderShape(new Vector2(boxDesc.Size.X, boxDesc.Size.Y)) { LocalOffset = boxDesc.LocalOffset, LocalRotation = boxDesc.LocalRotation };
                }
                else
                {
                    shape = new BoxColliderShape(boxDesc.Size) { LocalOffset = boxDesc.LocalOffset, LocalRotation = boxDesc.LocalRotation };
                }
            }
            else if (shapeType == typeof(CapsuleColliderShapeDesc))
            {
                var capsuleDesc = (CapsuleColliderShapeDesc)desc;
                shape = new CapsuleColliderShape(capsuleDesc.Is2D, capsuleDesc.Radius, capsuleDesc.Length, capsuleDesc.Orientation) { LocalOffset = capsuleDesc.LocalOffset, LocalRotation = capsuleDesc.LocalRotation };
            }
            else if (shapeType == typeof(CylinderColliderShapeDesc))
            {
                var cylinderDesc = (CylinderColliderShapeDesc)desc;
                shape = new CylinderColliderShape(cylinderDesc.Height, cylinderDesc.Radius, cylinderDesc.Orientation) { LocalOffset = cylinderDesc.LocalOffset, LocalRotation = cylinderDesc.LocalRotation };
            }
            else if (shapeType == typeof(SphereColliderShapeDesc))
            {
                var sphereDesc = (SphereColliderShapeDesc)desc;
                shape = new SphereColliderShape(sphereDesc.Is2D, sphereDesc.Radius) { LocalOffset = sphereDesc.LocalOffset };
            }
            else if (shapeType == typeof(StaticPlaneColliderShapeDesc))
            {
                var planeDesc = (StaticPlaneColliderShapeDesc)desc;
                shape = new StaticPlaneColliderShape(planeDesc.Normal, planeDesc.Offset);
            }
            else if (shapeType == typeof(ColliderShapeAssetDesc))
            {
                var assetDesc = (ColliderShapeAssetDesc)desc;

                if (assetDesc.Shape == null)
                {
                    return null;
                }

                if (assetDesc.Shape.Shape == null)
                {
                    assetDesc.Shape.Shape = PhysicsColliderShape.Compose(assetDesc.Shape.Descriptions);
                }

                shape = assetDesc.Shape.Shape;
            }

            if (shape != null)
            {
                shape.Parent = null; //from now parent might change
                shape.UpdateLocalTransformations();
            }

            return shape;
        }

        #endregion Utility
    }
}