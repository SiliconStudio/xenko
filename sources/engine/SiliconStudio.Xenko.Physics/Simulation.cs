// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    public class Simulation : IDisposable
    {
        private readonly PhysicsProcessor processor;

        private readonly BulletSharp.DiscreteDynamicsWorld discreteDynamicsWorld;
        private readonly BulletSharp.CollisionWorld collisionWorld;

        private readonly BulletSharp.CollisionDispatcher dispatcher;
        private readonly BulletSharp.CollisionConfiguration collisionConfiguration;
        private readonly BulletSharp.DbvtBroadphase broadphase;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly BulletSharp.ContactSolverInfo solverInfo;

        private readonly BulletSharp.DispatcherInfo dispatchInfo;

        internal readonly bool CanCcd;

        public bool ContinuousCollisionDetection
        {
            get
            {
                if (!CanCcd)
                {
                    throw new Exception("ContinuousCollisionDetection must be enabled at physics engine initialization using the proper flag.");
                }

                return dispatchInfo.UseContinuous;
            }
            set
            {
                if (!CanCcd)
                {
                    throw new Exception("ContinuousCollisionDetection must be enabled at physics engine initialization using the proper flag.");
                }

                dispatchInfo.UseContinuous = value;
            }
        }

        /// <summary>
        /// Totally disable the simulation if set to true
        /// </summary>
        public static bool DisableSimulation = false;

        public delegate PhysicsEngineFlags OnSimulationCreationDelegate();

        /// <summary>
        /// Temporary solution to inject engine flags
        /// </summary>
        public static OnSimulationCreationDelegate OnSimulationCreation;

        /// <summary>
        /// Initializes the Physics engine using the specified flags.
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="flags">The flags.</param>
        /// <exception cref="System.NotImplementedException">SoftBody processing is not yet available</exception>
        internal Simulation(PhysicsProcessor processor, PhysicsEngineFlags flags = PhysicsEngineFlags.None)
        {
            this.processor = processor;

            if (flags == PhysicsEngineFlags.None)
            {
                if (OnSimulationCreation != null)
                {
                    flags = OnSimulationCreation();
                }
            }

            MaxSubSteps = 1;
            FixedTimeStep = 1.0f / 60.0f;

            collisionConfiguration = new BulletSharp.DefaultCollisionConfiguration();
            dispatcher = new BulletSharp.CollisionDispatcher(collisionConfiguration);
            broadphase = new BulletSharp.DbvtBroadphase();

            //this allows characters to have proper physics behavior
            broadphase.OverlappingPairCache.SetInternalGhostPairCallback(new BulletSharp.GhostPairCallback());

            //2D pipeline
            var simplex = new BulletSharp.VoronoiSimplexSolver();
            var pdSolver = new BulletSharp.MinkowskiPenetrationDepthSolver();
            var convexAlgo = new BulletSharp.Convex2DConvex2DAlgorithm.CreateFunc(simplex, pdSolver);

            dispatcher.RegisterCollisionCreateFunc(BulletSharp.BroadphaseNativeType.Convex2DShape, BulletSharp.BroadphaseNativeType.Convex2DShape, convexAlgo); //this is the ONLY one that we are actually using
            dispatcher.RegisterCollisionCreateFunc(BulletSharp.BroadphaseNativeType.Box2DShape, BulletSharp.BroadphaseNativeType.Convex2DShape, convexAlgo);
            dispatcher.RegisterCollisionCreateFunc(BulletSharp.BroadphaseNativeType.Convex2DShape, BulletSharp.BroadphaseNativeType.Box2DShape, convexAlgo);
            dispatcher.RegisterCollisionCreateFunc(BulletSharp.BroadphaseNativeType.Box2DShape, BulletSharp.BroadphaseNativeType.Box2DShape, new BulletSharp.Box2DBox2DCollisionAlgorithm.CreateFunc());
            //~2D pipeline

            //default solver
            var solver = new BulletSharp.SequentialImpulseConstraintSolver();

            if (flags.HasFlag(PhysicsEngineFlags.CollisionsOnly))
            {
                collisionWorld = new BulletSharp.CollisionWorld(dispatcher, broadphase, collisionConfiguration);
            }
            else if (flags.HasFlag(PhysicsEngineFlags.SoftBodySupport))
            {
                //mSoftRigidDynamicsWorld = new BulletSharp.SoftBody.SoftRigidDynamicsWorld(mDispatcher, mBroadphase, solver, mCollisionConf);
                //mDiscreteDynamicsWorld = mSoftRigidDynamicsWorld;
                //mCollisionWorld = mSoftRigidDynamicsWorld;
                throw new NotImplementedException("SoftBody processing is not yet available");
            }
            else
            {
                discreteDynamicsWorld = new BulletSharp.DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfiguration);
                collisionWorld = discreteDynamicsWorld;
            }

            if (discreteDynamicsWorld != null)
            {
                solverInfo = discreteDynamicsWorld.SolverInfo; //we are required to keep this reference, or the GC will mess up
                dispatchInfo = discreteDynamicsWorld.DispatchInfo;

                solverInfo.SolverMode |= BulletSharp.SolverModes.CacheFriendly; //todo test if helps with performance or not

                if (flags.HasFlag(PhysicsEngineFlags.ContinuosCollisionDetection))
                {
                    CanCcd = true;
                    solverInfo.SolverMode |= BulletSharp.SolverModes.Use2FrictionDirections | BulletSharp.SolverModes.RandomizeOrder;
                    dispatchInfo.UseContinuous = true;
                }
            }
        }

        readonly Queue<Collision> collisionsQueue = new Queue<Collision>();
        readonly Queue<ContactPoint> contactsQueue = new Queue<ContactPoint>();

        readonly List<ContactPoint> processedContactPoints = new List<ContactPoint>();
        readonly List<ContactPoint> lastFrameContactPoints = new List<ContactPoint>();

        readonly List<Collision> newCollisionsCache = new List<Collision>();
        readonly List<Collision> removedCollisionsCache = new List<Collision>();
        readonly List<ContactPoint> newContactsFastCache = new List<ContactPoint>();
        readonly List<ContactPoint> updatedContactsCache = new List<ContactPoint>();
        readonly List<ContactPoint> removedContactsCache = new List<ContactPoint>();

        struct ContactInfo
        {
            public PhysicsComponent ColA;
            public PhysicsComponent ColB;
            public float Distance;
            public Vector3 Normal;
            public Vector3 PositionOnA;
            public Vector3 PositionOnB;
            public ContactPoint Contact;
            public bool NewContact;
        }

        readonly List<ContactInfo> lastFrameContacts = new List<ContactInfo>();

        internal void CacheContacts()
        {
            lastFrameContacts.Clear();
            var numManifolds = collisionWorld.Dispatcher.NumManifolds;
            for (var i = 0; i < numManifolds; i++)
            {
                var manifold = collisionWorld.Dispatcher.GetManifoldByIndexInternal(i);
                var numContacts = manifold.NumContacts;
                var bodyA = manifold.Body0;
                var bodyB = manifold.Body1;

                var colA = (PhysicsComponent)bodyA?.UserObject;
                var colB = (PhysicsComponent)bodyB?.UserObject;

                if (colA == null || colB == null)
                {
                    continue;
                }

                if (!colA.ProcessCollisions && !colB.ProcessCollisions)
                {
                    continue;
                }

                for (var y = 0; y < numContacts; y++)
                {
                    var cp = manifold.GetContactPoint(y);

                    if (cp.Distance > 0.0f)
                    {
                        cp.UserPersistentData = null;
                        continue;
                    }

                    var info = new ContactInfo
                    {
                        ColA = colA,
                        ColB = colB,
                        Distance = cp.Distance,
                        Normal = cp.NormalWorldOnB,
                        PositionOnA = cp.PositionWorldOnA,
                        PositionOnB = cp.PositionWorldOnB
                    };

                    var contact = (ContactPoint)cp.UserPersistentData;
                    if (contact == null)
                    {
                        contact = contactsQueue.Count > 0 ? contactsQueue.Dequeue() : new ContactPoint();
                        cp.UserPersistentData = contact;
                        info.NewContact = true;
                        contact.Collision = null;
                    }
                    else if (cp.LifeTime == 1)
                    {
                        //this is a new contact, (recycled by bullet possibly)
                        info.NewContact = true;
                        contact.Collision = null;
                    }

                    info.Contact = contact;

                    lastFrameContacts.Add(info);
                }
            }
        }

        internal void ProcessContacts()
        {
            var contactsProfiler = Profiler.Begin(PhysicsProfilingKeys.ContactsProfilingKey);

            newCollisionsCache.Clear();
            removedCollisionsCache.Clear();
            newContactsFastCache.Clear();
            updatedContactsCache.Clear();
            removedContactsCache.Clear();

            foreach (var contactInfo in lastFrameContacts)
            {
                var contact = contactInfo.Contact;

                if (contact.Collision == null)
                {
                    //find if a collision already existed
                    foreach (var col in contactInfo.ColA.Collisions)
                    {
                        if ((col.ColliderA != contactInfo.ColA || col.ColliderB != contactInfo.ColB) && (col.ColliderA != contactInfo.ColB || col.ColliderB != contactInfo.ColA)) continue;
                        contact.Collision = col;
                        break;
                    }

                    //if it's still null we need to create a new collision 
                    if (contact.Collision == null)
                    {
                        //new collision
                        if (collisionsQueue.Count > 0)
                        {
                            contact.Collision = collisionsQueue.Dequeue();
                        }
                        else
                        {
                            contact.Collision = new Collision
                            {
                                Contacts = new List<ContactPoint>()
                            };
                        }

                        contact.Collision.ColliderA = contactInfo.ColA;
                        contact.Collision.ColliderB = contactInfo.ColB;
                        contact.Collision.Contacts.Clear();

                        contactInfo.ColA.Collisions.Add(contact.Collision);
                        contactInfo.ColB.Collisions.Add(contact.Collision);

                        newCollisionsCache.Add(contact.Collision);
                    }
                }

                if (contactInfo.NewContact)
                {
                    contact.Collision.Contacts.Add(contact);

                    newContactsFastCache.Add(contact);
                }
                else
                {
                    updatedContactsCache.Add(contact);
                }

                contact.Distance = contactInfo.Distance;
                contact.PositionOnA = contactInfo.PositionOnA;
                contact.PositionOnB = contactInfo.PositionOnB;
                contact.Normal = contactInfo.Normal;

                //prevent removal of processed contacts
                lastFrameContactPoints.Remove(contact);
            }

            //these contacts did not persist
            foreach (var lastFrameContactPoint in lastFrameContactPoints)
            {
                var collision = lastFrameContactPoint.Collision;

                collision.Contacts.Remove(lastFrameContactPoint);
                contactsQueue.Enqueue(lastFrameContactPoint);

                removedContactsCache.Add(lastFrameContactPoint);

                //make sure we also remove eventually added in this frame
                newContactsFastCache.Remove(lastFrameContactPoint);
                updatedContactsCache.Remove(lastFrameContactPoint);

                if (collision.Contacts.Count > 0) continue;

                collision.ColliderA.Collisions.Remove(collision);
                collision.ColliderB.Collisions.Remove(collision);

                removedCollisionsCache.Add(collision);

                //make sure we also remove eventually added in this frame
                newCollisionsCache.Remove(collision);
                collisionsQueue.Enqueue(collision);
            }

            //clean and add current frame contacts to last frame contacts hash
            lastFrameContactPoints.Clear();

            foreach (var processedContactPoint in lastFrameContacts)
            {
                lastFrameContactPoints.Add(processedContactPoint.Contact);
            }

            contactsProfiler.End("Contacts: {0}", processedContactPoints.Count);
        }

        internal void ProcessContacts2()
        {
            var contactsProfiler = Profiler.Begin(PhysicsProfilingKeys.ContactsProfilingKey);

            newCollisionsCache.Clear();
            removedCollisionsCache.Clear();
            newContactsFastCache.Clear();
            updatedContactsCache.Clear();
            removedContactsCache.Clear();

            processedContactPoints.Clear();

            var numManifolds = collisionWorld.Dispatcher.NumManifolds;
            for (var i = 0; i < numManifolds; i++)
            {
                var manifold = collisionWorld.Dispatcher.GetManifoldByIndexInternal(i);
                var numContacts = manifold.NumContacts;
                var bodyA = manifold.Body0;
                var bodyB = manifold.Body1;

                var colA = (PhysicsComponent)bodyA?.UserObject;
                var colB = (PhysicsComponent)bodyB?.UserObject;

                if (colA == null || colB == null)
                {
                    continue;
                }

                if (!colA.ProcessCollisions && !colB.ProcessCollisions)
                {
                    continue;
                }

                //find if a collision already existed
                Collision collision = null;
                foreach (var col in colA.Collisions)
                {
                    if ((col.ColliderA != colA || col.ColliderB != colB) && (col.ColliderA != colB || col.ColliderB != colA)) continue;
                    collision = col;
                    break;
                }

                for (var y = 0; y < numContacts; y++)
                {
                    var cp = manifold.GetContactPoint(y);

                    if (cp.Distance < 0.0f)
                    {
                        ContactPoint contact = null;

                        if (collision != null)
                        {
                            foreach (var contact1 in collision.Contacts)
                            {
                                if (contact1.Handle.IsAllocated && cp.UserPersistentPtr != IntPtr.Zero && contact1.Handle.Target != GCHandle.FromIntPtr(cp.UserPersistentPtr).Target) continue;
                                contact = contact1;
                                break;
                            }
                        }
                        else
                        {
                            //new collision
                            if (collisionsQueue.Count > 0)
                            {
                                collision = collisionsQueue.Dequeue();
                            }
                            else
                            {
                                collision = new Collision
                                {
                                    Contacts = new List<ContactPoint>()
                                };
                            }

                            collision.ColliderA = colA;
                            collision.ColliderB = colB;
                            collision.Contacts.Clear();

                            colA.Collisions.Add(collision);
                            colB.Collisions.Add(collision);

                            newCollisionsCache.Add(collision);
                        }

                        //new contact
                        if (contact == null)
                        {
                            contact = contactsQueue.Count > 0 ? contactsQueue.Dequeue() : new ContactPoint();
                            contact.Collision = collision;
                            contact.Handle = GCHandle.Alloc(contact);
                            cp.UserPersistentPtr = GCHandle.ToIntPtr(contact.Handle);
                            collision.Contacts.Add(contact);

                            newContactsFastCache.Add(contact);
                        }
                        else
                        {
                            updatedContactsCache.Add(contact);
                        }

                        contact.Distance = cp.Distance;
                        contact.PositionOnA = new Vector3(cp.PositionWorldOnA.X, cp.PositionWorldOnA.Y, cp.PositionWorldOnA.Z);
                        contact.PositionOnB = new Vector3(cp.PositionWorldOnB.X, cp.PositionWorldOnB.Y, cp.PositionWorldOnB.Z);
                        contact.Normal = new Vector3(cp.NormalWorldOnB.X, cp.NormalWorldOnB.Y, cp.NormalWorldOnB.Z);

                        processedContactPoints.Add(contact);
                    }
                }
            }

            foreach (var processedContactPoint in processedContactPoints)
            {
                //the contact persisted
                if (lastFrameContactPoints.Contains(processedContactPoint))
                {
                    //remove since the contact persisted
                    lastFrameContactPoints.Remove(processedContactPoint);
                }
            }

            //these contacts did not persist
            foreach (var lastFrameContactPoint in lastFrameContactPoints)
            {
                lastFrameContactPoint.Collision.Contacts.Remove(lastFrameContactPoint);
                lastFrameContactPoint.Handle.Free();
                contactsQueue.Enqueue(lastFrameContactPoint);

                removedContactsCache.Add(lastFrameContactPoint);

                //make sure we also remove eventually added in this frame
                newContactsFastCache.Remove(lastFrameContactPoint);
                updatedContactsCache.Remove(lastFrameContactPoint);

                if (lastFrameContactPoint.Collision.Contacts.Count > 0) continue;

                var collision = lastFrameContactPoint.Collision;
                collision.ColliderA.Collisions.Remove(collision);
                collision.ColliderB.Collisions.Remove(collision);

                removedCollisionsCache.Add(collision);

                //make sure we also remove eventually added in this frame
                newCollisionsCache.Remove(collision);
            }

            //clean and add current frame contacts to last frame contacts hash
            lastFrameContactPoints.Clear();

            foreach (var processedContactPoint in processedContactPoints)
            {
                lastFrameContactPoints.Add(processedContactPoint);
            }

            SendEvents();

            contactsProfiler.End("Contacts: {0}", processedContactPoints.Count);
        }

        private uint eventFrame;

        internal void SendEvents()
        {
            ++eventFrame;

            foreach (var collision in newCollisionsCache)
            {
                while (collision.ColliderA.NewPairChannel.Balance < 0)
                {
                    collision.ColliderA.NewPairChannel.Send(collision);
                }

                while (collision.ColliderB.NewPairChannel.Balance < 0)
                {
                    collision.ColliderB.NewPairChannel.Send(collision);
                }
            }
            Debug.WriteLineIf(newCollisionsCache.Count > 0, $"New collisions at frame {eventFrame}: {newCollisionsCache.Count}");

            foreach (var collision in removedCollisionsCache)
            {
                while (collision.ColliderA.PairEndedChannel.Balance < 0)
                {
                    collision.ColliderA.PairEndedChannel.Send(collision);
                }

                while (collision.ColliderB.PairEndedChannel.Balance < 0)
                {
                    collision.ColliderB.PairEndedChannel.Send(collision);
                }
            }
            Debug.WriteLineIf(removedCollisionsCache.Count > 0, $"Removed collisions at frame {eventFrame}: {removedCollisionsCache.Count}");

            foreach (var contactPoint in newContactsFastCache)
            {
                while (contactPoint.Collision.NewContactChannel.Balance < 0)
                {
                    contactPoint.Collision.NewContactChannel.Send(contactPoint);
                }
            }
            Debug.WriteLineIf(newContactsFastCache.Count > 0, $"New contacts at frame {eventFrame}: {newContactsFastCache.Count}");

            foreach (var contactPoint in updatedContactsCache)
            {
                while (contactPoint.Collision.ContactUpdateChannel.Balance < 0)
                {
                    contactPoint.Collision.ContactUpdateChannel.Send(contactPoint);
                }
            }

            foreach (var contactPoint in removedContactsCache)
            {
                while (contactPoint.Collision.ContactEndedChannel.Balance < 0)
                {
                    contactPoint.Collision.ContactEndedChannel.Send(contactPoint);
                }
            }
            Debug.WriteLineIf(removedContactsCache.Count > 0, $"Removed contacts at frame {eventFrame}: {removedContactsCache.Count}");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //if (mSoftRigidDynamicsWorld != null) mSoftRigidDynamicsWorld.Dispose();
            if (discreteDynamicsWorld != null)
            {
                discreteDynamicsWorld.Dispose();
            }
            else
            {
                collisionWorld?.Dispose();
            }

            broadphase?.Dispose();
            dispatcher?.Dispose();
            collisionConfiguration?.Dispose();
        }

        /// <summary>
        /// Enables or disables the rendering of collider shapes
        /// </summary>
        public bool ColliderShapesRendering
        {
            set
            {
                processor.RenderColliderShapes(value);
            }
        }

        internal void AddCollider(PhysicsComponent component, CollisionFilterGroupFlags group, CollisionFilterGroupFlags mask)
        {
            collisionWorld.AddCollisionObject(component.NativeCollisionObject, (BulletSharp.CollisionFilterGroups)group, (BulletSharp.CollisionFilterGroups)mask);
        }

        internal void RemoveCollider(PhysicsComponent component)
        {
            collisionWorld.RemoveCollisionObject(component.NativeCollisionObject);
        }

        internal void AddRigidBody(RigidbodyComponent rigidBody, CollisionFilterGroupFlags group, CollisionFilterGroupFlags mask)
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            discreteDynamicsWorld.AddRigidBody(rigidBody.InternalRigidBody, (short)group, (short)mask);
        }

        internal void RemoveRigidBody(RigidbodyComponent rigidBody)
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            discreteDynamicsWorld.RemoveRigidBody(rigidBody.InternalRigidBody);
        }

        internal void AddCharacter(CharacterComponent character, CollisionFilterGroupFlags group, CollisionFilterGroupFlags mask)
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            var collider = character.NativeCollisionObject;
            var action = character.KinematicCharacter;
            discreteDynamicsWorld.AddCollisionObject(collider, (BulletSharp.CollisionFilterGroups)group, (BulletSharp.CollisionFilterGroups)mask);
            discreteDynamicsWorld.AddCharacter(action);

            character.Simulation = this;
        }

        internal void RemoveCharacter(CharacterComponent character)
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            var collider = character.NativeCollisionObject;
            var action = character.KinematicCharacter;
            discreteDynamicsWorld.RemoveCollisionObject(collider);
            discreteDynamicsWorld.RemoveCharacter(action);

            character.Simulation = null;
        }

        /// <summary>
        /// Creates the constraint.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="rigidBodyA">The rigid body a.</param>
        /// <param name="frameA">The frame a.</param>
        /// <param name="useReferenceFrameA">if set to <c>true</c> [use reference frame a].</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Cannot perform this action when the physics engine is set to CollisionsOnly
        /// or
        /// Both RigidBodies must be valid
        /// or
        /// A Gear constraint always needs two rigidbodies to be created.
        /// </exception>
        public static Constraint CreateConstraint(ConstraintTypes type, RigidbodyComponent rigidBodyA, Matrix frameA, bool useReferenceFrameA = false)
        {
            if (rigidBodyA == null) throw new Exception("Both RigidBodies must be valid");

            var rbA = rigidBodyA.InternalRigidBody;

            switch (type)
            {
                case ConstraintTypes.Point2Point:
                    {
                        var constraint = new Point2PointConstraint
                        {
                            InternalPoint2PointConstraint = new BulletSharp.Point2PointConstraint(rbA, frameA.TranslationVector),

                            RigidBodyA = rigidBodyA,
                        };

                        constraint.InternalConstraint = constraint.InternalPoint2PointConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.Hinge:
                    {
                        var constraint = new HingeConstraint
                        {
                            InternalHingeConstraint = new BulletSharp.HingeConstraint(rbA, frameA, useReferenceFrameA),

                            RigidBodyA = rigidBodyA,
                        };

                        constraint.InternalConstraint = constraint.InternalHingeConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.Slider:
                    {
                        var constraint = new SliderConstraint
                        {
                            InternalSliderConstraint = new BulletSharp.SliderConstraint(rbA, frameA, useReferenceFrameA),

                            RigidBodyA = rigidBodyA,
                        };

                        constraint.InternalConstraint = constraint.InternalSliderConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.ConeTwist:
                    {
                        var constraint = new ConeTwistConstraint
                        {
                            InternalConeTwistConstraint = new BulletSharp.ConeTwistConstraint(rbA, frameA),

                            RigidBodyA = rigidBodyA
                        };

                        constraint.InternalConstraint = constraint.InternalConeTwistConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.Generic6DoF:
                    {
                        var constraint = new Generic6DoFConstraint
                        {
                            InternalGeneric6DofConstraint = new BulletSharp.Generic6DofConstraint(rbA, frameA, useReferenceFrameA),

                            RigidBodyA = rigidBodyA
                        };

                        constraint.InternalConstraint = constraint.InternalGeneric6DofConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.Generic6DoFSpring:
                    {
                        var constraint = new Generic6DoFSpringConstraint
                        {
                            InternalGeneric6DofSpringConstraint = new BulletSharp.Generic6DofSpringConstraint(rbA, frameA, useReferenceFrameA),

                            RigidBodyA = rigidBodyA
                        };

                        constraint.InternalConstraint = constraint.InternalGeneric6DofConstraint = constraint.InternalGeneric6DofSpringConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.Gear:
                    {
                        throw new Exception("A Gear constraint always needs two rigidbodies to be created.");
                    }
            }

            return null;
        }

        /// <summary>
        /// Creates the constraint.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="rigidBodyA">The rigid body a.</param>
        /// <param name="rigidBodyB">The rigid body b.</param>
        /// <param name="frameA">The frame a.</param>
        /// <param name="frameB">The frame b.</param>
        /// <param name="useReferenceFrameA">if set to <c>true</c> [use reference frame a].</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Cannot perform this action when the physics engine is set to CollisionsOnly
        /// or
        /// Both RigidBodies must be valid
        /// </exception>
        public static Constraint CreateConstraint(ConstraintTypes type, RigidbodyComponent rigidBodyA, RigidbodyComponent rigidBodyB, Matrix frameA, Matrix frameB, bool useReferenceFrameA = false)
        {
            if (rigidBodyA == null || rigidBodyB == null) throw new Exception("Both RigidBodies must be valid");
            //todo check if the 2 rbs are on the same engine instance!

            var rbA = rigidBodyA.InternalRigidBody;
            var rbB = rigidBodyB.InternalRigidBody;

            switch (type)
            {
                case ConstraintTypes.Point2Point:
                    {
                        var constraint = new Point2PointConstraint
                        {
                            InternalPoint2PointConstraint = new BulletSharp.Point2PointConstraint(rbA, rbB, frameA.TranslationVector, frameB.TranslationVector),

                            RigidBodyA = rigidBodyA,
                            RigidBodyB = rigidBodyB
                        };

                        constraint.InternalConstraint = constraint.InternalPoint2PointConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);
                        rigidBodyB.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.Hinge:
                    {
                        var constraint = new HingeConstraint
                        {
                            InternalHingeConstraint = new BulletSharp.HingeConstraint(rbA, rbB, frameA, frameB, useReferenceFrameA),

                            RigidBodyA = rigidBodyA,
                            RigidBodyB = rigidBodyB
                        };

                        constraint.InternalConstraint = constraint.InternalHingeConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);
                        rigidBodyB.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.Slider:
                    {
                        var constraint = new SliderConstraint
                        {
                            InternalSliderConstraint = new BulletSharp.SliderConstraint(rbA, rbB, frameA, frameB, useReferenceFrameA),

                            RigidBodyA = rigidBodyA,
                            RigidBodyB = rigidBodyB
                        };

                        constraint.InternalConstraint = constraint.InternalSliderConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);
                        rigidBodyB.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.ConeTwist:
                    {
                        var constraint = new ConeTwistConstraint
                        {
                            InternalConeTwistConstraint = new BulletSharp.ConeTwistConstraint(rbA, rbB, frameA, frameB),

                            RigidBodyA = rigidBodyA,
                            RigidBodyB = rigidBodyB
                        };

                        constraint.InternalConstraint = constraint.InternalConeTwistConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);
                        rigidBodyB.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.Generic6DoF:
                    {
                        var constraint = new Generic6DoFConstraint
                        {
                            InternalGeneric6DofConstraint = new BulletSharp.Generic6DofConstraint(rbA, rbB, frameA, frameB, useReferenceFrameA),

                            RigidBodyA = rigidBodyA,
                            RigidBodyB = rigidBodyB
                        };

                        constraint.InternalConstraint = constraint.InternalGeneric6DofConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);
                        rigidBodyB.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.Generic6DoFSpring:
                    {
                        var constraint = new Generic6DoFSpringConstraint
                        {
                            InternalGeneric6DofSpringConstraint = new BulletSharp.Generic6DofSpringConstraint(rbA, rbB, frameA, frameB, useReferenceFrameA),

                            RigidBodyA = rigidBodyA,
                            RigidBodyB = rigidBodyB
                        };

                        constraint.InternalConstraint = constraint.InternalGeneric6DofConstraint = constraint.InternalGeneric6DofSpringConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);
                        rigidBodyB.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
                case ConstraintTypes.Gear:
                    {
                        var constraint = new GearConstraint
                        {
                            InternalGearConstraint = new BulletSharp.GearConstraint(rbA, rbB, frameA.TranslationVector, frameB.TranslationVector),

                            RigidBodyA = rigidBodyA,
                            RigidBodyB = rigidBodyB
                        };

                        constraint.InternalConstraint = constraint.InternalGearConstraint;

                        rigidBodyA.LinkedConstraints.Add(constraint);
                        rigidBodyB.LinkedConstraints.Add(constraint);

                        return constraint;
                    }
            }

            return null;
        }

        /// <summary>
        /// Adds the constraint to the engine processing pipeline.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void AddConstraint(Constraint constraint)
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            discreteDynamicsWorld.AddConstraint(constraint.InternalConstraint);
            constraint.Simulation = this;
        }

        /// <summary>
        /// Adds the constraint to the engine processing pipeline.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <param name="disableCollisionsBetweenLinkedBodies">if set to <c>true</c> [disable collisions between linked bodies].</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void AddConstraint(Constraint constraint, bool disableCollisionsBetweenLinkedBodies)
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            discreteDynamicsWorld.AddConstraint(constraint.InternalConstraint, disableCollisionsBetweenLinkedBodies);
            constraint.Simulation = this;
        }

        /// <summary>
        /// Removes the constraint from the engine processing pipeline.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void RemoveConstraint(Constraint constraint)
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            discreteDynamicsWorld.RemoveConstraint(constraint.InternalConstraint);
            constraint.Simulation = null;
        }

        /// <summary>
        /// Raycasts and stops at the first hit.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        public HitResult Raycast(Vector3 from, Vector3 to)
        {
            var result = new HitResult(); //result.Succeded is false by default

            using (var rcb = new BulletSharp.ClosestRayResultCallback(from, to))
            {
                collisionWorld.RayTest(ref from, ref to, rcb);

                if (rcb.CollisionObject == null) return result;
                result.Succeeded = true;
                result.Collider = (PhysicsComponent)rcb.CollisionObject.UserObject;
                result.Normal = rcb.HitNormalWorld;
                result.Point = rcb.HitPointWorld;
            }

            return result;
        }

        /// <summary>
        /// Raycasts penetrating any shape the ray encounters.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        public List<HitResult> RaycastPenetrating(Vector3 from, Vector3 to)
        {
            var result = new List<HitResult>();

            using (var rcb = new BulletSharp.AllHitsRayResultCallback(from, to))
            {
                collisionWorld.RayTest(ref from, ref to, rcb);

                var count = rcb.CollisionObjects.Count;

                for (var i = 0; i < count; i++)
                {
                    var singleResult = new HitResult
                    {
                        Succeeded = true,
                        Collider = (PhysicsComponent)rcb.CollisionObjects[i].UserObject,
                        Normal = rcb.HitNormalWorld[i],
                        Point = rcb.HitPointWorld[i]
                    };

                    result.Add(singleResult);
                }
            }

            return result;
        }

        /// <summary>
        /// Pefrorms a sweep test using a collider shape and stops at the first hit
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">This kind of shape cannot be used for a ShapeSweep.</exception>
        public HitResult ShapeSweep(ColliderShape shape, Matrix from, Matrix to)
        {
            var sh = shape.InternalShape as BulletSharp.ConvexShape;
            if (sh == null) throw new Exception("This kind of shape cannot be used for a ShapeSweep.");

            var result = new HitResult(); //result.Succeded is false by default

            using (var rcb = new BulletSharp.ClosestConvexResultCallback(from.TranslationVector, to.TranslationVector))
            {
                collisionWorld.ConvexSweepTest(sh, from, to, rcb);

                if (rcb.HitCollisionObject == null) return result;
                result.Succeeded = true;
                result.Collider = (PhysicsComponent)rcb.HitCollisionObject.UserObject;
                result.Normal = rcb.HitNormalWorld;
                result.Point = rcb.HitPointWorld;
            }

            return result;
        }

        /// <summary>
        /// Pefrorms a sweep test using a collider shape and never stops until "to"
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">This kind of shape cannot be used for a ShapeSweep.</exception>
        public List<HitResult> ShapeSweepPenetrating(ColliderShape shape, Matrix from, Matrix to)
        {
            var sh = shape.InternalShape as BulletSharp.ConvexShape;
            if (sh == null) throw new Exception("This kind of shape cannot be used for a ShapeSweep.");

            var result = new List<HitResult>();

            using (var rcb = new BulletSharp.AllHitsConvexResultCallback())
            {
                collisionWorld.ConvexSweepTest(sh, from, to, rcb);

                var count = rcb.CollisionObjects.Count;
                for (var i = 0; i < count; i++)
                {
                    var singleResult = new HitResult
                    {
                        Succeeded = true,
                        Collider = (PhysicsComponent)rcb.CollisionObjects[i].UserObject,
                        Normal = rcb.HitNormalWorld[i],
                        Point = rcb.HitPointWorld[i]
                    };

                    result.Add(singleResult);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets or sets the gravity.
        /// </summary>
        /// <value>
        /// The gravity.
        /// </value>
        /// <exception cref="System.Exception">
        /// Cannot perform this action when the physics engine is set to CollisionsOnly
        /// or
        /// Cannot perform this action when the physics engine is set to CollisionsOnly
        /// </exception>
        public Vector3 Gravity
        {
            get
            {
                if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");
                return discreteDynamicsWorld.Gravity;
            }
            set
            {
                if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");
                discreteDynamicsWorld.Gravity = value;
            }
        }

        /// <summary>
        /// The maximum number of steps that the Simulation is allowed to take each tick.
        /// If the engine is running slow (large deltaTime), then you must increase the number of maxSubSteps to compensate for this, otherwise your simulation is “losing” time.
        /// It's important that frame DeltaTime is always less than MaxSubSteps*FixedTimeStep, otherwise you are losing time.
        /// </summary>
        public int MaxSubSteps { get; set; }

        /// <summary>
        /// By decreasing the size of fixedTimeStep, you are increasing the “resolution” of the simulation.
        /// Default is 1.0f / 60.0f or 60fps
        /// </summary>
        public float FixedTimeStep { get; set; }

        public void ClearForces()
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");
            discreteDynamicsWorld.ClearForces();
        }

        public bool SpeculativeContactRestitution
        {
            get
            {
                if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");
                return discreteDynamicsWorld.ApplySpeculativeContactRestitution;
            }
            set
            {
                if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");
                discreteDynamicsWorld.ApplySpeculativeContactRestitution = value;
            }
        }

        public class SimulationArgs : EventArgs
        {
            public float DeltaTime;
        }

        /// <summary>
        /// Called right before the physics simulation.
        /// This event might not be fired by the main thread.
        /// </summary>
        public event EventHandler<SimulationArgs> SimulationBegin;

        protected virtual void OnSimulationBegin(SimulationArgs e)
        {
            var handler = SimulationBegin;
            handler?.Invoke(this, e);
        }

        internal int UpdatedRigidbodies;

        readonly SimulationArgs simulationArgs = new SimulationArgs();

        internal ProfilingState SimulationProfiler;

        internal void Simulate(float deltaTime)
        {
            if (collisionWorld == null) return;

            simulationArgs.DeltaTime = deltaTime;

            UpdatedRigidbodies = 0;

            OnSimulationBegin(simulationArgs);

            SimulationProfiler = Profiler.Begin(PhysicsProfilingKeys.SimulationProfilingKey);

            if (discreteDynamicsWorld != null) discreteDynamicsWorld.StepSimulation(deltaTime, MaxSubSteps, FixedTimeStep);
            else collisionWorld.PerformDiscreteCollisionDetection();

            SimulationProfiler.End("Alive rigidbodies: {0}", UpdatedRigidbodies);

            OnSimulationEnd(simulationArgs);
        }

        /// <summary>
        /// Called right after the physics simulation.
        /// This event might not be fired by the main thread.
        /// </summary>
        public event EventHandler<SimulationArgs> SimulationEnd;

        protected virtual void OnSimulationEnd(SimulationArgs e)
        {
            var handler = SimulationEnd;
            handler?.Invoke(this, e);
        }
    }
}