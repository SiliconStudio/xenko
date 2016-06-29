// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract("CharacterComponent")]
    [Display("Character")]
    public sealed class CharacterComponent : PhysicsComponent
    {
        public CharacterComponent()
        {
            StepHeight = 0.1f;
        }

        /// <summary>
        /// Jumps this instance.
        /// </summary>
        public void Jump()
        {
            KinematicCharacter?.Jump();
        }

        /// <summary>
        /// Gets or sets the height of the character step.
        /// </summary>
        /// <value>
        /// The height of the character step.
        /// </value>
        /// <userdoc>
        /// Only valid for CharacterController type, describes the max slope height a character can climb. Cannot change during run-time.
        /// </userdoc>
        [DataMember(75)]
        [DefaultValue(0.1f)]
        public float StepHeight { get; set; }

        private float fallSpeed = 10.0f;

        /// <summary>
        /// Gets or sets if this character element fall speed
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The fall speed of this character
        /// </userdoc>
        [DataMember(80)]
        public float FallSpeed
        {
            get
            {
                return fallSpeed;
            }
            set
            {
                fallSpeed = value;
                
                KinematicCharacter?.SetFallSpeed(value);
            }
        }

        private float maxSlope;

        /// <summary>
        /// Gets or sets if this character element max slope
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The max slope this character can climb
        /// </userdoc>
        [DataMember(85)]
        public float MaxSlope
        {
            get
            {
                return maxSlope;
            }
            set
            {
                maxSlope = value;

                if (KinematicCharacter != null)
                {
                    KinematicCharacter.MaxSlope = value;
                }
            }
        }

        private float jumpSpeed = 5.0f;

        /// <summary>
        /// Gets or sets if this character element max slope
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The max slope this character can climb
        /// </userdoc>
        [DataMember(90)]
        public float JumpSpeed
        {
            get
            {
                return jumpSpeed;
            }
            set
            {
                jumpSpeed = value;

                KinematicCharacter?.SetJumpSpeed(value);
            }
        }

        private float gravity = -10.0f;

        /// <summary>
        /// Gets or sets if this character gravity
        /// </summary>
        /// <value>
        /// true, false
        /// </value>
        /// <userdoc>
        /// The gravity force applied to this character
        /// </userdoc>
        [DataMember(95)]
        public float Gravity
        {
            get
            {
                return gravity;
            }
            set
            {
                gravity = value;

                if (KinematicCharacter != null)
                {
                    KinematicCharacter.Gravity = -value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is on the ground.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grounded; otherwise, <c>false</c>.
        /// </value>
        public bool IsGrounded => KinematicCharacter?.OnGround() ?? false;

        /// <summary>
        /// Teleports the specified target position.
        /// </summary>
        /// <param name="targetPosition">The target position.</param>
        public void Teleport(Vector3 targetPosition)
        {
            if (KinematicCharacter == null) return;

            //we assume that the user wants to teleport in world/entity space
            var entityPos = Entity.Transform.Position;
            var physPos = PhysicsWorldTransform.TranslationVector;
            var diff = physPos - entityPos;
            KinematicCharacter.Warp(targetPosition + diff);
        }

        /// <summary>
        /// Moves the specified movement.
        /// </summary>
        /// <param name="movement">The movement.</param>
        public void Move(Vector3 movement)
        {
            KinematicCharacter?.SetWalkDirection(movement);
        }

        [DataMemberIgnore]
        internal BulletSharp.KinematicCharacterController KinematicCharacter;

        protected override void OnAttach()
        {
            NativeCollisionObject = new BulletSharp.PairCachingGhostObject
            {
                CollisionShape = ColliderShape.InternalShape,
                UserObject = this
            };

            NativeCollisionObject.CollisionFlags |= BulletSharp.CollisionFlags.CharacterObject;

            if (ColliderShape.NeedsCustomCollisionCallback)
            {
                NativeCollisionObject.CollisionFlags |= BulletSharp.CollisionFlags.CustomMaterialCallback;
            }

            NativeCollisionObject.ContactProcessingThreshold = !Simulation.CanCcd ? 1e18f : 1e30f;

            KinematicCharacter = new BulletSharp.KinematicCharacterController((BulletSharp.PairCachingGhostObject)NativeCollisionObject, (BulletSharp.ConvexShape)ColliderShape.InternalShape, StepHeight);

            base.OnAttach();

            FallSpeed = fallSpeed;
            MaxSlope = maxSlope;
            JumpSpeed = jumpSpeed;
            Gravity = gravity;

            UpdatePhysicsTransformation(); //this will set position and rotation of the collider

            Simulation.AddCharacter(this, (CollisionFilterGroupFlags)CollisionGroup, CanCollideWith);
        }

        protected override void OnDetach()
        {
            if (KinematicCharacter == null) return;

            Simulation.RemoveCharacter(this);

            KinematicCharacter.Dispose();
            KinematicCharacter = null;

            base.OnDetach();
        }
    }
}
