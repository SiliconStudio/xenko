// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Physics
{
    public class Simulation : IDisposable
    {
        private readonly BulletSharp.DiscreteDynamicsWorld discreteDynamicsWorld;
        private readonly BulletSharp.CollisionWorld collisionWorld;

        private readonly BulletSharp.CollisionDispatcher dispatcher;
        private readonly BulletSharp.CollisionConfiguration collisionConfiguration;
        private readonly BulletSharp.DbvtBroadphase broadphase;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly BulletSharp.ContactSolverInfo solverInfo;

        private readonly BulletSharp.DispatcherInfo dispatchInfo;

        private readonly bool canCcd;

        public bool ContinuousCollisionDetection
        {
            get
            {
                if (!canCcd)
                {
                    throw new Exception("ContinuousCollisionDetection must be enabled at physics engine initialization using the proper flag.");
                }

                return dispatchInfo.UseContinuous;
            }
            set
            {
                if (!canCcd)
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
        /// <param name="flags">The flags.</param>
        /// <exception cref="System.NotImplementedException">SoftBody processing is not yet available</exception>
        internal Simulation(PhysicsEngineFlags flags = PhysicsEngineFlags.None)
        {
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
                    canCcd = true;
                    solverInfo.SolverMode |= BulletSharp.SolverModes.Use2FrictionDirections | BulletSharp.SolverModes.RandomizeOrder;
                    dispatchInfo.UseContinuous = true;
                }
            }
        }

        readonly List<ContactPoint> newContactsFastCache = new List<ContactPoint>();
        readonly List<ContactPoint> updatedContactsFastCache = new List<ContactPoint>();
        readonly List<ContactPoint> deletedContactsFastCache = new List<ContactPoint>();
        readonly List<Collision> alivePairsFastCache = new List<Collision>();
        readonly HashSet<Collision> processedPairsFastCache = new HashSet<Collision>();
        readonly Queue<Collision> removedPairsFastCache = new Queue<Collision>();

        readonly Queue<Collision> collisionsQueue = new Queue<Collision>();
        readonly Queue<ContactPoint> contactsQueue = new Queue<ContactPoint>(); 

        internal void ProcessContacts()
        {
            var contactsProfiler = Profiler.Begin(PhysicsProfilingKeys.ContactsProfilingKey);

            processedPairsFastCache.Clear();
            var numManifolds = collisionWorld.Dispatcher.NumManifolds;
            for (var i = 0; i < numManifolds; i++)
            {
                var manifold = collisionWorld.Dispatcher.GetManifoldByIndexInternal(i);
                var bodyA = manifold.Body0;
                var bodyB = manifold.Body1;

                var colA = (Collider)bodyA?.UserObject;
                var colB = (Collider)bodyB?.UserObject;

                if (colA == null || colB == null)
                {
                    continue;
                }

                if (!colA.ContactsAlwaysValid && !colB.ContactsAlwaysValid)
                {
                    continue;
                }

                //Pairs management
                Collision pair = null;
                var newPair = true;
                foreach (var pair1 in colA.Collisions)
                {
                    if ((pair1.ColliderA != colA || pair1.ColliderB != colB) && (pair1.ColliderA != colB || pair1.ColliderB != colA)) continue;
                    pair = pair1;
                    newPair = false;
                    break;
                }

                var numContacts = manifold.NumContacts;
                if (numContacts == 0 && newPair)
                {
                    continue;
                }

                newContactsFastCache.Clear();
                updatedContactsFastCache.Clear();
                deletedContactsFastCache.Clear();

                if (newPair)
                {
                    if (collisionsQueue.Count > 0)
                    {
                        pair = collisionsQueue.Dequeue();
                    }
                    else
                    {
                        pair = new Collision
                        {
                            Contacts = new List<ContactPoint>()
                        };
                    }

                    pair.ColliderA = colA;
                    pair.ColliderB = colB;
                    pair.Contacts.Clear();

                    colA.Collisions.Add(pair);
                    colB.Collisions.Add(pair);
                    alivePairsFastCache.Add(pair);
                }
                else
                {
                    foreach (var contact in pair.Contacts)
                    {
                        deletedContactsFastCache.Add(contact);
                    }
                }

                processedPairsFastCache.Add(pair);

                for (var y = 0; y < numContacts; y++)
                {
                    var cp = manifold.GetContactPoint(y);

                    ContactPoint contact = null;
                    var newContact = true;
                    foreach (var contact1 in pair.Contacts)
                    {
                        if (contact1.Handle.IsAllocated && cp.UserPersistentPtr != IntPtr.Zero && contact1.Handle.Target != GCHandle.FromIntPtr(cp.UserPersistentPtr).Target) continue;
                        contact = contact1;
                        newContact = false;
                        break;
                    }

                    //contactsProfilingState.Mark();

                    if (newContact)
                    {
                        contact = contactsQueue.Count > 0 ? contactsQueue.Dequeue() : new ContactPoint();

                        contact.Distance = cp.Distance;
                        contact.PositionOnA = new Vector3(cp.PositionWorldOnA.X, cp.PositionWorldOnA.Y, cp.PositionWorldOnA.Z);
                        contact.PositionOnB = new Vector3(cp.PositionWorldOnB.X, cp.PositionWorldOnB.Y, cp.PositionWorldOnB.Z);
                        contact.Normal = new Vector3(cp.NormalWorldOnB.X, cp.NormalWorldOnB.Y, cp.NormalWorldOnB.Z);
                        contact.Pair = pair;
                        contact.Handle = GCHandle.Alloc(contact);

                        cp.UserPersistentPtr = GCHandle.ToIntPtr(contact.Handle);

                        pair.Contacts.Add(contact);
                    }
                    else
                    {
                        deletedContactsFastCache.Remove(contact);
                    }

                    if (newContact)
                    {
                        newContactsFastCache.Add(contact);
                    }
                    else
                    {
                        updatedContactsFastCache.Add(contact);
                    }
                }

                //deliver async events

                if (newPair)
                {
                    //are we the first pair we detect?
                    if (colA.Collisions.Count == 1)
                    {
                        while (colA.FirstCollisionChannel.Balance < 0)
                        {
                            colA.FirstCollisionChannel.Send(pair);
                        }
                    }

                    //are we the first pair we detect?
                    if (colB.Collisions.Count == 1)
                    {
                        while (colB.FirstCollisionChannel.Balance < 0)
                        {
                            colB.FirstCollisionChannel.Send(pair);
                        }
                    }

                    while (colA.NewPairChannel.Balance < 0)
                    {
                        colA.NewPairChannel.Send(pair);
                    }

                    while (colB.NewPairChannel.Balance < 0)
                    {
                        colB.NewPairChannel.Send(pair);
                    }
                }

                foreach (var contact in newContactsFastCache)
                {
                    while (contact.Pair.NewContactChannel.Balance < 0)
                    {
                        contact.Pair.NewContactChannel.Send(contact);
                    }
                }

                foreach (var contact in updatedContactsFastCache)
                {
                    while (contact.Pair.ContactUpdateChannel.Balance < 0)
                    {
                        contact.Pair.ContactUpdateChannel.Send(contact);
                    }
                }

                foreach (var contact in deletedContactsFastCache)
                {
                    while (contact.Pair.ContactEndedChannel.Balance < 0)
                    {
                        contact.Pair.ContactEndedChannel.Send(contact);
                    }

                    pair.Contacts.Remove(contact);

                    contact.Handle.Free();
                    contactsQueue.Enqueue(contact);
                }

                if (pair.Contacts.Count == 0)
                {
                    colA.Collisions.Remove(pair);
                    colB.Collisions.Remove(pair);
                    alivePairsFastCache.Remove(pair);
                    collisionsQueue.Enqueue(pair);

                    while (colA.PairEndedChannel.Balance < 0)
                    {
                        colA.PairEndedChannel.Send(pair);
                    }

                    while (colB.PairEndedChannel.Balance < 0)
                    {
                        colB.PairEndedChannel.Send(pair);
                    }

                    if (colA.Collisions.Count == 0)
                    {
                        while (colA.AllPairsEndedChannel.Balance < 0)
                        {
                            colA.AllPairsEndedChannel.Send(pair);
                        }
                    }

                    if (colB.Collisions.Count == 0)
                    {
                        while (colB.AllPairsEndedChannel.Balance < 0)
                        {
                            colB.AllPairsEndedChannel.Send(pair);
                        }
                    }
                }
            }

            //Sometimes narrowphase is skipped it seems and we might get some stuck pair!
            foreach (var pair in alivePairsFastCache)
            {
                if (!processedPairsFastCache.Contains(pair))
                {
                    removedPairsFastCache.Enqueue(pair);
                }
            }

            while (removedPairsFastCache.Count > 0)
            {
                var pair = removedPairsFastCache.Dequeue();

                alivePairsFastCache.Remove(pair);

                //this pair got removed!
                foreach (var contactPoint in pair.Contacts)
                {
                    while (contactPoint.Pair.ContactEndedChannel.Balance < 0)
                    {
                        contactPoint.Pair.ContactEndedChannel.Send(contactPoint);
                    }

                    contactPoint.Handle.Free();
                    contactsQueue.Enqueue(contactPoint);
                }

                var colA = pair.ColliderA;
                var colB = pair.ColliderB;

                colA.Collisions.Remove(pair);
                colB.Collisions.Remove(pair);
                collisionsQueue.Enqueue(pair);

                while (colA.PairEndedChannel.Balance < 0)
                {
                    colA.PairEndedChannel.Send(pair);
                }

                while (colB.PairEndedChannel.Balance < 0)
                {
                    colB.PairEndedChannel.Send(pair);
                }

                if (colA.Collisions.Count == 0)
                {
                    while (colA.AllPairsEndedChannel.Balance < 0)
                    {
                        colA.AllPairsEndedChannel.Send(pair);
                    }
                }

                if (colB.Collisions.Count == 0)
                {
                    while (colB.AllPairsEndedChannel.Balance < 0)
                    {
                        colB.AllPairsEndedChannel.Send(pair);
                    }
                }
            }

            contactsProfiler.End("Contacts: {0}", alivePairsFastCache.Count);
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
        /// Creates the collider.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <returns></returns>
        public Collider CreateCollider(ColliderShape shape)
        {
            var collider = new Collider(shape)
            {
                InternalCollider = new BulletSharp.CollisionObject
                {
                    CollisionShape = shape.InternalShape,
                    ContactProcessingThreshold = !canCcd ? 1e18f : 1e30f
                }
            };

            collider.InternalCollider.UserObject = collider;

            collider.InternalCollider.CollisionFlags |= BulletSharp.CollisionFlags.NoContactResponse;

            if (shape.NeedsCustomCollisionCallback)
            {
                collider.InternalCollider.CollisionFlags |= BulletSharp.CollisionFlags.CustomMaterialCallback;
            }

            return collider;
        }

        /// <summary>
        /// Creates the rigid body.
        /// </summary>
        /// <param name="collider">The collider.</param>
        /// <returns></returns>
        public RigidBody CreateRigidBody(ColliderShape collider)
        {
            var rb = new RigidBody(collider);

            rb.InternalRigidBody = new BulletSharp.RigidBody(0.0f, rb.MotionState, collider.InternalShape, Vector3.Zero)
            {
                UserObject = rb
            };

            rb.InternalCollider = rb.InternalRigidBody;

            rb.InternalCollider.ContactProcessingThreshold = !canCcd ? 1e18f : 1e30f;

            if (collider.NeedsCustomCollisionCallback)
            {
                rb.InternalCollider.CollisionFlags |= BulletSharp.CollisionFlags.CustomMaterialCallback;
            }

            if (collider.Is2D) //set different defaults for 2D shapes
            {
                rb.InternalRigidBody.LinearFactor = new Vector3(1.0f, 1.0f, 0.0f);
                rb.InternalRigidBody.AngularFactor = new Vector3(0.0f, 0.0f, 1.0f);
            }

            return rb;
        }

        /// <summary>
        /// Creates the character.
        /// </summary>
        /// <param name="collider">The collider.</param>
        /// <param name="stepHeight">Height of the step.</param>
        /// <returns></returns>
        public Character CreateCharacter(ColliderShape collider, float stepHeight)
        {
            var ch = new Character(collider)
            {
                InternalCollider = new BulletSharp.PairCachingGhostObject
                {
                    CollisionShape = collider.InternalShape
                }
            };

            ch.InternalCollider.UserObject = ch;

            ch.InternalCollider.CollisionFlags |= BulletSharp.CollisionFlags.CharacterObject;

            if (collider.NeedsCustomCollisionCallback)
            {
                ch.InternalCollider.CollisionFlags |= BulletSharp.CollisionFlags.CustomMaterialCallback;
            }

            ch.InternalCollider.ContactProcessingThreshold = !canCcd ? 1e18f : 1e30f;

            ch.KinematicCharacter = new BulletSharp.KinematicCharacterController((BulletSharp.PairCachingGhostObject)ch.InternalCollider, (BulletSharp.ConvexShape)collider.InternalShape, stepHeight);

            return ch;
        }

        /// <summary>
        /// Adds the collider to the engine processing pipeline.
        /// </summary>
        /// <param name="collider">The collider.</param>
        /// <param name="group">The group.</param>
        /// <param name="mask">The mask.</param>
        public void AddCollider(Collider collider, CollisionFilterGroupFlags group, CollisionFilterGroupFlags mask)
        {
            collisionWorld.AddCollisionObject(collider.InternalCollider, (BulletSharp.CollisionFilterGroups)group, (BulletSharp.CollisionFilterGroups)mask);

            collider.Simulation = this;
        }

        /// <summary>
        /// Removes the collider from the engine processing pipeline.
        /// </summary>
        /// <param name="collider">The collider.</param>
        public void RemoveCollider(Collider collider)
        {
            collisionWorld.RemoveCollisionObject(collider.InternalCollider);

            collider.Simulation = null;
        }

        /// <summary>
        /// Adds the rigid body to the engine processing pipeline.
        /// </summary>
        /// <param name="rigidBody">The rigid body.</param>
        /// <param name="group">The group.</param>
        /// <param name="mask">The mask.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void AddRigidBody(RigidBody rigidBody, CollisionFilterGroupFlags group, CollisionFilterGroupFlags mask)
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            discreteDynamicsWorld.AddRigidBody(rigidBody.InternalRigidBody, (short)group, (short)mask);

            rigidBody.Simulation = this;
        }

        /// <summary>
        /// Removes the rigid body from the engine processing pipeline.
        /// </summary>
        /// <param name="rigidBody">The rigid body.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void RemoveRigidBody(RigidBody rigidBody)
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            discreteDynamicsWorld.RemoveRigidBody(rigidBody.InternalRigidBody);

            rigidBody.Simulation = null;
        }

        /// <summary>
        /// Adds the character to the engine processing pipeline.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="group">The group.</param>
        /// <param name="mask">The mask.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void AddCharacter(Character character, CollisionFilterGroupFlags group, CollisionFilterGroupFlags mask)
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            var collider = character.InternalCollider;
            var action = character.KinematicCharacter;
            discreteDynamicsWorld.AddCollisionObject(collider, (BulletSharp.CollisionFilterGroups)group, (BulletSharp.CollisionFilterGroups)mask);
            discreteDynamicsWorld.AddCharacter(action);

            character.Simulation = this;
        }

        /// <summary>
        /// Removes the character from the engine processing pipeline.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void RemoveCharacter(Character character)
        {
            if (discreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            var collider = character.InternalCollider;
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
        public static Constraint CreateConstraint(ConstraintTypes type, RigidBody rigidBodyA, Matrix frameA, bool useReferenceFrameA = false)
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
        public static Constraint CreateConstraint(ConstraintTypes type, RigidBody rigidBodyA, RigidBody rigidBodyB, Matrix frameA, Matrix frameB, bool useReferenceFrameA = false)
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
                result.Collider = (Collider)rcb.CollisionObject.UserObject;
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
                        Collider = (Collider)rcb.CollisionObjects[i].UserObject,
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
                result.Collider = (Collider)rcb.HitCollisionObject.UserObject;
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
                        Collider = (Collider)rcb.CollisionObjects[i].UserObject,
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