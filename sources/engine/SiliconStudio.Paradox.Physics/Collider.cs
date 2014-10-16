// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Physics
{
    public class Collider : IDisposable
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

        internal PhysicsEngine Engine;

        bool mEnabled = true;
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
                return mEnabled;
            }
            set
            {
                mEnabled = value;

                if (value)
                {
                    InternalCollider.ActivationState = mCanSleep ? BulletSharp.ActivationState.ActiveTag : BulletSharp.ActivationState.DisableDeactivation;
                }
                else
                {
                    InternalCollider.ActivationState = BulletSharp.ActivationState.DisableSimulation;
                }
            }
        }

        bool mCanSleep = true; //default true
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
                return mCanSleep;
            }
            set
            {
                mCanSleep = value;

                if (mEnabled)
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

        int mEventUsers; //this helps optimize performance
        internal bool NeedsCollisionCheck
        {
            get
            {
                return ContactsAlwaysValid || mEventUsers > 0;
            }
        }

        readonly object mEventsLock = new Object();

        event EventHandler<CollisionArgs> mOnFirstContactBegin;
        /// <summary>
        /// Occurs when the first contant with a collider begins.
        /// </summary>
        public event EventHandler<CollisionArgs> OnFirstContactBegin
        {
            add
            {
                lock (mEventsLock)
                {
                    mEventUsers++;
                    mOnFirstContactBegin += value;
                }
            }
            remove
            {
                lock (mEventsLock)
                {
                    mEventUsers--;
                    mOnFirstContactBegin -= value;
                }
            }
        }

        internal void PropagateOnFirstContactBegin(CollisionArgs args)
        {
            var e = mOnFirstContactBegin;
            if (e == null) return;
            e(this, args);
        }

        event EventHandler<CollisionArgs> mOnContactBegin;
        /// <summary>
        /// Occurs when a contact begins (there could be multiple contacts and contact points).
        /// </summary>
        public event EventHandler<CollisionArgs> OnContactBegin
        {
            add
            {
                lock (mEventsLock)
                {
                    mEventUsers++;
                    mOnContactBegin += value;
                }
            }
            remove
            {
                lock (mEventsLock)
                {
                    mEventUsers--;
                    mOnContactBegin -= value;
                }
            }
        }

        internal void PropagateOnContactBegin(CollisionArgs args)
        {
            var e = mOnContactBegin;
            if (e == null) return;
            e(this, args);
        }

        event EventHandler<CollisionArgs> mOnContactChange;
        /// <summary>
        /// Occurs when a contact changed.
        /// </summary>
        public event EventHandler<CollisionArgs> OnContactChange
        {
            add
            {
                lock (mEventsLock)
                {
                    mEventUsers++;
                    mOnContactChange += value;
                }
            }
            remove
            {
                lock (mEventsLock)
                {
                    mEventUsers--;
                    mOnContactChange -= value;
                }
            }
        }

        internal void PropagateOnContactChange(CollisionArgs args)
        {
            var e = mOnContactChange;
            if (e == null) return;
            e(this, args);
        }

        event EventHandler<CollisionArgs> mOnLastContactEnd;
        /// <summary>
        /// Occurs when the last contact with a collider happened.
        /// </summary>
        public event EventHandler<CollisionArgs> OnLastContactEnd
        {
            add
            {
                lock (mEventsLock)
                {
                    mEventUsers++;
                    mOnLastContactEnd += value;
                }
            }
            remove
            {
                lock (mEventsLock)
                {
                    mEventUsers--;
                    mOnLastContactEnd -= value;
                }
            }
        }

        internal void PropagateOnLastContactEnd(CollisionArgs args)
        {
            var e = mOnLastContactEnd;
            if (e == null) return;
            e(this, args);
        }

        event EventHandler<CollisionArgs> mOnContactEnd;
        /// <summary>
        /// Occurs when a contact ended.
        /// </summary>
        public event EventHandler<CollisionArgs> OnContactEnd
        {
            add
            {
                lock (mEventsLock)
                {
                    mEventUsers++;
                    mOnContactEnd += value;
                }
            }
            remove
            {
                lock (mEventsLock)
                {
                    mEventUsers--;
                    mOnContactEnd -= value;
                }
            }
        }

        internal void PropagateOnContactEnd(CollisionArgs args)
        {
            var e = mOnContactEnd;
            if (e == null) return;
            e(this, args);
        }

        readonly FastList<Contact> mContacts = new FastList<Contact>();
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
                return mContacts;
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
        /// Gets or sets the entity object. 
        /// Should always cast this as an Entity
        /// </summary>
        /// <value>
        /// The entity object.
        /// </value>
        public object EntityObject { get; set; }
    }
}
