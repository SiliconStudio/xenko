// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;

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
            ColliderShape = collider;
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

        bool enabled = true;
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
                    InternalCollider.ActivationState = canSleep ? BulletSharp.ActivationState.ActiveTag : BulletSharp.ActivationState.DisableDeactivation;
                }
                else
                {
                    InternalCollider.ActivationState = BulletSharp.ActivationState.DisableSimulation;
                }
            }
        }

        bool canSleep = true; //default true
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
        public bool IsActive
        {
            get
            {
                return InternalCollider.IsActive;
            }
        }

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
            get
            {
                return (InternalCollider.CollisionFlags & BulletSharp.CollisionFlags.NoContactResponse) != 0;
            }
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

        /// <summary>
        /// Gets the collider shape.
        /// </summary>
        /// <value>
        /// The collider shape.
        /// </value>
        public ColliderShape ColliderShape { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Contacts list needs to be always valid.
        /// This is used to improve performance in the case the list is not needed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [contacts always valid]; otherwise, <c>false</c>.
        /// </value>
        public bool ContactsAlwaysValid { get; set; }

        int eventUsers; //this helps optimize performance

        internal bool NeedsCollisionCheck
        {
            get
            {
                return ContactsAlwaysValid || eventUsers > 0;
            }
        }

        readonly object eventsLock = new Object();

        event EventHandler<CollisionArgs> PrivateFirstContactBegin;

        /// <summary>
        /// Occurs when the first contant with a collider begins.
        /// </summary>
        public event EventHandler<CollisionArgs> FirstContactStart
        {
            add
            {
                lock (eventsLock)
                {
                    eventUsers++;
                    PrivateFirstContactBegin += value;
                }
            }
            remove
            {
                lock (eventsLock)
                {
                    eventUsers--;
                    PrivateFirstContactBegin -= value;
                }
            }
        }

        internal void OnFirstContactStart(CollisionArgs args)
        {
            var e = PrivateFirstContactBegin;
            if (e == null) return;
            e(this, args);
        }

        event EventHandler<CollisionArgs> PrivateContactStart;

        /// <summary>
        /// Occurs when a contact begins (there could be multiple contacts and contact points).
        /// </summary>
        public event EventHandler<CollisionArgs> ContactStart
        {
            add
            {
                lock (eventsLock)
                {
                    eventUsers++;
                    PrivateContactStart += value;
                }
            }
            remove
            {
                lock (eventsLock)
                {
                    eventUsers--;
                    PrivateContactStart -= value;
                }
            }
        }

        internal void OnContactStart(CollisionArgs args)
        {
            var e = PrivateContactStart;
            if (e == null) return;
            e(this, args);
        }

        event EventHandler<CollisionArgs> PrivateContactChange;

        /// <summary>
        /// Occurs when a contact changed.
        /// </summary>
        public event EventHandler<CollisionArgs> ContactChange
        {
            add
            {
                lock (eventsLock)
                {
                    eventUsers++;
                    PrivateContactChange += value;
                }
            }
            remove
            {
                lock (eventsLock)
                {
                    eventUsers--;
                    PrivateContactChange -= value;
                }
            }
        }

        internal void OnContactChange(CollisionArgs args)
        {
            var e = PrivateContactChange;
            if (e == null) return;
            e(this, args);
        }

        event EventHandler<CollisionArgs> PrivateLastContactEnd;
        /// <summary>
        /// Occurs when the last contact with a collider happened.
        /// </summary>
        public event EventHandler<CollisionArgs> LastContactEnd
        {
            add
            {
                lock (eventsLock)
                {
                    eventUsers++;
                    PrivateLastContactEnd += value;
                }
            }
            remove
            {
                lock (eventsLock)
                {
                    eventUsers--;
                    PrivateLastContactEnd -= value;
                }
            }
        }

        internal void OnLastContactEnd(CollisionArgs args)
        {
            var e = PrivateLastContactEnd;
            if (e == null) return;
            e(this, args);
        }

        event EventHandler<CollisionArgs> PrivateContactEnd;

        /// <summary>
        /// Occurs when a contact ended.
        /// </summary>
        public event EventHandler<CollisionArgs> ContactEnd
        {
            add
            {
                lock (eventsLock)
                {
                    eventUsers++;
                    PrivateContactEnd += value;
                }
            }
            remove
            {
                lock (eventsLock)
                {
                    eventUsers--;
                    PrivateContactEnd -= value;
                }
            }
        }

        internal void OnContactEnd(CollisionArgs args)
        {
            var e = PrivateContactEnd;
            if (e == null) return;
            e(this, args);
        }

        readonly FastList<Contact> contacts = new FastList<Contact>();
        /// <summary>
        /// Gets the contacts.
        /// </summary>
        /// <value>
        /// The contacts.
        /// </value>
        public FastList<Contact> Contacts
        {
            get
            {
                return contacts;
            }
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
