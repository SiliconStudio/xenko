// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Paradox.Engine;
using System;
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Physics
{
    public class Collider : IDisposable, IRelative
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Collider"/> class.
        /// </summary>
        /// <param name="collider">The collider.</param>
        public Collider(ColliderShape collider)
        {
            ProtectedColliderShape = collider;

            FirstCollisionChannel = new Channel<Collision> { Preference = ChannelPreference.PreferSender };
            NewPairChannel = new Channel<Collision> { Preference = ChannelPreference.PreferSender };
            PairEndedChannel = new Channel<Collision> { Preference = ChannelPreference.PreferSender };
            AllPairsEndedChannel = new Channel<Collision> { Preference = ChannelPreference.PreferSender };
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (InternalCollider == null) return;

            InternalCollider.Dispose();
            InternalCollider = null;
        }

        private bool enabled = true;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Collider"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;

                if (value)
                {
                    InternalCollider.ForceActivationState(canSleep ? BulletSharp.ActivationState.ActiveTag : BulletSharp.ActivationState.DisableDeactivation);
                }
                else
                {
                    InternalCollider.ForceActivationState(BulletSharp.ActivationState.DisableSimulation);
                }
            }
        }

        private bool canSleep = true; //default true

        /// <summary>
        /// Gets or sets a value indicating whether this instance can sleep.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can sleep; otherwise, <c>false</c>.
        /// </value>
        public bool CanSleep
        {
            get
            {
                return canSleep;
            }
            set
            {
                canSleep = value;

                if (enabled)
                {
                    InternalCollider.ActivationState = value ? BulletSharp.ActivationState.ActiveTag : BulletSharp.ActivationState.DisableDeactivation;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active (awake).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive => InternalCollider.IsActive;

        /// <summary>
        /// Attempts to awake the collider.
        /// </summary>
        /// <param name="forceActivation">if set to <c>true</c> [force activation].</param>
        public void Activate(bool forceActivation = false)
        {
            InternalCollider.Activate(forceActivation);
        }

        /// <summary>
        /// Gets or sets the restitution.
        /// </summary>
        /// <value>
        /// The restitution.
        /// </value>
        public float Restitution
        {
            get
            {
                return InternalCollider.Restitution;
            }
            set
            {
                InternalCollider.Restitution = value;
            }
        }

        /// <summary>
        /// Gets or sets the friction.
        /// </summary>
        /// <value>
        /// The friction.
        /// </value>
        public float Friction
        {
            get
            {
                return InternalCollider.Friction;
            }
            set
            {
                InternalCollider.Friction = value;
            }
        }

        /// <summary>
        /// Gets or sets the rolling friction.
        /// </summary>
        /// <value>
        /// The rolling friction.
        /// </value>
        public float RollingFriction
        {
            get
            {
                return InternalCollider.RollingFriction;
            }
            set
            {
                InternalCollider.RollingFriction = value;
            }
        }

        /// <summary>
        /// Gets or sets the CCD motion threshold.
        /// </summary>
        /// <value>
        /// The CCD motion threshold.
        /// </value>
        public float CcdMotionThreshold
        {
            get
            {
                return InternalCollider.CcdMotionThreshold;
            }
            set
            {
                InternalCollider.CcdMotionThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the CCD swept sphere radius.
        /// </summary>
        /// <value>
        /// The CCD swept sphere radius.
        /// </value>
        public float CcdSweptSphereRadius
        {
            get
            {
                return InternalCollider.CcdSweptSphereRadius;
            }
            set
            {
                InternalCollider.CcdSweptSphereRadius = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a trigger.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is trigger; otherwise, <c>false</c>.
        /// </value>
        public bool IsTrigger
        {
            get { return InternalCollider.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.NoContactResponse); }
            set
            {
                if (value) InternalCollider.CollisionFlags |= BulletSharp.CollisionFlags.NoContactResponse;
                else if (InternalCollider.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.NoContactResponse)) InternalCollider.CollisionFlags ^= BulletSharp.CollisionFlags.NoContactResponse;
            }
        }

        internal BulletSharp.CollisionObject InternalCollider;

        /// <summary>
        /// Gets the physics world transform.
        /// </summary>
        /// <value>
        /// The physics world transform.
        /// </value>
        public Matrix PhysicsWorldTransform
        {
            get
            {
                return InternalCollider.WorldTransform;
            }
            set
            {
                InternalCollider.WorldTransform = value;
            }
        }

        protected ColliderShape ProtectedColliderShape;

        /// <summary>
        /// Gets the collider shape.
        /// </summary>
        /// <value>
        /// The collider shape.
        /// </value>
        public virtual ColliderShape ColliderShape
        {
            get
            {
                return ProtectedColliderShape;
            }
            set
            {
                if (InternalCollider != null) InternalCollider.CollisionShape = value.InternalShape;
                ProtectedColliderShape = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Contacts list needs to be always valid.
        /// This is used to improve performance in the case the list is not needed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [contacts always valid]; otherwise, <c>false</c>.
        /// </value>
        public bool ContactsAlwaysValid { get; set; }

        /// <summary>
        /// Gets the contacts.
        /// </summary>
        /// <value>
        /// The contacts.
        /// </value>
        public List<Collision> Collisions { get; } = new List<Collision>();

        internal Channel<Collision> FirstCollisionChannel;

        public ChannelMicroThreadAwaiter<Collision> FirstCollision()
        {
            return FirstCollisionChannel.Receive();
        }

        internal Channel<Collision> NewPairChannel;

        public ChannelMicroThreadAwaiter<Collision> NewCollision()
        {
            return NewPairChannel.Receive();
        }

        internal Channel<Collision> PairEndedChannel;

        public ChannelMicroThreadAwaiter<Collision> CollisionEnded()
        {
            return PairEndedChannel.Receive();
        }

        internal Channel<Collision> AllPairsEndedChannel;

        public ChannelMicroThreadAwaiter<Collision> AllCollisionsEnded()
        {
            return AllPairsEndedChannel.Receive();
        }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public string Tag { get; set; }

        /// <summary>
        /// Gets the entity linked with this Collider
        /// </summary>
        public Entity Entity { get; internal set; }

        /// <summary>
        /// Gets the Simulation where this Collider is being processed
        /// </summary>
        public Simulation Simulation { get; internal set; }
    }
}