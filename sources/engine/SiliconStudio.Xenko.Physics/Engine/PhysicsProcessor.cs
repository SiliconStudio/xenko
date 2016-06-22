// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using System.Collections.Generic;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Physics.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Physics
{
    public class PhysicsProcessor : EntityProcessor<PhysicsComponent, PhysicsProcessor.AssociatedData>
    {
        public class AssociatedData
        {
            public PhysicsComponent PhysicsComponent;
            public TransformComponent TransformComponent;
            public ModelComponent ModelComponent; //not mandatory, could be null e.g. invisible triggers
            public bool BoneMatricesUpdated;
        }

        private readonly List<PhysicsComponent> elements = new List<PhysicsComponent>();
        private readonly List<PhysicsSkinnedComponentBase> boneElements = new List<PhysicsSkinnedComponentBase>();
        private readonly List<PhysicsComponent> characters = new List<PhysicsComponent>();

        private Bullet2PhysicsSystem physicsSystem;
        private SceneSystem sceneSystem;
        private Scene debugScene;
        private Entity debugEntityScene;

        private bool colliderShapesRendering;

        private SceneChildRenderer debugSceneRenderer;
        private PhysicsShapesRenderingService debugShapeRendering;

        public PhysicsProcessor()
            : base(typeof(TransformComponent))
        {
        }

        public Simulation Simulation { get; private set; }

        internal void RenderColliderShapes(bool enabled)
        {
            colliderShapesRendering = enabled;

            if (!colliderShapesRendering)
            {
                var mainCompositor = (SceneGraphicsCompositorLayers)sceneSystem.SceneInstance.Scene.Settings.GraphicsCompositor;
                var scene = debugEntityScene.Get<ChildSceneComponent>().Scene;

                foreach (var element in elements)
                {
                    element.RemoveDebugEntity(scene);
                }

                sceneSystem.SceneInstance.Scene.Entities.Remove(debugEntityScene);
                mainCompositor.Master.Renderers.Remove(debugSceneRenderer);
            }
            else
            {
                //we create a child scene to render the shapes, so that they are totally separated from the normal scene
                var mainCompositor = (SceneGraphicsCompositorLayers)sceneSystem.SceneInstance.Scene.Settings.GraphicsCompositor;

                var graphicsCompositor = new SceneGraphicsCompositorLayers
                {
                    Cameras = { mainCompositor.Cameras[0] },
                    Master =
                    {
                        Renderers =
                        {
                            new SceneCameraRenderer { Mode = new PhysicsDebugCameraRendererMode { Name = "Camera renderer" } },
                        }
                    }
                };

                debugScene = new Scene { Settings = { GraphicsCompositor = graphicsCompositor } };

                var childComponent = new ChildSceneComponent { Scene = debugScene };
                debugEntityScene = new Entity { childComponent };
                debugSceneRenderer = new SceneChildRenderer(childComponent);

                mainCompositor.Master.Add(debugSceneRenderer);
                sceneSystem.SceneInstance.Scene.Entities.Add(debugEntityScene);

                foreach (var element in elements)
                {
                    if (element.Enabled)
                    {
                        element.AddDebugEntity(debugScene);
                    }
                }
            }
        }

        protected override AssociatedData GenerateComponentData(Entity entity, PhysicsComponent component)
        {
            var data = new AssociatedData
            {
                PhysicsComponent = component,
                TransformComponent = entity.Transform,
                ModelComponent = entity.Get<ModelComponent>()
            };

            data.PhysicsComponent.Simulation = Simulation;
            data.PhysicsComponent.DebugShapeRendering = debugShapeRendering;

            return data;
        }

        protected override bool IsAssociatedDataValid(Entity entity, PhysicsComponent physicsComponent, AssociatedData associatedData)
        {
            return
                physicsComponent == associatedData.PhysicsComponent &&
                entity.Transform == associatedData.TransformComponent &&
                entity.Get<ModelComponent>() == associatedData.ModelComponent;
        }

        protected override void OnEntityComponentAdding(Entity entity, PhysicsComponent component, AssociatedData data)
        {
            component.Attach(data);

            var character = component as CharacterComponent;
            if (character != null)
            {
                characters.Add(character);
            }

            if (colliderShapesRendering)
            {
                component.AddDebugEntity(debugScene);
            }

            elements.Add(component);
            if (component.BoneIndex != -1)
            {
                boneElements.Add((PhysicsSkinnedComponentBase)component);
            }
        }

        protected override void OnEntityComponentRemoved(Entity entity, PhysicsComponent component, AssociatedData data)
        {
            component.Detach();
        }

        protected override void OnSystemAdd()
        {
            physicsSystem = (Bullet2PhysicsSystem)Services.GetServiceAs<IPhysicsSystem>();
            if (physicsSystem == null)
            {
                physicsSystem = new Bullet2PhysicsSystem(Services);
                var gameSystems = Services.GetServiceAs<IGameSystemCollection>();
                gameSystems.Add(physicsSystem);
            }

            ((IReferencable)physicsSystem).AddReference();

            debugShapeRendering = Services.GetServiceAs<PhysicsShapesRenderingService>();
            if (debugShapeRendering == null)
            {
                debugShapeRendering = new PhysicsShapesRenderingService(Services);
                var gameSystems = Services.GetServiceAs<IGameSystemCollection>();
                gameSystems.Add(debugShapeRendering);
            }

            Simulation = physicsSystem.Create(this);

            sceneSystem = Services.GetSafeServiceAs<SceneSystem>();
        }

        protected override void OnSystemRemove()
        {
            physicsSystem.Release(this);
            ((IReferencable)physicsSystem).Release();
        }

        internal void UpdateCharacters()
        {
            var charactersProfilingState = Profiler.Begin(PhysicsProfilingKeys.CharactersProfilingKey);
            var activeCharacters = 0;
            //characters need manual updating
            foreach (var element in characters)
            {
                if(!element.Enabled || element.ColliderShape == null) continue;

                var worldTransform = element.PhysicsWorldTransform;
                element.UpdateTransformationComponent(ref worldTransform);

                if (element.DebugEntity != null)
                {
                    Vector3 scale, pos;
                    Quaternion rot;
                    worldTransform.Decompose(out scale, out rot, out pos);
                    element.DebugEntity.Transform.Position = pos;
                    element.DebugEntity.Transform.Rotation = rot;
                }

                charactersProfilingState.Mark();
                activeCharacters++;
            }
            charactersProfilingState.End("Active characters: {0}", activeCharacters);
        }

        public override void Draw(RenderContext context)
        {
            foreach (var element in boneElements)
            {
                element.UpdateDraw();
            }
        }

        internal void UpdateBones()
        {
            foreach (var element in boneElements)
            {
                element.UpdateBones();
            }
        }

        public void UpdateContacts()
        {
            Simulation.BeginContactTesting();

            foreach (var dataPair in ComponentDatas)
            {
                var data = dataPair.Value;
                if (data.PhysicsComponent.Enabled && data.PhysicsComponent.ProcessCollisions && data.PhysicsComponent.ColliderShape != null)
                {
                    Simulation.ContactTest(data.PhysicsComponent);
                }
            }

            Simulation.EndContactTesting();
        }
    }
}
