// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Physics.Engine
{
    public class PhysicsShapesRenderingService : GameSystem
    {
        private Material triggerMaterial;
        private Material staticMaterial;
        private Material dynamicMaterial;
        private Material kinematicMaterial;
        private Material characterMaterial;
        private GraphicsDevice graphicsDevice;

        private readonly Dictionary<Type, MeshDraw> debugMeshCache = new Dictionary<Type, MeshDraw>();
        private readonly Dictionary<ColliderShape, MeshDraw> debugMeshCache2 = new Dictionary<ColliderShape, MeshDraw>();

        public override void Initialize()
        {
            graphicsDevice = Services.GetServiceAs<IGraphicsDeviceService>().GraphicsDevice;

            triggerMaterial = PhysicsDebugShapeMaterial.Create(graphicsDevice, Color.AdjustSaturation(Color.Purple, 0.77f), 1);
            staticMaterial = PhysicsDebugShapeMaterial.Create(graphicsDevice, Color.AdjustSaturation(Color.Red, 0.77f), 1);
            dynamicMaterial = PhysicsDebugShapeMaterial.Create(graphicsDevice, Color.AdjustSaturation(Color.Green, 0.77f), 1);
            kinematicMaterial = PhysicsDebugShapeMaterial.Create(graphicsDevice, Color.AdjustSaturation(Color.Blue, 0.77f), 1);
            characterMaterial = PhysicsDebugShapeMaterial.Create(graphicsDevice, Color.AdjustSaturation(Color.LightPink, 0.77f), 1);
        }

        public PhysicsShapesRenderingService(IServiceRegistry registry) : base(registry)
        {
        }

        public Entity CreateDebugEntity(PhysicsComponent component)
        {
            if (component?.ColliderShape == null) return null;

            if (component.DebugEntity != null) return null;

            var debugEntity = new Entity();

            var skinnedElement = component as PhysicsSkinnedComponentBase;
            if (skinnedElement != null && skinnedElement.BoneIndex != -1)
            {
                Vector3 scale, pos;
                Quaternion rot;
                skinnedElement.BoneWorldMatrixOut.Decompose(out scale, out rot, out pos);
                debugEntity.Transform.Position = pos;
                debugEntity.Transform.Rotation = rot;

                if (component.CanScaleShape)
                {
                    component.ColliderShape.Scaling = scale;
                }
            }
            else
            {
                Vector3 scale, pos;
                Quaternion rot;
                component.Entity.Transform.WorldMatrix.Decompose(out scale, out rot, out pos);
                debugEntity.Transform.Position = pos;
                debugEntity.Transform.Rotation = rot;

                if (component.CanScaleShape)
                {
                    component.ColliderShape.Scaling = scale;
                }
            }

            var colliderEntity = CreateChildEntity(component, component.ColliderShape, true);
            if (colliderEntity != null) debugEntity.AddChild(colliderEntity);

            return debugEntity;
        }

        private Entity CreateChildEntity(PhysicsComponent component, ColliderShape shape, bool addOffset)
        {
            if (shape == null)
                return null;

            switch (shape.Type)
            {
                case ColliderShapeTypes.Compound:
                    {
                        var entity = new Entity();

                        //We got to recurse
                        var compound = (CompoundColliderShape)shape;
                        for (var i = 0; i < compound.Count; i++)
                        {
                            var subShape = compound[i];
                            var subEntity = CreateChildEntity(component, subShape, true);
                            if (subEntity != null)
                            {
                                entity.AddChild(subEntity);
                            }
                        }

                        entity.Transform.LocalMatrix = Matrix.Identity;
                        entity.Transform.UseTRS = false;

                        compound.DebugEntity = entity;

                        return entity;
                    }
                case ColliderShapeTypes.Box:
                case ColliderShapeTypes.Capsule:
                case ColliderShapeTypes.ConvexHull:
                case ColliderShapeTypes.Cylinder:
                case ColliderShapeTypes.Sphere:
                    {
                        var mat = triggerMaterial;

                        var rigidbodyComponent = component as RigidbodyComponent;
                        if (rigidbodyComponent != null)
                        {
                            mat = rigidbodyComponent.IsKinematic ? kinematicMaterial : dynamicMaterial;
                            mat = rigidbodyComponent.IsTrigger ? triggerMaterial : mat;
                        }
                        else if (component is CharacterComponent)
                        {
                            mat = characterMaterial;
                        }
                        else if (component is StaticColliderComponent)
                        {
                            var staticCollider = (StaticColliderComponent)component;
                            mat = staticCollider.IsTrigger ? triggerMaterial : staticMaterial;
                        }

                        MeshDraw draw;
                        var type = shape.GetType();
                        if (type == typeof(CapsuleColliderShape) || type == typeof(ConvexHullColliderShape))
                        {
                            if (!debugMeshCache2.TryGetValue(shape, out draw))
                            {
                                draw = shape.CreateDebugPrimitive(graphicsDevice);
                                debugMeshCache2[shape] = draw;
                            }
                        }
                        else
                        {
                            if (!debugMeshCache.TryGetValue(shape.GetType(), out draw))
                            {
                                draw = shape.CreateDebugPrimitive(graphicsDevice);
                                debugMeshCache[shape.GetType()] = draw;
                            }
                        }

                        var entity = new Entity
                        {
                            new ModelComponent
                            {
                                Model = new Model
                                {
                                    mat,
                                    new Mesh
                                    {
                                        Draw = draw
                                    }
                                }
                            }
                        };

                        var offset = addOffset ? Matrix.RotationQuaternion(shape.LocalRotation) * Matrix.Translation(shape.LocalOffset * shape.Scaling) : Matrix.Identity;

                        entity.Transform.LocalMatrix = shape.DebugPrimitiveMatrix * Matrix.Scaling(shape.Scaling) * offset;

                        entity.Transform.UseTRS = false;

                        shape.DebugEntity = entity;

                        return entity;
                    }
                default:
                    return null;
            }
        }
    }
}
