using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;

// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("PhysicsElement")]
    [Display(40, "Element")]
    public class PhysicsElement
    {
        public enum Types
        {
            PhantomCollider,
            StaticCollider,
            StaticRigidBody,
            DynamicRigidBody,
            KinematicRigidBody,
            CharacterController
        };

        /// <userdoc>
        /// The physics type of this element. 
        /// </userdoc>
        public Types Type { get; set; }

        /// <summary>
        /// Gets or sets the link (usually a bone).
        /// </summary>
        /// <value>
        /// The mesh's linked bone name
        /// </value>
        /// <userdoc>
        /// In the case of skinned mesh this must be the bone node name linked with this element.
        /// </userdoc>
        public string LinkedBoneName { get; set; }

        /// <userdoc>
        /// the Collider Shape of this element.
        /// </userdoc>
        public PhysicsColliderShape Shape { get; set; }

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

        /// <summary>
        /// Gets or sets the collision group.
        /// </summary>
        /// <value>
        /// The collision group.
        /// </value>
        /// <userdoc>
        /// The collision group of this element, default is AllFilter.
        /// </userdoc>
        public CollisionFilterGroups CollisionGroup { get; set; }

        /// <summary>
        /// Gets or sets the can collide with.
        /// </summary>
        /// <value>
        /// The can collide with.
        /// </value>
        /// <userdoc>
        /// Which collider groups this element can collide with, when nothing is selected AllFilter is intended to be default.
        /// </userdoc>
        public CollisionFilterGroupFlags CanCollideWith { get; set; }

        /// <summary>
        /// Gets or sets the height of the character step.
        /// </summary>
        /// <value>
        /// The height of the character step.
        /// </value>
        /// <userdoc>
        /// Only valid for CharacterController type, describes the max slope height a character can climb.
        /// </userdoc>
        public float StepHeight { get; set; }

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

            if (!entity.Transform.UseTRS)
            {
                //derive rotation and translation, scale is ignored for now
                Vector3 scale;
                entity.Transform.WorldMatrix.Decompose(out scale, out rotation, out translation);
            }
            else
            {
                rotation = entity.Transform.Rotation;
                translation = entity.Transform.Position;
            }

            derivedTransformation = Matrix.RotationQuaternion(rotation) * Matrix.Translation(translation);

            //Handle collider shape offset
            if (Shape.Shape.LocalOffset != Vector3.Zero || Shape.Shape.LocalRotation != Quaternion.Identity)
            {
                derivedTransformation = Matrix.Multiply(Shape.Shape.PositiveCenterMatrix, derivedTransformation);
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
            if (Shape.Shape.LocalOffset != Vector3.Zero || Shape.Shape.LocalRotation != Quaternion.Identity)
            {
                derivedTransformation = Matrix.Multiply(Shape.Shape.PositiveCenterMatrix, derivedTransformation);
            }
        }

        /// <summary>
        /// Updades the graphics transformation from the given physics transformation
        /// </summary>
        /// <param name="physicsTransform"></param>
        internal void UpdateTransformationComponent(Matrix physicsTransform)
        {
            var entity = Collider.Entity;

            if (Shape.Shape.LocalOffset != Vector3.Zero || Shape.Shape.LocalRotation != Quaternion.Identity)
            {
                physicsTransform = Matrix.Multiply(Shape.Shape.NegativeCenterMatrix, physicsTransform);
            }

            var rotation = Quaternion.RotationMatrix(physicsTransform);
            var translation = physicsTransform.TranslationVector;

            if (entity.Transform.UseTRS)
            {
                entity.Transform.Position = translation;
                entity.Transform.Rotation = rotation;
            }
            else
            {
                var worldMatrix = entity.Transform.WorldMatrix;

                Vector3 scale;
                scale.X = (float)Math.Sqrt((worldMatrix.M11 * worldMatrix.M11) + (worldMatrix.M12 * worldMatrix.M12) + (worldMatrix.M13 * worldMatrix.M13));
                scale.Y = (float)Math.Sqrt((worldMatrix.M21 * worldMatrix.M21) + (worldMatrix.M22 * worldMatrix.M22) + (worldMatrix.M23 * worldMatrix.M23));
                scale.Z = (float)Math.Sqrt((worldMatrix.M31 * worldMatrix.M31) + (worldMatrix.M32 * worldMatrix.M32) + (worldMatrix.M33 * worldMatrix.M33));

                TransformComponent.CreateMatrixTRS(ref translation, ref rotation, ref scale, out entity.Transform.WorldMatrix);
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
            }
        }

        internal void UpdateBoneTransformation(Matrix physicsTransform)
        {
            if (Shape.Shape.LocalOffset != Vector3.Zero || Shape.Shape.LocalRotation != Quaternion.Identity)
            {
                physicsTransform = Matrix.Multiply(Shape.Shape.NegativeCenterMatrix, physicsTransform);
            }

            var rotation = Quaternion.RotationMatrix(physicsTransform);
            var translation = physicsTransform.TranslationVector;

            var worldMatrix = BoneWorldMatrix;

            Vector3 scale;
            scale.X = (float)Math.Sqrt((worldMatrix.M11 * worldMatrix.M11) + (worldMatrix.M12 * worldMatrix.M12) + (worldMatrix.M13 * worldMatrix.M13));
            scale.Y = (float)Math.Sqrt((worldMatrix.M21 * worldMatrix.M21) + (worldMatrix.M22 * worldMatrix.M22) + (worldMatrix.M23 * worldMatrix.M23));
            scale.Z = (float)Math.Sqrt((worldMatrix.M31 * worldMatrix.M31) + (worldMatrix.M32 * worldMatrix.M32) + (worldMatrix.M33 * worldMatrix.M33));

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

        #endregion Utility
    }
}