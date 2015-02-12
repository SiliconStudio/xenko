// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// State of a <see cref="SceneComponent"/> and <see cref="SceneChildComponent"/> used by <see cref="SceneProcessor"/>
    /// and <see cref="SceneChildProcessor"/>.
    /// </summary>
    public class SceneInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneInstance" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="sceneEntityRoot">The scene entity root.</param>
        /// <exception cref="System.ArgumentNullException">services
        /// or
        /// sceneEntityRoot</exception>
        public SceneInstance(IServiceRegistry services, Entity entity, Scene sceneEntityRoot)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (entity == null) throw new ArgumentNullException("entity");
            if (sceneEntityRoot == null) throw new ArgumentNullException("sceneEntityRoot");

            Entity = entity;
            Scene = sceneEntityRoot;
            EntityManager = services.GetSafeServiceAs<SceneSystem>().CreateSceneEntitySystem(sceneEntityRoot);
            RendererTypes = new List<EntityComponentRendererType>();
            Load();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneInstance"/> class.
        /// </summary>
        /// <param name="entityManager">The entity system.</param>
        /// <param name="entity"></param>
        /// <param name="scene">The scene.</param>
        /// <exception cref="System.ArgumentNullException">
        /// EntityManager
        /// or
        /// scene
        /// </exception>
        internal SceneInstance(EntityManager entityManager, Entity entity, Scene scene)
        {
            if (entityManager == null) throw new ArgumentNullException("entityManager");
            if (entity == null) throw new ArgumentNullException("entity");
            if (scene == null) throw new ArgumentNullException("scene");

            Entity = entity;
            EntityManager = entityManager;
            Scene = scene;
            RendererTypes = new List<EntityComponentRendererType>();
            Load();
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <value>The entity.</value>
        public Entity Entity { get; private set; }

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

        private void Load()
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
            // TODO: Unload resources
            EntityManager.ComponentTypeRegistered -= EntitySystemOnComponentTypeRegistered;
            RendererTypes.Clear();
        }

        private void EntitySystemOnComponentTypeRegistered(Type type)
        {
            var rendererTypeAttribute = type.GetTypeInfo().GetCustomAttribute<DefaultEntityComponentRendererAttribute>();
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
    }
}