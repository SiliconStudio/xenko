// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;

// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("PhysicsElement")]
    [Display(40, "Element")]
    public class PhysicsElement
    {
        public PhysicsElement()
        {
            CanScaleShape = true;
            ColliderShapes = new TrackingCollection<IColliderShapeDesc>();
            ColliderShapes.CollectionChanged += (sender, args) =>
            {
                ColliderShapeChanged = true;
            };
        }

        public enum Types
        {
            [Display("Trigger")]
            PhantomCollider,
            StaticCollider,
            StaticRigidBody,
            DynamicRigidBody,
            KinematicRigidBody,
            CharacterController
        };

        private Types type;

        /// <userdoc>
        /// The physics type of this element.
        /// </userdoc>
        [DataMember(10)]
        public Types Type
        {
            get
            {
                return type;
            }
            set
            {
                if (InternalCollider != null)
                {
                    switch (value)
                    {
                        case Types.PhantomCollider:
                            switch (type)
                            {
                                case Types.PhantomCollider:
                                    break;

                                case Types.StaticCollider:
                                    InternalCollider.IsTrigger = true;
                                    break;

                                case Types.StaticRigidBody:
                                case Types.DynamicRigidBody:
                                case Types.KinematicRigidBody:
                                    throw new Exception("Cannot change a RigidBody type to PhantomCollider when the entity is already in the scene. If you need this behavior please programmatically manage and set the Collider object.");
                                case Types.CharacterController:
                                    throw new Exception("Cannot change type CharacterController to PhantomCollider when the entity is already in the scene. If you need this behavior please programmatically manage and set the Collider object.");
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;

                        case Types.StaticCollider:
                            switch (type)
                            {
                                case Types.PhantomCollider:
                                    InternalCollider.IsTrigger = false;
                                    break;

                                case Types.StaticCollider:
                                    break;

                                case Types.StaticRigidBody:
                                case Types.DynamicRigidBody:
                                case Types.KinematicRigidBody:
                                    throw new Exception("Cannot change a RigidBody type to a simple Collider type when the entity is already in the scene. If you need this behavior please programmatically manage and set the Collider object or use StaticRigidBody instead.");
                                case Types.CharacterController:
                                    throw new Exception("Cannot change type CharacterController to StaticCollider when the entity is already in the scene. If you need this behavior please programmatically manage and set the Collider object.");
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;

                        case Types.StaticRigidBody:
                            switch (type)
                            {
                                case Types.PhantomCollider:
                                case Types.StaticCollider:
                                    throw new Exception("Cannot change a simple Collider type to a RigidBody type when the entity is already in the scene. If you need this behavior please programmatically manage and set the Collider object.");
                                case Types.StaticRigidBody:
                                case Types.DynamicRigidBody:
                                case Types.KinematicRigidBody:
                                    RigidBody.Type = RigidBodyTypes.Static;
                                    break;

                                case Types.CharacterController:
                                    throw new Exception("Cannot change type CharacterController to a RigidBody type when the entity is already in the scene. If you need this behavior please programmatically manage and set the Collider object.");
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;

                        case Types.DynamicRigidBody:
                            switch (type)
                            {
                                case Types.PhantomCollider:
                                case Types.StaticCollider:
                                    throw new Exception("Cannot change a simple Collider type to a RigidBody type when the entity is already in the scene. If you need this behavior please programmatically manage and set the Collider object.");
                                case Types.StaticRigidBody:
                                case Types.DynamicRigidBody:
                                case Types.KinematicRigidBody:
                                    RigidBody.Type = RigidBodyTypes.Dynamic;
                                    break;

                                case Types.CharacterController:
                                    throw new Exception("Cannot change type CharacterController to a RigidBody type when the entity is already in the scene. If you need this behavior please programmatically manage and set the Collider object.");
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;

                        case Types.KinematicRigidBody:
                            switch (type)
                            {
                                case Types.PhantomCollider:
                                case Types.StaticCollider:
                                    throw new Exception("Cannot change a simple Collider type to a RigidBody type when the entity is already in the scene. If you need this behavior please programmatically manage and set the Collider object.");
                                case Types.StaticRigidBody:
                                case Types.DynamicRigidBody:
                                case Types.KinematicRigidBody:
                                    RigidBody.Type = RigidBodyTypes.Kinematic;
                                    break;

                                case Types.CharacterController:
                                    throw new Exception("Cannot change type CharacterController to a RigidBody type when the entity is already in the scene. If you need this behavior please programmatically manage and set the Collider object.");
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;

                        case Types.CharacterController:
                            throw new Exception("Cannot change to CharacterController type when the entity is already in the scene. If you need this behavior please programmatically manage and set the Collider object.");
                        default:
                            throw new ArgumentOutOfRangeException("value", value, null);
                    }
                }

                type = value;
            }
        }

        /// <userdoc>
        /// The reference to the collider Shape of this element.
        /// </userdoc>
        [DataMember(20)]
        [Category]
        public TrackingCollection<IColliderShapeDesc> ColliderShapes { get; private set; }

        public bool ColliderShapeChanged { get; private set; }

        [DataMemberIgnore]
        public ColliderShape ColliderShape { get; private set; }

        [DataMemberIgnore]
        public bool CanScaleShape { get; private set; }

        private Vector3 currentPhysicsScaling;

        public enum CollisionFilterGroups //needed for the editor as this is not tagged as flag...
        {
            DefaultFilter = 0x1,

            StaticFilter = 0x2,

            KinematicFilter = 0x4,

            DebrisFilter = 0x8,

            SensorTrigger = 0x10,

            CharacterFilter = 0x20,

            CustomFilter1 = 0x40,

            CustomFilter2 = 0x80,

            CustomFilter3 = 0x100,

            CustomFilter4 = 0x200,

            CustomFilter5 = 0x400,

            CustomFilter6 = 0x800,

            CustomFilter7 = 0x1000,

            CustomFilter8 = 0x2000,

            CustomFilter9 = 0x4000,

            CustomFilter10 = 0x8000,

            AllFilter = 0xFFFF
        }

        private CollisionFilterGroups collisionGroup;

        /// <summary>
        /// Gets or sets the collision group.
        /// </summary>
        /// <value>
        /// The collision group.
        /// </value>
        /// <userdoc>
        /// The collision group of this element, default is AllFilter.
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
        /// Which collider groups this element can collide with, when nothing is selected AllFilter is intended to be default.
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

        private float stepHeight;

        /// <summary>
        /// Gets or sets the height of the character step.
        /// </summary>
        /// <value>
        /// The height of the character step.
        /// </value>
        /// <userdoc>
        /// Only valid for CharacterController type, describes the max slope height a character can climb.
        /// </userdoc>
        [DataMember(60)]
        public float StepHeight
        {
            get
            {
                return stepHeight;
            }
            set
            {
                if (InternalCollider != null)
                {
                    throw new Exception("Cannot change StepHeight when the entity is already in the scene.");
                }

                stepHeight = value;
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
            internal set { InternalCollider = value; }
        }

        [DataMemberIgnore]
        public RigidBody RigidBody
        {
            get { return (RigidBody)Collider; }
        }

        [DataMemberIgnore]
        public Character Character
        {
            get { return (Character)Collider; }
        }

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
                var newScaling = ColliderShape.Scaling * scale;
                if (newScaling != currentPhysicsScaling)
                {
                    ColliderShape.Scaling = newScaling;
                    currentPhysicsScaling = newScaling;
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

            //Handle collider shape offset
            if (ColliderShape.LocalOffset != Vector3.Zero || ColliderShape.LocalRotation != Quaternion.Identity)
            {
                derivedTransformation = Matrix.Multiply(ColliderShape.PositiveCenterMatrix, derivedTransformation);
            }

            //handle dynamic scaling if allowed (aka not using assets)
            if (CanScaleShape)
            {
                var newScaling = ColliderShape.Scaling * scale;
                if (newScaling != currentPhysicsScaling)
                {
                    ColliderShape.Scaling = newScaling;
                    currentPhysicsScaling = newScaling;
                }
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
                ColliderShape.Dispose();
                ColliderShape = null;
            }

            try
            {
                if (ColliderShapes.Count == 1) //single shape case
                {
                    if (ColliderShapes[0] != null)
                    {
                        ColliderShape = CreateShape(ColliderShapes[0]);
                        
                        if (ColliderShape == null) return;

                        currentPhysicsScaling = ColliderShape.Scaling;
                    }
                }
                else if (ColliderShapes.Count > 1) //need a compound shape in this case
                {
                    var compound = new CompoundColliderShape();
                    foreach (var desc in ColliderShapes)
                    {
                        if (desc != null)
                        {
                            compound.AddChildShape(CreateShape(desc));
                        }
                    }
                    ColliderShape = compound;
                    currentPhysicsScaling = ColliderShape.Scaling;
                }
            }
            catch (DllNotFoundException)
            {
                //during pre process and build process we often don't have the physics native engine running.
            }
        }

        public ColliderShape CreateShape(IColliderShapeDesc desc)
        {
            ColliderShape shape = null;

            var type = desc.GetType();
            if (type == typeof(Box2DColliderShapeDesc))
            {
                var boxDesc = (Box2DColliderShapeDesc)desc;
                shape = new Box2DColliderShape(boxDesc.Size) { LocalOffset = boxDesc.LocalOffset, LocalRotation = boxDesc.LocalRotation };
            }
            else if (type == typeof(BoxColliderShapeDesc))
            {
                var boxDesc = (BoxColliderShapeDesc)desc;
                shape = new BoxColliderShape(boxDesc.Size) { LocalOffset = boxDesc.LocalOffset, LocalRotation = boxDesc.LocalRotation };
            }
            else if (type == typeof(CapsuleColliderShapeDesc))
            {
                var capsuleDesc = (CapsuleColliderShapeDesc)desc;
                shape = new CapsuleColliderShape(capsuleDesc.Is2D, capsuleDesc.Radius, capsuleDesc.Length, capsuleDesc.Orientation) { LocalOffset = capsuleDesc.LocalOffset, LocalRotation = capsuleDesc.LocalRotation };
            }
            else if (type == typeof(CylinderColliderShapeDesc))
            {
                var cylinderDesc = (CylinderColliderShapeDesc)desc;
                shape = new CylinderColliderShape(cylinderDesc.Height, cylinderDesc.Radius, cylinderDesc.Orientation) { LocalOffset = cylinderDesc.LocalOffset, LocalRotation = cylinderDesc.LocalRotation };
            }
            else if (type == typeof(SphereColliderShapeDesc))
            {
                var sphereDesc = (SphereColliderShapeDesc)desc;
                shape = new SphereColliderShape(sphereDesc.Is2D, sphereDesc.Radius) { LocalOffset = sphereDesc.LocalOffset };
            }
            else if (type == typeof(StaticPlaneColliderShapeDesc))
            {
                var planeDesc = (StaticPlaneColliderShapeDesc)desc;
                shape = new StaticPlaneColliderShape(planeDesc.Normal, planeDesc.Offset);
            }
            else if (type == typeof(ConvexHullColliderShapeDesc))
            {
                var convexDesc = (ConvexHullColliderShapeDesc)desc;

                if (convexDesc.ConvexHulls == null) return null;

                //Optimize performance and focus on less shapes creation since this shape could be nested

                if (convexDesc.ConvexHulls.Count == 1)
                {
                    if (convexDesc.ConvexHulls[0].Count == 1)
                    {
                        shape = new ConvexHullColliderShape(convexDesc.ConvexHulls[0][0], convexDesc.ConvexHullsIndices[0][0], convexDesc.Scaling)
                        {
                            NeedsCustomCollisionCallback = true
                        };

                        shape.UpdateLocalTransformations();
                        shape.Description = desc;

                        return shape;
                    }

                    if (convexDesc.ConvexHulls[0].Count <= 1) return null;

                    var subCompound = new CompoundColliderShape
                    {
                        NeedsCustomCollisionCallback = true
                    };

                    for (var i = 0; i < convexDesc.ConvexHulls[0].Count; i++)
                    {
                        var verts = convexDesc.ConvexHulls[0][i];
                        var indices = convexDesc.ConvexHullsIndices[0][i];

                        var subHull = new ConvexHullColliderShape(verts, indices, convexDesc.Scaling);
                        subHull.UpdateLocalTransformations();
                        subCompound.AddChildShape(subHull);
                    }

                    subCompound.UpdateLocalTransformations();
                    subCompound.Description = desc;

                    return subCompound;
                }

                if (convexDesc.ConvexHulls.Count <= 1) return null;

                var compound = new CompoundColliderShape
                {
                    NeedsCustomCollisionCallback = true
                };

                for (var i = 0; i < convexDesc.ConvexHulls.Count; i++)
                {
                    var verts = convexDesc.ConvexHulls[i];
                    var indices = convexDesc.ConvexHullsIndices[i];

                    if (verts.Count == 1)
                    {
                        var subHull = new ConvexHullColliderShape(verts[0], indices[0], convexDesc.Scaling);
                        subHull.UpdateLocalTransformations();
                        compound.AddChildShape(subHull);
                    }
                    else if (verts.Count > 1)
                    {
                        var subCompound = new CompoundColliderShape();

                        for (var b = 0; b < verts.Count; b++)
                        {
                            var subVerts = verts[b];
                            var subIndex = indices[b];

                            var subHull = new ConvexHullColliderShape(subVerts, subIndex, convexDesc.Scaling);
                            subHull.UpdateLocalTransformations();
                            subCompound.AddChildShape(subHull);
                        }

                        subCompound.UpdateLocalTransformations();

                        compound.AddChildShape(subCompound);
                    }
                }

                compound.UpdateLocalTransformations();
                compound.Description = desc;

                return compound;
            }
            else if (type == typeof(ColliderShapeAssetDesc))
            {
                var assetDesc = (ColliderShapeAssetDesc)desc;

                if (assetDesc.Shape == null) return null;

                shape = assetDesc.Shape.Shape;
                CanScaleShape = false; //if any shape asset is present we should not scale the shape automatically!
            }

            if (shape != null)
            {
                shape.UpdateLocalTransformations();
                shape.Description = desc;
            }

            return shape;
        }

        #endregion Utility
    }
}