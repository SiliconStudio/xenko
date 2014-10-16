// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Physics
{
    public class Character : Collider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Character"/> class.
        /// </summary>
        /// <param name="collider">The collider.</param>
        internal Character(ColliderShape collider)
            : base(collider)
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (KinematicCharacter != null)
            {
                KinematicCharacter.Dispose();
                KinematicCharacter = null;
            }

            base.Dispose();
        }

        private float mFallSpeed = 55.0f; // Terminal velocity of a sky diver in m/s. (from bullet source defaults)
        private float mJumpSpeed = 10.0f; // (from bullet source defaults)
        private Vector3 mUpAxis = Vector3.UnitY;

        internal BulletSharp.KinematicCharacterController KinematicCharacter;

        /// <summary>
        /// Gets or sets the fall speed.
        /// </summary>
        /// <value>
        /// The fall speed.
        /// </value>
        public float FallSpeed
        {
            get { return mFallSpeed; }
            set
            {
                mFallSpeed = value;
                KinematicCharacter.SetFallSpeed(mFallSpeed);
            }
        }

        /// <summary>
        /// Gets or sets the maximum slope.
        /// </summary>
        /// <value>
        /// The maximum slope.
        /// </value>
        public float MaxSlope
        {
            get { return KinematicCharacter.MaxSlope; }
            set { KinematicCharacter.MaxSlope = value; }
        }

        /// <summary>
        /// Gets or sets the jump speed.
        /// </summary>
        /// <value>
        /// The jump speed.
        /// </value>
        public float JumpSpeed
        {
            get { return mJumpSpeed; }
            set
            {
                mJumpSpeed = value;
                KinematicCharacter.SetJumpSpeed(mJumpSpeed);
            }
        }

        /// <summary>
        /// Jumps this instance.
        /// </summary>
        public void Jump()
        {
            KinematicCharacter.Jump();
        }

        /// <summary>
        /// Gets or sets up axis.
        /// </summary>
        /// <value>
        /// Up axis.
        /// </value>
        /// <exception cref="System.Exception">Invalid Up Axis.</exception>
        public Vector3 UpAxis
        {
            get { return mUpAxis; }
            set
            {
                if (value == Vector3.UnitX)
                {
                    KinematicCharacter.SetUpAxis(0);
                }
                else if (value == Vector3.UnitY)
                {
                    KinematicCharacter.SetUpAxis(1);
                }
                else if (value == Vector3.UnitZ)
                {
                    KinematicCharacter.SetUpAxis(2);
                }
                else
                {
                    throw new Exception("Invalid Up Axis.");
                }

                mUpAxis = value;
            }
        }

        /// <summary>
        /// Gets or sets the gravity.
        /// </summary>
        /// <value>
        /// The gravity.
        /// </value>
        public float Gravity
        {
            get { return -KinematicCharacter.Gravity; }
            set { KinematicCharacter.Gravity = -value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is on the ground.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grounded; otherwise, <c>false</c>.
        /// </value>
        public bool IsGrounded
        {
            get { return KinematicCharacter.OnGround(); }
        }

        /// <summary>
        /// Teleports the specified target position.
        /// </summary>
        /// <param name="targetPosition">The target position.</param>
        public void Teleport(Vector3 targetPosition)
        {
            KinematicCharacter.Warp(targetPosition);
        }

        /// <summary>
        /// Moves the specified movement.
        /// </summary>
        /// <param name="movement">The movement.</param>
        public void Move(Vector3 movement)
        {
            KinematicCharacter.SetWalkDirection(movement);
        }
    }
}
