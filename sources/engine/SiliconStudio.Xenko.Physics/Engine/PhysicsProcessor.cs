// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using System.Collections.Generic;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

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
        private Simulation simulation;

        private bool colliderShapesRendering;

        private PhysicsDebugShapeRendering debugShapeRendering;

        public PhysicsProcessor()
            : base(typeof(TransformComponent))
        {
        }

        public Simulation Simulation => simulation;

        internal void RenderColliderShapes(bool enabled)
        {
            colliderShapesRendering = enabled;

            foreach (var element in elements)
            {
                if(enabled) element.AddDebugEntity(sceneSystem.SceneInstance.Scene);
                else element.RemoveDebugEntity(sceneSystem.SceneInstance.Scene);
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

            data.PhysicsComponent.Simulation = simulation;
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
                component.AddDebugEntity(sceneSystem.SceneInstance.Scene);
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

            var gfxDevice = Services.GetSafeServiceAs<IGraphicsDeviceService>()?.GraphicsDevice;
            if (gfxDevice != null)
            {
                debugShapeRendering = new PhysicsDebugShapeRendering(gfxDevice);
            }

            sceneSystem = Services.GetSafeServiceAs<SceneSystem>();
        }

        protected override void OnSystemRemove()
        {
            physicsSystem.Release(this);
        }

        internal void UpdateCharacters()
        {
            var charactersProfilingState = Profiler.Begin(PhysicsProfilingKeys.CharactersProfilingKey);
            var activeCharacters = 0;
            //characters need manual updating
            foreach (var element in characters)
            {
                if(!element.Enabled) continue;

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
    }
}