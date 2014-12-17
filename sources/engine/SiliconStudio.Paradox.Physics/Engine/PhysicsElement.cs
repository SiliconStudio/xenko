using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;

// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract]
    [DataConverter(AutoGenerate = true)]
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
        [DataMemberConvert]
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
        [DataMemberConvert]
        public string LinkedBoneName { get; set; }

        /// <userdoc>
        /// the Collider Shape of this element.
        /// </userdoc>
        [DataMemberConvert]
        public PhysicsColliderShape Shape { get; set; }

        //todo: is there a better way to solve this?
        public enum CollisionFilterGroups1 //needed for the editor as this is not tagged as flag...
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
        [DataMemberConvert]
        public CollisionFilterGroups1 CollisionGroup { get; set; }

        /// <summary>
        /// Gets or sets the can collide with.
        /// </summary>
        /// <value>
        /// The can collide with.
        /// </value>
        /// <userdoc>
        /// Which collider groups this element can collide with, when nothing is selected AllFilter is intended to be default.
        /// </userdoc>
        [DataMemberConvert]
        public CollisionFilterGroups CanCollideWith { get; set; }

        /// <summary>
        /// Gets or sets the height of the character step.
        /// </summary>
        /// <value>
        /// The height of the character step.
        /// </value>
        /// <userdoc>
        /// Only valid for CharacterController type, describes the max slope height a character can climb.
        /// </userdoc>
        [DataMemberConvert]
        public float StepHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PhysicsElement"/> is representing a sprite.
        /// </summary>
        /// <value>
        ///   <c>true</c> if sprite; otherwise, <c>false</c>.
        /// </value>
        /// <userdoc>
        /// If this element is associated with a Sprite Component's sprite. This is necessary because Sprites use an inverted Y axis and the physics engine must be aware of that.
        /// </userdoc>
        [DataMemberConvert]
        public bool Sprite { get; set; }

        #region Ignore or Private/Internal

        private Collider mCollider;

        [DataMemberIgnore]
        public Collider Collider
        {
            get
            {
                if (mCollider == null)
                {
                    throw new Exception("Collider is null, please make sure that you are trying to access this object after it is added to the game entities ( Entities.Add(entity) ).");
                }

                return mCollider;
            }
            internal set { mCollider = value; }
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

        internal int BoneIndex;

        internal PhysicsProcessor.AssociatedData Data;

        #endregion Ignore or Private/Internal

        #region Utility

        /// <summary>
        /// Computes the physics transformation from the TransformationComponent values
        /// </summary>
        /// <returns></returns>
        internal Matrix DerivePhysicsTransformation()
        {
            var entity = (Entity)Collider.EntityObject;

            Quaternion rotation;
            Vector3 translation;

            if (!entity.Transformation.UseTRS)
            {
                //derive rotation and translation, scale is ignored for now
                Vector3 scale;
                entity.Transformation.WorldMatrix.Decompose(out scale, out rotation, out translation);
            }
            else
            {
                rotation = entity.Transformation.Rotation;
                translation = entity.Transformation.Translation;
            }

            //Invert up axis in the case of a Sprite
            if (Sprite)
            {
                translation.Y = -translation.Y;
            }

            var physicsTransform = Matrix.RotationQuaternion(rotation) * Matrix.Translation(translation);

            //Handle collider shape offset
            if (Shape.Shape.LocalOffset != Vector3.Zero || Shape.Shape.LocalRotation != Quaternion.Identity)
            {
                physicsTransform = Matrix.Multiply(Shape.Shape.PositiveCenterMatrix, physicsTransform);
            }

            return physicsTransform;
        }

        /// <summary>
        /// Updades the graphics transformation from the given physics transformation
        /// </summary>
        /// <param name="physicsTransform"></param>
        internal void UpdateTransformationComponent(Matrix physicsTransform)
        {
            var entity = (Entity)Collider.EntityObject;

            if (Shape.Shape.LocalOffset != Vector3.Zero || Shape.Shape.LocalRotation != Quaternion.Identity)
            {
                physicsTransform = Matrix.Multiply(Shape.Shape.NegativeCenterMatrix, physicsTransform);
            }

            var rotation = Quaternion.RotationMatrix(physicsTransform);
            var translation = physicsTransform.TranslationVector;

            //Invert up axis in the case of a Sprite
            if (Sprite)
            {
                translation.Y = -translation.Y;
            }

            if (entity.Transformation.UseTRS)
            {
                entity.Transformation.Translation = translation;
                entity.Transformation.Rotation = rotation;
            }
            else
            {
                var worldMatrix = entity.Transformation.WorldMatrix;

                Vector3 scale;
                scale.X = (float)Math.Sqrt((worldMatrix.M11 * worldMatrix.M11) + (worldMatrix.M12 * worldMatrix.M12) + (worldMatrix.M13 * worldMatrix.M13));
                scale.Y = (float)Math.Sqrt((worldMatrix.M21 * worldMatrix.M21) + (worldMatrix.M22 * worldMatrix.M22) + (worldMatrix.M23 * worldMatrix.M23));
                scale.Z = (float)Math.Sqrt((worldMatrix.M31 * worldMatrix.M31) + (worldMatrix.M32 * worldMatrix.M32) + (worldMatrix.M33 * worldMatrix.M33));

                TransformationComponent.CreateMatrixTRS(ref translation, ref rotation, ref scale, out entity.Transformation.WorldMatrix);
                if (entity.Transformation.Parent == null)
                {
                    entity.Transformation.LocalMatrix = entity.Transformation.WorldMatrix;
                }
                else
                {
                    //We are not root so we need to derive the local matrix as well
                    var inverseParent = entity.Transformation.Parent.WorldMatrix;
                    inverseParent.Invert();
                    entity.Transformation.LocalMatrix = Matrix.Multiply(entity.Transformation.WorldMatrix, inverseParent);
                }
            }
        }

        /// <summary>
        /// Forces an update from the TransformationComponent to the Collider.PhysicsWorldTransform.
        /// Useful to manually force movements.
        /// In the case of dynamic rigidbodies a velocity reset should be applied first.
        /// </summary>
        public void UpdatePhysicsTransformation()
        {
            Collider.PhysicsWorldTransform = DerivePhysicsTransformation();
        }

        #endregion Utility
    }
}