// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// The scene processor to handle a scene. See remarks.
    /// </summary>
    /// <remarks>
    /// This processor is handling specially an entity with a scene component. If an scene component is found, it will
    /// create a sub-<see cref="EntityManager"/> dedicated to handle the entities inside the scene.
    /// </remarks>
    public sealed class SceneProcessor : EntityProcessor<SceneProcessor.SceneState>
    {
        private readonly Scene sceneEntityRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneProcessor"/> class.
        /// </summary>
        public SceneProcessor() : base(new []{ SceneComponent.Key })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneProcessor"/> class.
        /// </summary>
        /// <param name="sceneEntityRoot">The scene entity root.</param>
        /// <exception cref="System.ArgumentNullException">sceneEntityRoot</exception>
        public SceneProcessor(Scene sceneEntityRoot)
            : this()
        {
            if (sceneEntityRoot == null) throw new ArgumentNullException("sceneEntityRoot");
            this.sceneEntityRoot = sceneEntityRoot;
            Scenes = new List<SceneState>();
        }

        public SceneState CurrentState { get; private set; }

        public List<SceneState> Scenes { get; private set; }

        protected override SceneState GenerateAssociatedData(Entity entity)
        {
            var sceneEntity = (Scene)entity;
            return sceneEntity == sceneEntityRoot ? CurrentState = new SceneState(EntityManager, sceneEntity) : new SceneState(EntityManager.Services, sceneEntity);
        }

        protected override void OnEntityAdding(Entity entity, SceneState data)
        {
            if (data != null)
            {
                data.Load();
                Scenes.Add(data);
            }
        }

        protected override void OnEntityRemoved(Entity entity, SceneState data)
        {
            if (data != null)
            {
                data.Unload();
                Scenes.Remove(data);
            }
        }

        internal override bool ShouldStopProcessorChain(Entity entity)
        {
            // If the entity being added is not the scene entity root, don't run other processors, as this is handled 
            // by a nested EntityManager
            return !ReferenceEquals(entity, sceneEntityRoot);
        }

        public override void Update(GameTime time)
        {
            foreach (var sceneEntityAndState in Scenes)
            {
                sceneEntityAndState.EntityManager.Update(time);
            }
        }

        public override void Draw(GameTime time)
        {
            foreach (var sceneEntityAndState in Scenes)
            {
                sceneEntityAndState.EntityManager.Draw(time);
            }
        }

        public class SceneState
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SceneState"/> class.
            /// </summary>
            /// <param name="services">The services.</param>
            /// <param name="sceneEntityRoot">The scene entity root.</param>
            /// <exception cref="System.ArgumentNullException">
            /// services
            /// or
            /// sceneEntityRoot
            /// </exception>
            public SceneState(IServiceRegistry services, Scene sceneEntityRoot)
            {
                if (services == null) throw new ArgumentNullException("services");
                if (sceneEntityRoot == null) throw new ArgumentNullException("sceneEntityRoot");

                Scene = sceneEntityRoot;
                EntityManager = services.GetSafeServiceAs<SceneSystem>().CreateSceneEntitySystem(sceneEntityRoot);
                RendererTypes = new List<EntityComponentRendererType>();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SceneState"/> class.
            /// </summary>
            /// <param name="entityManager">The entity system.</param>
            /// <param name="scene">The scene.</param>
            /// <exception cref="System.ArgumentNullException">
            /// EntityManager
            /// or
            /// scene
            /// </exception>
            public SceneState(EntityManager entityManager, Scene scene)
            {
                if (entityManager == null) throw new ArgumentNullException("entityManager");
                if (scene == null) throw new ArgumentNullException("scene");

                EntityManager = entityManager;
                Scene = scene;
                RendererTypes = new List<EntityComponentRendererType>();
            }

            public void Load()
            {
                RendererTypes.Clear();

                foreach (var componentType in EntityManager.RegisteredComponentTypes)
                {
                    EntitySystemOnComponentTypeRegistered(componentType);
                }

                EntityManager.ComponentTypeRegistered += EntitySystemOnComponentTypeRegistered;   
            }

            public void Unload()
            {
                EntityManager.ComponentTypeRegistered -= EntitySystemOnComponentTypeRegistered;
                RendererTypes.Clear();
            }

            private void EntitySystemOnComponentTypeRegistered(Type type)
            {
                var rendererTypeAttribute = type.GetTypeInfo().GetCustomAttribute<EntityComponentRendererAttribute>();
                if (rendererTypeAttribute == null)
                {
                    return;
                }
                var renderType = rendererTypeAttribute.Value.Type;

                if (renderType != null && typeof(IEntityComponentRenderer).IsAssignableFrom(renderType) && renderType.GetConstructor(Type.EmptyTypes) != null)
                {
                    RendererTypes.Add(rendererTypeAttribute.Value);
                    RendererTypes.Sort(EntityComponentRendererType.DefaultComparer);
                }
            }

            /// <summary>
            /// Gets the scene.
            /// </summary>
            /// <value>The scene.</value>
            public Scene Scene { get; private set; }

            /// <summary>
            /// Gets the component renderers.
            /// </summary>
            /// <value>The renderers.</value>
            public List<EntityComponentRendererType> RendererTypes { get; private set; }

            /// <summary>
            /// Entity System dedicated to this scene.
            /// </summary>
            public EntityManager EntityManager { get; private set; }
        }
    }
}