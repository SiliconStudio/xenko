// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Threading;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Physics
{
    public class PhysicsProcessor : EntityProcessor<PhysicsProcessor.AssociatedData>
    {
        public class AssociatedData
        {
            public PhysicsComponent PhysicsComponent;
            public TransformComponent TransformComponent;
            public ModelComponent ModelComponent; //not mandatory, could be null e.g. invisible triggers
            public bool BoneMatricesUpdated;
        }

        private readonly List<PhysicsElementBase> elements = new List<PhysicsElementBase>();
        private readonly List<PhysicsElementBase> boneElements = new List<PhysicsElementBase>();
        private readonly List<PhysicsElementBase> characters = new List<PhysicsElementBase>();

        private Bullet2PhysicsSystem physicsSystem;
        private Simulation simulation;

        public PhysicsProcessor()
            : base(PhysicsComponent.Key, TransformComponent.Key)
        {
        }

        protected override AssociatedData GenerateAssociatedData(Entity entity)
        {
            var data = new AssociatedData
            {
                PhysicsComponent = entity.Get(PhysicsComponent.Key),
                TransformComponent = entity.Get(TransformComponent.Key),
                ModelComponent = entity.Get(ModelComponent.Key)
            };

            data.PhysicsComponent.Simulation = simulation;

            return data;
        }

        //This is called by the physics engine to update the transformation of Dynamic rigidbodies.
        private static void RigidBodySetWorldTransform(PhysicsElementBase element, ref Matrix physicsTransform)
        {
            if (element.BoneIndex == -1)
            {
                element.UpdateTransformationComponent(ref physicsTransform);
            }
            else
            {
                element.UpdateBoneTransformation(ref physicsTransform);
            }
        }

        //This is valid for Dynamic rigidbodies (called once at initialization)
        //and Kinematic rigidbodies, called every simulation tick (if body not sleeping) to let the physics engine know where the kinematic body is.
        private static void RigidBodyGetWorldTransform(PhysicsElementBase element, out Matrix physicsTransform)
        {
            if (element.BoneIndex == -1)
            {
                element.DerivePhysicsTransformation(out physicsTransform);
            }
            else
            {
                element.DeriveBonePhysicsTransformation(out physicsTransform);
            }
        }

        private void NewElement(PhysicsElementBase element, AssociatedData data, Entity entity)
        {
            element.Data = data;

            if (element.ColliderShapes.Count == 0) return; //no shape no purpose

            if (element.ColliderShape == null) element.ComposeShape();
            var shape = element.ColliderShape;

            if (shape == null) return; //no shape no purpose

            element.BoneIndex = -1;

            var skinnedElement = element as PhysicsSkinnedElementBase;
            if (skinnedElement != null && !skinnedElement.NodeName.IsNullOrEmpty() && data.ModelComponent?.ModelViewHierarchy != null)
            {
                if (!data.BoneMatricesUpdated)
                {
                    Vector3 position, scaling;
                    Quaternion rotation;
                    entity.Transform.WorldMatrix.Decompose(out scaling, out rotation, out position);
                    var isScalingNegative = scaling.X * scaling.Y * scaling.Z < 0.0f;
                    data.ModelComponent.ModelViewHierarchy.NodeTransformations[0].LocalMatrix = entity.Transform.WorldMatrix;
                    data.ModelComponent.ModelViewHierarchy.NodeTransformations[0].IsScalingNegative = isScalingNegative;
                    data.ModelComponent.ModelViewHierarchy.UpdateMatrices();
                    data.BoneMatricesUpdated = true;
                }

                skinnedElement.BoneIndex = data.ModelComponent.ModelViewHierarchy.Nodes.IndexOf(x => x.Name == skinnedElement.NodeName);

                if (element.BoneIndex == -1)
                {
                    throw new Exception("The specified NodeName doesn't exist in the model hierarchy.");
                }

                element.BoneWorldMatrixOut = element.BoneWorldMatrix = data.ModelComponent.ModelViewHierarchy.NodeTransformations[element.BoneIndex].WorldMatrix;
            }

            var defaultGroups = element.CanCollideWith == 0 || element.CollisionGroup == 0;

            switch (element.Type)
            {
                case PhysicsElementBase.Types.PhantomCollider:
                    {
                        var c = simulation.CreateCollider(shape);

                        element.Collider = c; //required by the next call
                        element.Collider.Entity = entity; //required by the next call
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        c.IsTrigger = true;

                        if (defaultGroups)
                        {
                            simulation.AddCollider(c, CollisionFilterGroupFlags.DefaultFilter, CollisionFilterGroupFlags.AllFilter);
                        }
                        else
                        {
                            simulation.AddCollider(c, (CollisionFilterGroupFlags)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;

                case PhysicsElementBase.Types.StaticCollider:
                    {
                        var c = simulation.CreateCollider(shape);

                        element.Collider = c; //required by the next call
                        element.Collider.Entity = entity; //required by the next call
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        c.IsTrigger = false;

                        if (defaultGroups)
                        {
                            simulation.AddCollider(c, CollisionFilterGroupFlags.DefaultFilter, CollisionFilterGroupFlags.AllFilter);
                        }
                        else
                        {
                            simulation.AddCollider(c, (CollisionFilterGroupFlags)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;

                case PhysicsElementBase.Types.StaticRigidBody:
                    {
                        var rb = simulation.CreateRigidBody(shape);

                        rb.Entity = entity;
                        rb.GetWorldTransformCallback = (out Matrix transform) => RigidBodyGetWorldTransform(element, out transform);
                        rb.SetWorldTransformCallback = transform => RigidBodySetWorldTransform(element, ref transform);
                        element.Collider = rb;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        rb.Type = RigidBodyTypes.Static;
                        rb.Mass = 0.0f;

                        if (defaultGroups)
                        {
                            simulation.AddRigidBody(rb, CollisionFilterGroupFlags.DefaultFilter, CollisionFilterGroupFlags.AllFilter);
                        }
                        else
                        {
                            simulation.AddRigidBody(rb, (CollisionFilterGroupFlags)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;

                case PhysicsElementBase.Types.DynamicRigidBody:
                    {
                        var rb = simulation.CreateRigidBody(shape);

                        rb.Entity = entity;
                        rb.GetWorldTransformCallback = (out Matrix transform) => RigidBodyGetWorldTransform(element, out transform);
                        rb.SetWorldTransformCallback = transform => RigidBodySetWorldTransform(element, ref transform);
                        element.Collider = rb;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        rb.Type = RigidBodyTypes.Dynamic;
                        if (rb.Mass == 0.0f) rb.Mass = 1.0f;                        

                        if (defaultGroups)
                        {
                            simulation.AddRigidBody(rb, CollisionFilterGroupFlags.DefaultFilter, CollisionFilterGroupFlags.AllFilter);
                        }
                        else
                        {
                            simulation.AddRigidBody(rb, (CollisionFilterGroupFlags)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;

                case PhysicsElementBase.Types.KinematicRigidBody:
                    {
                        var rb = simulation.CreateRigidBody(shape);

                        rb.Entity = entity;
                        rb.GetWorldTransformCallback = (out Matrix transform) => RigidBodyGetWorldTransform(element, out transform);
                        rb.SetWorldTransformCallback = transform => RigidBodySetWorldTransform(element, ref transform);
                        element.Collider = rb;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        rb.Type = RigidBodyTypes.Kinematic;
                        if (rb.Mass == 0.0f) rb.Mass = 1.0f;

                        if (defaultGroups)
                        {
                            simulation.AddRigidBody(rb, CollisionFilterGroupFlags.DefaultFilter, CollisionFilterGroupFlags.AllFilter);
                        }
                        else
                        {
                            simulation.AddRigidBody(rb, (CollisionFilterGroupFlags)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;

                case PhysicsElementBase.Types.CharacterController:
                    {
                        var charElem = (CharacterElement)element;
                        var ch = simulation.CreateCharacter(shape, charElem.StepHeight);

                        element.Collider = ch;
                        element.Collider.Entity = entity;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        if (defaultGroups)
                        {
                            simulation.AddCharacter(ch, CollisionFilterGroupFlags.DefaultFilter, CollisionFilterGroupFlags.AllFilter);
                        }
                        else
                        {
                            simulation.AddCharacter(ch, (CollisionFilterGroupFlags)element.CollisionGroup, element.CanCollideWith);
                        }

                        characters.Add(element);
                    }
                    break;
            }

            elements.Add(element);
            if (element.BoneIndex != -1) boneElements.Add(element);
        }

        private void DeleteElement(PhysicsElementBase element, bool now = false)
        {
            element.Data = null;

            //might be possible that this element was not valid during creation so it would be already null
            if (element.InternalCollider == null) return;

            var toDispose = new List<IDisposable>();

            elements.Remove(element);
            if (element.BoneIndex != -1) boneElements.Remove(element);

            switch (element.Type)
            {
                case PhysicsElementBase.Types.PhantomCollider:
                case PhysicsElementBase.Types.StaticCollider:
                    {
                        simulation.RemoveCollider(element.Collider);
                    }
                    break;

                case PhysicsElementBase.Types.StaticRigidBody:
                case PhysicsElementBase.Types.DynamicRigidBody:
                case PhysicsElementBase.Types.KinematicRigidBody:
                    {
                        var rb = (RigidBody)element.Collider;
                        var constraints = rb.LinkedConstraints.ToArray();
                        foreach (var c in constraints)
                        {
                            simulation.RemoveConstraint(c);
                            toDispose.Add(c);
                        }

                        simulation.RemoveRigidBody(rb);
                    }
                    break;

                case PhysicsElementBase.Types.CharacterController:
                    {
                        characters.Remove(element);
                        simulation.RemoveCharacter((Character)element.Collider);
                    }
                    break;
            }

            toDispose.Add(element.Collider);
            if (element.ColliderShape != null && !element.ColliderShape.IsPartOfAsset)
            {
                toDispose.Add(element.ColliderShape);
            }
            element.Collider = null;

            //dispose in another thread for better performance
            //if (!now)
            //{
            //    TaskList.Dispatch(toDispose, 4, 128, (i, disposable) => disposable.Dispose());
            //}
            //else
            {
                foreach (var d in toDispose)
                {
                    d.Dispose();
                }
            }
        }

        protected override void OnEntityAdding(Entity entity, AssociatedData data)
        {
            if (Simulation.DisableSimulation)
            {
                foreach (var element in data.PhysicsComponent.Elements)
                {
                    var e = (PhysicsElementBase)element;
                    e.Data = data;
                }
                return;
            }

            //this is not optimal as UpdateWorldMatrix will end up being called twice this frame.. but we need to ensure that we have valid data.
            entity.Transform.UpdateWorldMatrix();

            foreach (var element in data.PhysicsComponent.Elements)
            {
                NewElement((PhysicsElementBase)element, data, entity);
            }
        }

        protected override void OnEntityRemoved(Entity entity, AssociatedData data)
        {
            if (Simulation.DisableSimulation)
            {
                foreach (var element in data.PhysicsComponent.Elements)
                {
                    var e = (PhysicsElementBase)element;
                    e.Data = null;
                }
                return;
            }

            foreach (var element in data.PhysicsComponent.Elements)
            {
                var e = (PhysicsElementBase)element;
                DeleteElement(e, true);
            }
        }

        protected override void OnEnabledChanged(Entity entity, bool enabled)
        {
            if (Simulation.DisableSimulation) return;

            var entityElements = entity.Get(PhysicsComponent.Key).Elements;

            foreach (var element in entityElements)
            {
                var e = (PhysicsElementBase)element;
                if (e.Collider != null)
                {
                    e.Collider.Enabled = enabled;
                }
            }
        }

        protected override void OnSystemAdd()
        {
            try
            {
                physicsSystem = (Bullet2PhysicsSystem)Services.GetSafeServiceAs<IPhysicsSystem>();
            }
            catch (ServiceNotFoundException)
            {
                physicsSystem = new Bullet2PhysicsSystem(Services);
                var game = Services.GetSafeServiceAs<IGame>();
                game.GameSystems.Add(physicsSystem);
            }

            simulation = physicsSystem.Create(this);

            //setup debug device and debug shader
            //var gfxDevice = Services.GetSafeServiceAs<IGraphicsDeviceService>();
            //Simulation.DebugGraphicsDevice = gfxDevice.GraphicsDevice;
        }

        protected override void OnSystemRemove()
        {
            //remove all elements from the engine
            foreach (var element in elements)
            {
                DeleteElement(element);
            }

            physicsSystem.Release(this);
        }

        /*
        private static void DrawDebugCompound(ref Matrix viewProj, CompoundColliderShape compound, PhysicsElement element)
        {
            for (var i = 0; i < compound.Count; i++)
            {
                var subShape = compound[i];
                switch (subShape.Type)
                {
                    case ColliderShapeTypes.StaticPlane:
                        continue;
                    case ColliderShapeTypes.Compound:
                        DrawDebugCompound(ref viewProj, (CompoundColliderShape)compound[i], element);
                        break;

                    default:
                        {
                            var physTrans = Matrix.Multiply(subShape.PositiveCenterMatrix, element.Collider.PhysicsWorldTransform);

                            //must account collider shape scaling
                            Matrix worldTrans;
                            Matrix.Multiply(ref subShape.DebugPrimitiveScaling, ref physTrans, out worldTrans);

                            Simulation.DebugEffect.WorldViewProj = worldTrans * viewProj;
                            Simulation.DebugEffect.Color = element.Collider.IsActive ? Color.Green : Color.Red;
                            Simulation.DebugEffect.UseUv = subShape.Type != ColliderShapeTypes.ConvexHull;

                            Simulation.DebugEffect.Apply();

                            subShape.DebugPrimitive.Draw();
                        }
                        break;
                }
            }
        }

        private void DebugShapesDraw(RenderContext context)
        {
            if (    !Simulation.CreateDebugPrimitives ||
                    !Simulation.RenderDebugPrimitives ||
                    Simulation.DebugGraphicsDevice == null ||
                    Simulation.DebugEffect == null)
                return;

            Matrix viewProj;
            if (context.Parameters.ContainsKey(TransformationKeys.View) && context.Parameters.ContainsKey(TransformationKeys.Projection))
            {
                viewProj = context.Parameters.Get(TransformationKeys.View) * context.Parameters.Get(TransformationKeys.Projection);
            }
            else
            {
                return;
            }

            var rasterizers = Simulation.DebugGraphicsDevice.RasterizerStates;
            Simulation.DebugGraphicsDevice.SetRasterizerState(rasterizers.CullNone);

            foreach (var element in elements)
            {
                var shape = element.Shape.Shape;

                if (shape.Type == ColliderShapeTypes.Compound) //multiple shapes
                {
                    DrawDebugCompound(ref viewProj, (CompoundColliderShape)shape, element);
                }
                else if (shape.Type != ColliderShapeTypes.StaticPlane) //a single shape
                {
                    var physTrans = element.Collider.PhysicsWorldTransform;

                    //must account collider shape scaling
                    Matrix worldTrans;
                    Matrix.Multiply(ref element.Shape.Shape.DebugPrimitiveScaling, ref physTrans, out worldTrans);

                    Simulation.DebugEffect.WorldViewProj = worldTrans * viewProj;
                    Simulation.DebugEffect.Color = element.Collider.IsActive ? Color.Green : Color.Red;
                    Simulation.DebugEffect.UseUv = shape.Type != ColliderShapeTypes.ConvexHull;

                    Simulation.DebugEffect.Apply();

                    shape.DebugPrimitive.Draw();
                }
            }

            Simulation.DebugGraphicsDevice.SetRasterizerState(rasterizers.CullBack);
        }
        */

        internal void UpdateCharacters()
        {
            //characters need manual updating
            foreach (var element in characters.Where(x => x.Collider.Enabled))
            {
                var worldTransform = element.Collider.PhysicsWorldTransform;
                element.UpdateTransformationComponent(ref worldTransform);
            }
        }

        public override void Draw(RenderContext context)
        {
            foreach (var element in boneElements.Where(x => x.Collider.Enabled))
            {
                var model = element.Data.ModelComponent;

                //write to ModelViewHierarchy
                if ((element.Collider as RigidBody) != null && element.RigidBody.Type == RigidBodyTypes.Dynamic)
                {
                    model.ModelViewHierarchy.NodeTransformations[element.BoneIndex].WorldMatrix = element.BoneWorldMatrixOut;
                }
            }
        }

        internal void UpdateBones()
        {
            foreach (var element in boneElements.Where(x => x.Collider.Enabled))
            {
                var model = element.Data.ModelComponent;

                //read from ModelViewHierarchy
                element.BoneWorldMatrix = model.ModelViewHierarchy.NodeTransformations[element.BoneIndex].WorldMatrix;
            }
        }
    }
}