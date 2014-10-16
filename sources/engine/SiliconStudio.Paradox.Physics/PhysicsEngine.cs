// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Physics
{
    public class PhysicsEngine : IDisposable
    {
        BulletSharp.DiscreteDynamicsWorld mDiscreteDynamicsWorld;
        //BulletSharp.SoftBody.SoftRigidDynamicsWorld mSoftRigidDynamicsWorld;
        BulletSharp.CollisionWorld mCollisionWorld;

        BulletSharp.CollisionDispatcher mDispatcher;
        BulletSharp.CollisionConfiguration mCollisionConf;
        BulletSharp.DbvtBroadphase mBroadphase;

        BulletSharp.ContactSolverInfo mSolverInfo;
        BulletSharp.DispatcherInfo mDispatchInfo;

        bool mCanCcd;

        public bool ContinuousCollisionDetection
        {
            get
            {
                if (!mCanCcd)
                {
                    throw new Exception("ContinuousCollisionDetection must be enabled at physics engine initialization using the proper flag.");
                }

                return mDispatchInfo.UseContinuous;
            }
            set
            {
                if (!mCanCcd)
                {
                    throw new Exception("ContinuousCollisionDetection must be enabled at physics engine initialization using the proper flag.");
                }

                mDispatchInfo.UseContinuous = value;
            }
        }

        /// <summary>
        /// The debug effect, populate this field in the case of debug rendering
        /// </summary>
        public PhysicsDebugEffect DebugEffect = null;

        /// <summary>
        /// Set to true if you want the engine to create the debug primitives
        /// </summary>
        public bool CreateDebugPrimitives = false;

        /// <summary>
        ///  Set to true if you want the engine to render the debug primitives
        /// </summary>
        public bool RenderDebugPrimitives = false;

        /// <summary>
        /// Totally disable the simulation if set to true
        /// </summary>
        public bool DisableSimulation = false;

        internal GraphicsDevice DebugGraphicsDevice;

        internal static PhysicsEngine Singleton = null;

        /// <summary>
        /// Initializes the Physics engine using the specified flags.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <exception cref="System.NotImplementedException">SoftBody processing is not yet available</exception>
        public void Initialize(PhysicsEngineFlags flags)
        {
            // Preload proper freetype native library (depending on CPU type)
            Core.NativeLibrary.PreloadLibrary("libbulletc.dll");

            mCollisionConf = new BulletSharp.DefaultCollisionConfiguration();
            mDispatcher = new BulletSharp.CollisionDispatcher(mCollisionConf);
            mBroadphase = new BulletSharp.DbvtBroadphase();

            //this allows characters to have proper physics behavior
            mBroadphase.OverlappingPairCache.SetInternalGhostPairCallback(new BulletSharp.GhostPairCallback());

            //2D pipeline
            var simplex = new BulletSharp.VoronoiSimplexSolver();
            var pdSolver = new BulletSharp.MinkowskiPenetrationDepthSolver();
            var convexAlgo = new BulletSharp.Convex2DConvex2DAlgorithm.CreateFunc(simplex, pdSolver);

            mDispatcher.RegisterCollisionCreateFunc(BulletSharp.BroadphaseNativeType.Convex2DShape, BulletSharp.BroadphaseNativeType.Convex2DShape, convexAlgo); //this is the ONLY one that we are actually using
            mDispatcher.RegisterCollisionCreateFunc(BulletSharp.BroadphaseNativeType.Box2DShape, BulletSharp.BroadphaseNativeType.Convex2DShape, convexAlgo);
            mDispatcher.RegisterCollisionCreateFunc(BulletSharp.BroadphaseNativeType.Convex2DShape, BulletSharp.BroadphaseNativeType.Box2DShape, convexAlgo);
            mDispatcher.RegisterCollisionCreateFunc(BulletSharp.BroadphaseNativeType.Box2DShape, BulletSharp.BroadphaseNativeType.Box2DShape, new BulletSharp.Box2DBox2DCollisionAlgorithm.CreateFunc());
            //~2D pipeline

            //default solver
            var solver = new BulletSharp.SequentialImpulseConstraintSolver();

            if (flags.HasFlag(PhysicsEngineFlags.CollisionsOnly))
            {
                mCollisionWorld = new BulletSharp.CollisionWorld(mDispatcher, mBroadphase, mCollisionConf);
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
                mDiscreteDynamicsWorld = new BulletSharp.DiscreteDynamicsWorld(mDispatcher, mBroadphase, solver, mCollisionConf);
                mCollisionWorld = mDiscreteDynamicsWorld;
            }

            if (mDiscreteDynamicsWorld != null)
            {
                mSolverInfo = mDiscreteDynamicsWorld.SolverInfo; //we are required to keep this reference, or the GC will mess up
                mDispatchInfo = mDiscreteDynamicsWorld.DispatchInfo;

                mSolverInfo.SolverMode |= BulletSharp.SolverModes.CacheFriendly; //todo test if helps with performance or not

                if (flags.HasFlag(PhysicsEngineFlags.ContinuosCollisionDetection))
                {
                    mCanCcd = true;
                    mSolverInfo.SolverMode |= BulletSharp.SolverModes.Use2FrictionDirections | BulletSharp.SolverModes.RandomizeOrder;
                    mDispatchInfo.UseContinuous = true;
                }
            }

            BulletSharp.PersistentManifold.ContactProcessed += PersistentManifoldContactProcessed;
            BulletSharp.PersistentManifold.ContactDestroyed += PersistentManifoldContactDestroyed;

            Initialized = true;

            Singleton = this;
        }

        static void PersistentManifoldContactDestroyed(object userPersistantData)
        {
            var contact = (Contact)userPersistantData;
            var args = new CollisionArgs { Contact = contact };

            var colA = contact.ColliderA;
            var colB = contact.ColliderB;

            colA.Contacts.Remove(contact);
            colB.Contacts.Remove(contact);

            var colAEnded = false;
            var previousColAState = colA.Contacts.Where(x => (x.ColliderA == colA && x.ColliderB == colB) || (x.ColliderA == colB && x.ColliderB == colA));
            if (!previousColAState.Any()) colAEnded = true;

            var colBEnded = false;
            var previousColBState = colB.Contacts.Where(x => (x.ColliderB == colB && x.ColliderA == colA) || (x.ColliderB == colA && x.ColliderA == colB));
            if (!previousColBState.Any()) colBEnded = true;

            if(colAEnded) colA.PropagateOnLastContactEnd(args);
            colA.PropagateOnContactEnd(args);

            if(colBEnded) colB.PropagateOnLastContactEnd(args);
            colB.PropagateOnContactEnd(args);
        }

        static void PersistentManifoldContactProcessed(BulletSharp.ManifoldPoint cp, BulletSharp.CollisionObject body0, BulletSharp.CollisionObject body1)
        {
            var colA = (Collider)body0.UserObject;
            var colB = (Collider)body1.UserObject;

            if (!colA.NeedsCollisionCheck && !colB.NeedsCollisionCheck) return; //don't process at all if both the objects don't need any collision event

            if (cp.UserPersistentData == null) //New contact!
            {
                var contact = new Contact
                {
                    ColliderA = colA,
                    ColliderB = colB,
                    Distance = cp.Distance,
                    PositionOnA = new Vector3(cp.PositionWorldOnA.X, cp.PositionWorldOnA.Y, cp.PositionWorldOnA.Z),
                    PositionOnB = new Vector3(cp.PositionWorldOnB.X, cp.PositionWorldOnB.Y, cp.PositionWorldOnB.Z),
                    Normal = new Vector3(cp.NormalWorldOnB.X, cp.NormalWorldOnB.Y, cp.NormalWorldOnB.Z)
                };

                //must figure if we are a really brand new collision for correct event propagation
                var colABegan = false;
                var previousColAState = colA.Contacts.Where(x => (x.ColliderA == colA && x.ColliderB == colB) || (x.ColliderA == colB && x.ColliderB == colA));
                if (!previousColAState.Any()) colABegan = true;

                var colBBegan = false;
                var previousColBState = colB.Contacts.Where(x => (x.ColliderB == colB && x.ColliderA == colA) || (x.ColliderB == colA && x.ColliderA == colB));
                if (!previousColBState.Any()) colBBegan = true;

                colA.Contacts.Add(contact);
                colB.Contacts.Add(contact);

                var args = new CollisionArgs { Contact = contact };

                cp.UserPersistentData = contact;

                if(colABegan) colA.PropagateOnFirstContactBegin(args);
                colA.PropagateOnContactBegin(args);

                if(colBBegan) colB.PropagateOnFirstContactBegin(args);
                colB.PropagateOnContactBegin(args);
            }
            else
            {
                var contact = (Contact)cp.UserPersistentData;

                contact.Distance = cp.Distance;
                contact.PositionOnA = new Vector3(cp.PositionWorldOnA.X, cp.PositionWorldOnA.Y, cp.PositionWorldOnA.Z);
                contact.PositionOnB = new Vector3(cp.PositionWorldOnB.X, cp.PositionWorldOnB.Y, cp.PositionWorldOnB.Z);
                contact.Normal = new Vector3(cp.NormalWorldOnB.X, cp.NormalWorldOnB.Y, cp.NormalWorldOnB.Z);

                var args = new CollisionArgs { Contact = contact };

                colA.PropagateOnContactChange(args);
                colB.PropagateOnContactChange(args);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //if (mSoftRigidDynamicsWorld != null) mSoftRigidDynamicsWorld.Dispose();
            if (mDiscreteDynamicsWorld != null) mDiscreteDynamicsWorld.Dispose();
            else if (mCollisionWorld != null) mCollisionWorld.Dispose();

            if (mBroadphase != null) mBroadphase.Dispose();
            if (mDispatcher != null) mDispatcher.Dispose();
            if (mCollisionConf != null) mCollisionConf.Dispose();
        }

        /// <summary>
        /// Creates the collider.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="initialPosition">The initial position.</param>
        /// <param name="initialRotation">The initial rotation.</param>
        /// <returns></returns>
        public Collider CreateCollider(ColliderShape shape)
        {
            var collider = new Collider(shape)
            {
                InternalCollider = new BulletSharp.CollisionObject
                {
                    CollisionShape = shape.InternalShape,
                    ContactProcessingThreshold = 1e18f
                }
            };

            collider.InternalCollider.CollisionFlags |= BulletSharp.CollisionFlags.NoContactResponse;

            collider.InternalCollider.UserObject = collider;
            collider.Engine = this;

            return collider;
        }

        /// <summary>
        /// Creates the rigid body.
        /// </summary>
        /// <param name="collider">The collider.</param>
        /// <param name="getWorldTransformCallback"></param>
        /// <param name="setWorldTransformCallback"></param>
        /// <returns></returns>
        public RigidBody CreateRigidBody(ColliderShape collider)
        {
            var rb = new RigidBody(collider);

            rb.InternalRigidBody = new BulletSharp.RigidBody(0.0f, rb.MotionState, collider.InternalShape, Vector3.Zero);
            rb.InternalRigidBody.CollisionFlags |= BulletSharp.CollisionFlags.StaticObject; //already set if mass is 0 actually!

            rb.InternalCollider = rb.InternalRigidBody;

            rb.InternalCollider.ContactProcessingThreshold = 1e18f;

            if (collider.Is2D) //set different defaults for 2D shapes
            {
                rb.InternalRigidBody.LinearFactor = new Vector3(1.0f, 1.0f, 0.0f);
                rb.InternalRigidBody.AngularFactor = new Vector3(0.0f, 0.0f, 1.0f);
            }

            rb.InternalRigidBody.UserObject = rb;
            rb.Engine = this;

            return rb;
        }

        /// <summary>
        /// Creates the character.
        /// </summary>
        /// <param name="collider">The collider.</param>
        /// <param name="initialPosition">The initial position.</param>
        /// <param name="initialRotation">The initial rotation.</param>
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

            ch.InternalCollider.CollisionFlags |= BulletSharp.CollisionFlags.CharacterObject;

            ch.InternalCollider.ContactProcessingThreshold = 1e18f;

            ch.KinematicCharacter = new BulletSharp.KinematicCharacterController((BulletSharp.PairCachingGhostObject)ch.InternalCollider, (BulletSharp.ConvexShape)collider.InternalShape, stepHeight);

            ch.InternalCollider.UserObject = ch;
            ch.Engine = this;

            return ch;
        }

        /// <summary>
        /// Adds the collider to the engine processing pipeline.
        /// </summary>
        /// <param name="collider">The collider.</param>
        public void AddCollider(Collider collider)
        {
            mCollisionWorld.AddCollisionObject(collider.InternalCollider);
        }

        /// <summary>
        /// Adds the collider to the engine processing pipeline.
        /// </summary>
        /// <param name="collider">The collider.</param>
        /// <param name="group">The group.</param>
        /// <param name="mask">The mask.</param>
        public void AddCollider(Collider collider, CollisionFilterGroups group, CollisionFilterGroups mask)
        {
            mCollisionWorld.AddCollisionObject(collider.InternalCollider, (BulletSharp.CollisionFilterGroups)group, (BulletSharp.CollisionFilterGroups)mask);
        }

        /// <summary>
        /// Removes the collider from the engine processing pipeline.
        /// </summary>
        /// <param name="collider">The collider.</param>
        public void RemoveCollider(Collider collider)
        {
            mCollisionWorld.RemoveCollisionObject(collider.InternalCollider);
        }

        /// <summary>
        /// Adds the rigid body to the engine processing pipeline.
        /// </summary>
        /// <param name="rigidBody">The rigid body.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void AddRigidBody(RigidBody rigidBody)
        {
            if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            mDiscreteDynamicsWorld.AddRigidBody(rigidBody.InternalRigidBody);
        }

        /// <summary>
        /// Adds the rigid body to the engine processing pipeline.
        /// </summary>
        /// <param name="rigidBody">The rigid body.</param>
        /// <param name="group">The group.</param>
        /// <param name="mask">The mask.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void AddRigidBody(RigidBody rigidBody, CollisionFilterGroups group, CollisionFilterGroups mask)
        {
            if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            mDiscreteDynamicsWorld.AddRigidBody(rigidBody.InternalRigidBody, (short)group, (short)mask);
        }

        /// <summary>
        /// Removes the rigid body from the engine processing pipeline.
        /// </summary>
        /// <param name="rigidBody">The rigid body.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void RemoveRigidBody(RigidBody rigidBody)
        {
            if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            mDiscreteDynamicsWorld.RemoveRigidBody(rigidBody.InternalRigidBody);
        }

        /// <summary>
        /// Adds the character to the engine processing pipeline.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void AddCharacter(Character character)
        {
            if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            var collider = character.InternalCollider;
            var action = character.KinematicCharacter;
            mDiscreteDynamicsWorld.AddCollisionObject(collider, BulletSharp.CollisionFilterGroups.CharacterFilter);
            mDiscreteDynamicsWorld.AddCharacter(action);
        }

        /// <summary>
        /// Adds the character to the engine processing pipeline.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="group">The group.</param>
        /// <param name="mask">The mask.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void AddCharacter(Character character, CollisionFilterGroups group, CollisionFilterGroups mask)
        {
            if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            var collider = character.InternalCollider;
            var action = character.KinematicCharacter;
            mDiscreteDynamicsWorld.AddCollisionObject(collider, (BulletSharp.CollisionFilterGroups)group, (BulletSharp.CollisionFilterGroups)mask);
            mDiscreteDynamicsWorld.AddCharacter(action);
        }

        /// <summary>
        /// Removes the character from the engine processing pipeline.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void RemoveCharacter(Character character)
        {
            if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            var collider = character.InternalCollider;
            var action = character.KinematicCharacter;
            mDiscreteDynamicsWorld.RemoveCollisionObject(collider);
            mDiscreteDynamicsWorld.RemoveCharacter(action);
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
        public Constraint CreateConstraint(ConstraintTypes type, RigidBody rigidBodyA, Matrix frameA, bool useReferenceFrameA = false)
        {
            if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");
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

                    constraint.Engine = this;

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

                    constraint.Engine = this;

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

                    constraint.Engine = this;

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

                    constraint.Engine = this;

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

                    constraint.Engine = this;

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

                    constraint.Engine = this;

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
        public Constraint CreateConstraint(ConstraintTypes type, RigidBody rigidBodyA, RigidBody rigidBodyB, Matrix frameA, Matrix frameB, bool useReferenceFrameA = false)
        {
            if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");
            if (rigidBodyA == null || rigidBodyB == null) throw new Exception("Both RigidBodies must be valid");

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

                    constraint.Engine = this;

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

                    constraint.Engine = this;

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

                    constraint.Engine = this;

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

                    constraint.Engine = this;

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

                    constraint.Engine = this;

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

                    constraint.Engine = this;

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

                    constraint.Engine = this;

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
            if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            var c = constraint.InternalConstraint;

            mDiscreteDynamicsWorld.AddConstraint(c);
        }

        /// <summary>
        /// Adds the constraint to the engine processing pipeline.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <param name="disableCollisionsBetweenLinkedBodies">if set to <c>true</c> [disable collisions between linked bodies].</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void AddConstraint(Constraint constraint, bool disableCollisionsBetweenLinkedBodies)
        {
            if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            var c = constraint.InternalConstraint;

            mDiscreteDynamicsWorld.AddConstraint(c, disableCollisionsBetweenLinkedBodies);
        }

        /// <summary>
        /// Removes the constraint from the engine processing pipeline.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <exception cref="System.Exception">Cannot perform this action when the physics engine is set to CollisionsOnly</exception>
        public void RemoveConstraint(Constraint constraint)
        {
            if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");

            var c = constraint.InternalConstraint;

            mDiscreteDynamicsWorld.RemoveConstraint(c);
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
                mCollisionWorld.RayTest(ref from, ref to, rcb);

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
                mCollisionWorld.RayTest(ref from, ref to, rcb);

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
            if(sh == null) throw new Exception("This kind of shape cannot be used for a ShapeSweep.");

            var result = new HitResult(); //result.Succeded is false by default

            using (var rcb = new BulletSharp.ClosestConvexResultCallback(from.TranslationVector, to.TranslationVector))
            {
                mCollisionWorld.ConvexSweepTest(sh, from, to, rcb);

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
                mCollisionWorld.ConvexSweepTest(sh, from, to, rcb);

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
        /// Gets a value indicating whether this <see cref="PhysicsEngine"/> is initialized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initialized; otherwise, <c>false</c>.
        /// </value>
        public bool Initialized { get; private set; }

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
                if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");
                return mDiscreteDynamicsWorld.Gravity;
            }
            set
            {
                if (mDiscreteDynamicsWorld == null) throw new Exception("Cannot perform this action when the physics engine is set to CollisionsOnly");
                mDiscreteDynamicsWorld.Gravity = value;
            }
        }

        internal void Update(float delta)
        {
            if (DisableSimulation) return;

            if (mCollisionWorld == null) return;

            if (mDiscreteDynamicsWorld != null) mDiscreteDynamicsWorld.StepSimulation(delta);
            else mCollisionWorld.PerformDiscreteCollisionDetection();
        }
    }
}
