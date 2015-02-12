// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Engine.Graphics.Composers;
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

        private RenderContext renderContext;

        private EntityManager entityManager;

        private SceneProcessor sceneProcessor;

        private Scene scene;

        private RenderFrame mainRenderFrame;

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
        public EntityManager EntityManager
        {
            get
            {
                return entityManager;
            }
        }

        /// <summary>
        /// Gets or sets the root scene.
        /// </summary>
        /// <value>The scene.</value>
        /// <exception cref="System.ArgumentNullException">Scene cannot be null</exception>
        public Scene Scene
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
                        entityManager.Remove(scene);
                    }

                    entityManager = CreateSceneEntitySystem(value);
                    sceneProcessor = entityManager.GetProcessor<SceneProcessor>();
                    scene = value;
                }
            }
        }

        protected override void LoadContent()
        {
            var assetManager = Services.GetSafeServiceAs<AssetManager>();

            // TODO: Temp work around for PreviewGame init
            //    // Preload the scene if it exists
            //    if (assetManager.Exists(DefaultSceneName))
            //    {
            //        Scene = assetManager.Load<Scene>(DefaultSceneName);
            //    }

            mainRenderFrame = RenderFrame.FromTexture(GraphicsDevice.BackBuffer, GraphicsDevice.DepthStencilBuffer);

            // Create the drawing context
            renderContext = RenderContext.GetShared(Services);
        }

        public override void Update(GameTime gameTime)
        {
            if (EntityManager != null)
            {
                EntityManager.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (EntityManager == null)
            {
                return;
            }

            // Update global time
            renderContext.Tags.Set(GameTime.Current, gameTime);
            GraphicsDevice.Parameters.Set(GlobalKeys.Time, (float)gameTime.Total.TotalSeconds);
            GraphicsDevice.Parameters.Set(GlobalKeys.TimeStep, (float)gameTime.Elapsed.TotalSeconds);

            // Draw the scene
            Draw(renderContext, sceneProcessor.CurrentState, mainRenderFrame);
        }

        public static void Draw(RenderContext context, SceneInstance sceneInstance, RenderFrame toFrame, ISceneGraphicsCompositor compositorOverload = null)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (sceneInstance == null) throw new ArgumentNullException("sceneInstance");
            if (toFrame == null) throw new ArgumentNullException("toFrame");

            var graphicsDevice = context.GraphicsDevice;

            bool hasGraphicsBegin = false;

            try
            {
                graphicsDevice.Begin();
                hasGraphicsBegin = true;

                graphicsDevice.ClearState();

                // Update the render context to use the main RenderFrame as current by default
                context.Tags.Set(RenderFrame.Current, toFrame);
                context.Tags.Set(SceneGraphicsLayer.Master, context.Tags.GetSafe(RenderFrame.Current));
                context.Tags.Set(EntityManager.Current, sceneInstance.EntityManager);
                context.Tags.Set(CameraRendererMode.RendererTypesKey, sceneInstance.RendererTypes);

                // Draw the main scene.
                var graphicsCompositor = compositorOverload ?? sceneInstance.Scene.Settings.GraphicsCompositor;
                if (graphicsCompositor != null)
                {
                    graphicsCompositor.Draw(context);
                }
            }
            catch (Exception ex)
            {
                Log.Error("An exception occured while rendering", ex);
            }
            finally
            {
                if (hasGraphicsBegin)
                {
                    graphicsDevice.End();
                }
            }
        }

        internal EntityManager CreateSceneEntitySystem(Scene sceneEntity)
        {
            // When a scene root is used for an entity system, 
            var newEntitySystem = new EntityManager(Services) { AutoRegisterDefaultProcessors = true };
            newEntitySystem.Processors.Add(new SceneProcessor(sceneEntity));
            newEntitySystem.Add(sceneEntity);
            return newEntitySystem;
        }
    }
}