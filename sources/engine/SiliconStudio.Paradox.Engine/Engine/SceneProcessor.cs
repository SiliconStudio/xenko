// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// The scene processor to handle a scene. See remarks.
    /// </summary>
    /// <remarks>
    /// This processor is handling specially an entity with a scene component. If an scene component is found, it will
    /// create a sub-<see cref="EntitySystem"/> dedicated to handle the entities inside the scene.
    /// </remarks>
    public sealed class SceneProcessor : EntityProcessor<SceneProcessor.SceneState>
    {
        private readonly Entity sceneEntityRoot;

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
        public SceneProcessor(Entity sceneEntityRoot)
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
            return entity == sceneEntityRoot ? CurrentState = new SceneState(EntitySystem, sceneEntityRoot) : new SceneState(this.EntitySystem.Services, entity);
        }

        protected override void OnEntityAdding(Entity entity, SceneState data)
        {
            if (data != null)
            {
                Scenes.Add(data);
            }
        }

        protected override void OnEntityRemoved(Entity entity, SceneState data)
        {
            if (data != null)
            {
                Scenes.Remove(data);
            }
        }

        internal override bool ShouldStopProcessorChain(Entity entity)
        {
            // If the entity being added is not the scene entity root, don't run other processors, as this is handled 
            // by a nested EntitySystem
            return !ReferenceEquals(entity, sceneEntityRoot);
        }

        public override void Update(GameTime time)
        {
            foreach (var sceneEntityAndState in Scenes)
            {
                sceneEntityAndState.EntitySystem.Update(time);
            }
        }

        public override void Draw(GameTime time)
        {
            foreach (var sceneEntityAndState in Scenes)
            {
                sceneEntityAndState.EntitySystem.Draw(time);
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
            public SceneState(IServiceRegistry services, Entity sceneEntityRoot)
            {
                if (services == null) throw new ArgumentNullException("services");
                if (sceneEntityRoot == null) throw new ArgumentNullException("sceneEntityRoot");

                Scene = sceneEntityRoot;
                EntitySystem = services.GetSafeServiceAs<SceneSystem>().CreateSceneEntitySystem(sceneEntityRoot);
                SceneComponent = Scene.Get<SceneComponent>();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SceneState"/> class.
            /// </summary>
            /// <param name="entitySystem">The entity system.</param>
            /// <param name="scene">The scene.</param>
            /// <exception cref="System.ArgumentNullException">
            /// entitySystem
            /// or
            /// scene
            /// </exception>
            public SceneState(EntitySystem entitySystem, Entity scene)
            {
                if (entitySystem == null) throw new ArgumentNullException("entitySystem");
                if (scene == null) throw new ArgumentNullException("scene");

                EntitySystem = entitySystem;
                Scene = scene;
                SceneComponent = Scene.Get<SceneComponent>();
            }

            /// <summary>
            /// Gets the scene.
            /// </summary>
            /// <value>The scene.</value>
            public Entity Scene { get; private set; }

            /// <summary>
            /// Gets the scene renderer.
            /// </summary>
            /// <value>The scene renderer.</value>
            public SceneComponent SceneComponent { get; private set; }

            /// <summary>
            /// Entity System dedicated to this scene.
            /// </summary>
            public EntitySystem EntitySystem { get; private set; }
        }
    }
}