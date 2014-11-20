// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Threading;

namespace SiliconStudio.Paradox.Physics
{
    public class PhysicsProcessor : EntityProcessor<PhysicsProcessor.AssociatedData>
    {
        public class AssociatedData
        {
            public PhysicsComponent PhysicsComponent;
            public TransformationComponent TransformationComponent;
            public ModelComponent ModelComponent; //not mandatory, could be null e.g. invisible triggers
        }

        readonly FastList<PhysicsElement> elements = new FastList<PhysicsElement>();
        readonly FastList<PhysicsElement> characters = new FastList<PhysicsElement>();

        Bullet2PhysicsSystem physicsSystem;
        RenderSystem renderSystem;

        public PhysicsProcessor()
            : base(new PropertyKey[] { PhysicsComponent.Key, TransformationComponent.Key })
        {
        }

        protected override AssociatedData GenerateAssociatedData(Entity entity)
        {
            return new AssociatedData
            {
                PhysicsComponent = entity.Get(PhysicsComponent.Key),
                TransformationComponent = entity.Get(TransformationComponent.Key),
                ModelComponent = entity.Get(ModelComponent.Key),
            };
        }

        //This is called by the physics engine to update the transformation of Dynamic rigidbodies
        static void RigidBodySetWorldTransform(PhysicsElement element, Matrix physicsTransform)
        {
            element.UpdateTransformationComponent(physicsTransform);
        }

        //This is valid for Dynamic rigidbodies (called once at initialization) 
        //and Kinematic rigidbodies (called every simulation tick (if body not sleeping) to let the physics engine know where the kinematic body is)
        static void RigidBodyGetWorldTransform(PhysicsElement element, out Matrix physicsTransform)
        {
            physicsTransform = element.DerivePhysicsTransformation();
        }

        void NewElement(PhysicsElement element, AssociatedData data, Entity entity)
        {
            if (element.Shape == null) return; //no shape no purpose

            var shape = element.Shape.Shape;

            element.Data = data;
            element.BoneIndex = -1;

            if (!element.Sprite && element.LinkedBoneName != null && data.ModelComponent != null)
            {
                //find the linked bone, if can't be found we just skip this element
                for (var index = 0; index < data.ModelComponent.ModelViewHierarchy.Nodes.Length; index++)
                {
                    var node = data.ModelComponent.ModelViewHierarchy.Nodes[index];
                    if (node.Name != element.LinkedBoneName) continue;
                    element.BoneIndex = index;
                    break;
                }

                if (element.BoneIndex == -1)
                {
                    throw new Exception("The specified LinkedBoneName doesn't exist in the model hierarchy.");
                }
            }

            //complex hierarchy models not implemented yet
            if (element.BoneIndex != -1)
            {
                throw new NotImplementedException("Physics on complex hierarchy model's bones is not implemented yet.");
            }

            var defaultGroups = element.CanCollideWith == 0 || element.CollisionGroup == 0;

            switch (element.Type)
            {
                case PhysicsElement.Types.PhantomCollider:
                    {
                        var c = physicsSystem.PhysicsEngine.CreateCollider(shape);

                        element.Collider = c; //required by the next call
                        element.Collider.EntityObject = entity; //required by the next call
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        c.IsTrigger = true;

                        if (defaultGroups)
                        {
                            physicsSystem.PhysicsEngine.AddCollider(c);
                        }
                        else
                        {
                            physicsSystem.PhysicsEngine.AddCollider(c, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;
                case PhysicsElement.Types.StaticCollider:
                    {
                        var c = physicsSystem.PhysicsEngine.CreateCollider(shape);

                        element.Collider = c; //required by the next call
                        element.Collider.EntityObject = entity; //required by the next call
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        c.IsTrigger = false;

                        if (defaultGroups)
                        {
                            physicsSystem.PhysicsEngine.AddCollider(c);
                        }
                        else
                        {
                            physicsSystem.PhysicsEngine.AddCollider(c, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;
                case PhysicsElement.Types.StaticRigidBody:
                    {
                        var rb = physicsSystem.PhysicsEngine.CreateRigidBody(shape);

                        rb.EntityObject = entity;
                        rb.GetWorldTransformCallback = (out Matrix transform) => RigidBodyGetWorldTransform(element, out transform);
                        rb.SetWorldTransformCallback = transform => RigidBodySetWorldTransform(element, transform);
                        element.Collider = rb;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        rb.Type = RigidBodyTypes.Static;

                        if (defaultGroups)
                        {
                            physicsSystem.PhysicsEngine.AddRigidBody(rb);
                        }
                        else
                        {
                            physicsSystem.PhysicsEngine.AddRigidBody(rb, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;
                case PhysicsElement.Types.DynamicRigidBody:
                    {
                        var rb = physicsSystem.PhysicsEngine.CreateRigidBody(shape);

                        rb.EntityObject = entity;
                        rb.GetWorldTransformCallback = (out Matrix transform) => RigidBodyGetWorldTransform(element, out transform);
                        rb.SetWorldTransformCallback = transform => RigidBodySetWorldTransform(element, transform);
                        element.Collider = rb;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        rb.Type = RigidBodyTypes.Dynamic;
                        rb.Mass = 1.0f;

                        if (defaultGroups)
                        {
                            physicsSystem.PhysicsEngine.AddRigidBody(rb);
                        }
                        else
                        {
                            physicsSystem.PhysicsEngine.AddRigidBody(rb, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;
                case PhysicsElement.Types.KinematicRigidBody:
                    {
                        var rb = physicsSystem.PhysicsEngine.CreateRigidBody(shape);

                        rb.EntityObject = entity;
                        rb.GetWorldTransformCallback = (out Matrix transform) => RigidBodyGetWorldTransform(element, out transform);
                        rb.SetWorldTransformCallback = transform => RigidBodySetWorldTransform(element, transform);
                        element.Collider = rb;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        rb.Type = RigidBodyTypes.Kinematic;
                        rb.Mass = 0.0f;

                        if (defaultGroups)
                        {
                            physicsSystem.PhysicsEngine.AddRigidBody(rb);
                        }
                        else
                        {
                            physicsSystem.PhysicsEngine.AddRigidBody(rb, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;
                case PhysicsElement.Types.CharacterController:
                    {
                        var ch = physicsSystem.PhysicsEngine.CreateCharacter(shape, element.StepHeight);

                        element.Collider = ch;
                        element.Collider.EntityObject = entity;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        if (defaultGroups)
                        {
                            physicsSystem.PhysicsEngine.AddCharacter(ch);
                        }
                        else
                        {
                            physicsSystem.PhysicsEngine.AddCharacter(ch, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }

                        characters.Add(element);
                    }
                    break;
            }

            elements.Add(element);
        }

        void DeleteElement(PhysicsElement element, bool now = false)
        {
            //might be possible that this element was not valid during creation so it would be already null
            if (element.Collider == null) return;

            var toDispose = new List<IDisposable>();

            elements.Remove(element);   

            switch (element.Type)
            {
                case PhysicsElement.Types.PhantomCollider:
                case PhysicsElement.Types.StaticCollider:
                {
                    physicsSystem.PhysicsEngine.RemoveCollider(element.Collider);
                }
                    break;
                case PhysicsElement.Types.StaticRigidBody:
                case PhysicsElement.Types.DynamicRigidBody:
                case PhysicsElement.Types.KinematicRigidBody:
                {
                    var rb = (RigidBody)element.Collider;
                    var constraints = rb.LinkedConstraints.ToArray();
                    foreach (var c in constraints)
                    {
                        physicsSystem.PhysicsEngine.RemoveConstraint(c);
                        toDispose.Add(c);
                    }

                    physicsSystem.PhysicsEngine.RemoveRigidBody(rb);
                }
                    break;
                case PhysicsElement.Types.CharacterController:
                {
                    characters.Remove(element);
                    physicsSystem.PhysicsEngine.RemoveCharacter((Character) element.Collider);
                }
                    break;
            }

            toDispose.Add(element.Collider);
            element.Collider = null;

            //dispose in another thread for better performance
            if (!now)
            {
                TaskList.Dispatch(toDispose, 4, 128, (i, disposable) => disposable.Dispose());
            }
            else
            {
                foreach (var d in toDispose)
                {
                    d.Dispose();
                }
            }
        }

        protected override void OnEntityAdding(Entity entity, AssociatedData data)
        {
            if (!physicsSystem.PhysicsEngine.Initialized) return;

            foreach (var element in data.PhysicsComponent.Elements)
            {
                NewElement(element, data, entity);
            }
        }

        protected override void OnEntityRemoved(Entity entity, AssociatedData data)
        {
            if (!physicsSystem.PhysicsEngine.Initialized) return;

            foreach (var element in data.PhysicsComponent.Elements)
            {
                DeleteElement(element, true);
            }
        }

        protected override void OnEnabledChanged(Entity entity, bool enabled)
        {
            if (!physicsSystem.PhysicsEngine.Initialized) return;

            var elements = entity.Get(PhysicsComponent.Key).Elements;

            foreach (var element in elements.Where(element => element.Collider != null))
            {
                element.Collider.Enabled = enabled;
            }
        }

        protected override void OnSystemAdd()
        {
            physicsSystem = (Bullet2PhysicsSystem)Services.GetSafeServiceAs<IPhysicsSystem>();
            renderSystem = Services.GetSafeServiceAs<RenderSystem>();

            //setup debug device and debug shader
            var gfxDevice = Services.GetSafeServiceAs<IGraphicsDeviceService>();
            physicsSystem.PhysicsEngine.DebugGraphicsDevice = gfxDevice.GraphicsDevice;

            //Debug primitives render, should happen about the last steps of the pipeline
            renderSystem.Pipeline.EndPass += DebugShapesDraw;
        }

        protected override void OnSystemRemove()
        {
            //remove all elements from the engine
            foreach (var element in elements)
            {
                DeleteElement(element);
            }
        }

        private void DrawDebugCompound(ref Matrix viewProj, CompoundColliderShape compound, PhysicsElement element)
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
                        var physTrans = element.BoneIndex == -1 ? element.Collider.PhysicsWorldTransform : element.BoneWorldMatrix;
                        physTrans = Matrix.Multiply(subShape.PositiveCenterMatrix, physTrans);

                        //must account collider shape scaling
                        Matrix worldTrans;
                        Matrix.Multiply(ref subShape.DebugPrimitiveScaling, ref physTrans, out worldTrans);

                        physicsSystem.PhysicsEngine.DebugEffect.WorldViewProj = worldTrans * viewProj;
                        physicsSystem.PhysicsEngine.DebugEffect.Color = element.Collider.IsActive ? Color.Green : Color.Red;
                        physicsSystem.PhysicsEngine.DebugEffect.UseUv = subShape.Type != ColliderShapeTypes.ConvexHull;

                        physicsSystem.PhysicsEngine.DebugEffect.Apply();

                        subShape.DebugPrimitive.Draw();
                    }
                        break;
                }
            }
        }

        private void DebugShapesDraw(RenderContext context)
        {
            if (!physicsSystem.PhysicsEngine.CreateDebugPrimitives ||
                    !physicsSystem.PhysicsEngine.RenderDebugPrimitives || 
                    !physicsSystem.PhysicsEngine.Initialized ||
                    physicsSystem.PhysicsEngine.DebugGraphicsDevice == null ||
                    physicsSystem.PhysicsEngine.DebugEffect == null) 
                return;

            Matrix viewProj;
            if (renderSystem.Pipeline.Parameters.ContainsKey(TransformationKeys.View) && renderSystem.Pipeline.Parameters.ContainsKey(TransformationKeys.Projection))
            {
                viewProj = renderSystem.Pipeline.Parameters.Get(TransformationKeys.View) * renderSystem.Pipeline.Parameters.Get(TransformationKeys.Projection);
            }
            else
            {
                return;
            }

            var rasterizers = physicsSystem.PhysicsEngine.DebugGraphicsDevice.RasterizerStates;
            physicsSystem.PhysicsEngine.DebugGraphicsDevice.SetRasterizerState(rasterizers.CullNone);

            foreach (var element in elements)
            {
                var shape = element.Shape.Shape;

                if (shape.Type == ColliderShapeTypes.Compound) //multiple shapes
                {
                    DrawDebugCompound(ref viewProj, (CompoundColliderShape)shape, element);
                }
                else if (shape.Type != ColliderShapeTypes.StaticPlane) //a single shape
                {
                    var physTrans = element.BoneIndex == -1 ? element.Collider.PhysicsWorldTransform : element.BoneWorldMatrix;

                    //must account collider shape scaling
                    Matrix worldTrans;
                    Matrix.Multiply(ref element.Shape.Shape.DebugPrimitiveScaling, ref physTrans, out worldTrans);

                    physicsSystem.PhysicsEngine.DebugEffect.WorldViewProj = worldTrans * viewProj;
                    physicsSystem.PhysicsEngine.DebugEffect.Color = element.Collider.IsActive ? Color.Green : Color.Red;
                    physicsSystem.PhysicsEngine.DebugEffect.UseUv = shape.Type != ColliderShapeTypes.ConvexHull;

                    physicsSystem.PhysicsEngine.DebugEffect.Apply();

                    shape.DebugPrimitive.Draw();
                }
            }

            physicsSystem.PhysicsEngine.DebugGraphicsDevice.SetRasterizerState(rasterizers.CullBack);
        }

        public override void Update(GameTime time)
        {
            if (!physicsSystem.PhysicsEngine.Initialized) return;

            //Simulation processing is from here
            physicsSystem.PhysicsEngine.Update((float)time.Elapsed.TotalSeconds);

            //characters need manual updating
            foreach (var element in characters.Where(element => element.Collider.Enabled))
            {
                element.UpdateTransformationComponent(element.Collider.PhysicsWorldTransform);
            }
        }

        //public override void Draw(GameTime time)
        //{
        //    if (!mPhysicsSystem.PhysicsEngine.Initialized) return;

        //    //process all enabled elements
        //    foreach (var e in mElementsToUpdateDraw)
        //    {
        //        var collider = e.Collider;

        //        var mesh = e.Data.ModelComponent;
        //        if (mesh == null) continue;

        //        var nodeTransform = mesh.ModelViewHierarchy.NodeTransformations[e.BoneIndex];

        //        Vector3 translation;
        //        Vector3 scale;
        //        Quaternion rotation;
        //        nodeTransform.WorldMatrix.Decompose(out scale, out rotation, out translation); //derive rot and translation, scale is ignored for now
        //        if (collider.UpdateTransformation(ref rotation, ref translation))
        //        {
        //            //true, Phys is the authority so we need to update the transformation
        //            TransformationComponent.CreateMatrixTRS(ref translation, ref rotation, ref scale, out nodeTransform.WorldMatrix);
        //            if (nodeTransform.ParentIndex != -1) //assuming -1 is root node
        //            {
        //                var parentWorld = mesh.ModelViewHierarchy.NodeTransformations[nodeTransform.ParentIndex];
        //                var inverseParent = parentWorld.WorldMatrix;
        //                inverseParent.Invert();
        //                nodeTransform.LocalMatrix = Matrix.Multiply(nodeTransform.WorldMatrix, inverseParent);
        //            }
        //            else
        //            {
        //                nodeTransform.LocalMatrix = nodeTransform.WorldMatrix;
        //            }
        //        }

        //        e.BoneWorldMatrix = Matrix.AffineTransformation(1.0f, rotation, translation);

        //        //update TRS
        //        nodeTransform.LocalMatrix.Decompose(out nodeTransform.Transform.Scaling, out nodeTransform.Transform.Rotation, out nodeTransform.Transform.Translation);

        //        mesh.ModelViewHierarchy.NodeTransformations[e.BoneIndex] = nodeTransform; //its a struct so we need to copy back
        //    }
        //}
    }
}
