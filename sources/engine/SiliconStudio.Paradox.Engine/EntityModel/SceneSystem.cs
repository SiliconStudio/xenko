// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// The scene system handles the scenes of a game.
    /// </summary>
    public class SceneSystem : GameSystemBase
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("SceneSystem");

        private const string DefaultSceneName = "__DefaultScene__"; // TODO: How to determine the default scene?

        private RenderContext drawContext;

        private EntitySystem entitySystem;

        private Entity scene;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSystemBase" /> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <remarks>The GameSystem is expecting the following services to be registered: <see cref="IGame" /> and <see cref="IAssetManager" />.</remarks>
        public SceneSystem(IServiceRegistry registry)
            : base(registry)
        {
            registry.AddService(typeof(SceneSystem), this);
        }

        /// <summary>
        /// Gets the entity system of the current scene.
        /// </summary>
        /// <value>The scene entity system.</value>
        public EntitySystem EntitySystem
        {
            get
            {
                return entitySystem;
            }
        }

        /// <summary>
        /// Gets or sets the root scene.
        /// </summary>
        /// <value>The scene.</value>
        /// <exception cref="System.ArgumentNullException">Scene cannot be null</exception>
        public Entity Scene
        {
            get
            {
                return scene;
            }
            set // TODO Should we allow a setter?
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Scene cannot be null");
                }

                // Check that we actually have a scene component
                if (value.Get<SceneComponent>() == null)
                {
                    throw new InvalidOperationException("The entity requires a SceneComponent");
                }

                if (value != scene)
                {
                    if (scene != null)
                    {
                        entitySystem.Remove(scene);
                    }

                    entitySystem = CreateSceneEntitySystem(value);
                    scene = value;
                }
            }
        }

        protected override void LoadContent()
        {
            var assetManager = Services.GetSafeServiceAs<AssetManager>();
            Scene = assetManager.Load<Entity>(DefaultSceneName);
            // Create the drawing context
            drawContext = new RenderContext(GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            if (entitySystem != null)
            {
                entitySystem.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            try
            {
                var sceneState = EntitySystem.GetProcessor<SceneProcessor>().CurrentState;
                var renderer = sceneState.Renderer;

                GraphicsDevice.Begin();

                GraphicsDevice.ClearState();

                // Update global time
                GraphicsDevice.Parameters.Set(GlobalKeys.Time, (float)gameTime.Total.TotalSeconds);
                GraphicsDevice.Parameters.Set(GlobalKeys.TimeStep, (float)gameTime.Elapsed.TotalSeconds);

                if (GraphicsDevice.IsProfilingSupported)
                {
                    GraphicsDevice.EnableProfile(true);
                }

                renderer.Draw(drawContext);
            }
            catch (Exception ex)
            {
                Log.Error("An exception occured while rendering", ex);
            }
            finally
            {
                GraphicsDevice.End();
            }


            if (entitySystem != null)
            {
                entitySystem.Draw(gameTime);
            }
        }

        internal EntitySystem CreateSceneEntitySystem(Entity sceneEntity)
        {
            // When a scene root is used for an entity system, 
            var newEntitySystem = new EntitySystem(Services) { AutoRegisterDefaultProcessors = true };
            newEntitySystem.Processors.Add(new SceneProcessor(sceneEntity));
            newEntitySystem.Add(sceneEntity);
            return newEntitySystem;
        }
    }
}