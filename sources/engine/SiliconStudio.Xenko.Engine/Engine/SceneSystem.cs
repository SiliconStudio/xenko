// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// The scene system handles the scenes of a game.
    /// </summary>
    public class SceneSystem : GameSystemBase
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("SceneSystem");

        private RenderContext renderContext;
        private RenderDrawContext renderDrawContext;

        private int previousWidth;
        private int previousHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSystemBase" /> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <remarks>The GameSystem is expecting the following services to be registered: <see cref="IGame" /> and <see cref="IContentManager" />.</remarks>
        public SceneSystem(IServiceRegistry registry)
            : base(registry)
        {
            registry.AddService(typeof(SceneSystem), this);
            Enabled = true;
            Visible = true;
            GraphicsCompositor = new GraphicsCompositor();
        }

        /// <summary>
        /// Gets or sets the root scene.
        /// </summary>
        /// <value>The scene.</value>
        /// <exception cref="System.ArgumentNullException">Scene cannot be null</exception>
        public SceneInstance SceneInstance { get; set; }

        /// <summary>
        /// URL of the initial scene that should be used upon loading
        /// </summary>
        public string InitialSceneUrl { get; set; }

        /// <summary>
        /// URL of the initial graphics compositor that should be used upon loading
        /// </summary>
        public string InitialGraphicsCompositorUrl { get; set; }

        /// <summary>
        /// URL of the splash screen texture that should be used upon loading
        /// </summary>
        public string SplashScreenUrl { get; set; }

        public GraphicsCompositor GraphicsCompositor { get; set; }

        private Task<Scene> sceneTask;
        private Task<GraphicsCompositor> compositorTask;

        private const double MinSplashScreenTime = 3.0f;

        private Texture splashScreenTexture;

        protected override void LoadContent()
        {
            var content = Services.GetSafeServiceAs<ContentManager>();
            var graphicsContext = Services.GetSafeServiceAs<GraphicsContext>();

            // Preload the scene if it exists and show splash screen
            if (InitialSceneUrl != null && content.Exists(InitialSceneUrl))
            {
                sceneTask = content.LoadAsync<Scene>(InitialSceneUrl);
            }

            if (InitialGraphicsCompositorUrl != null && content.Exists(InitialGraphicsCompositorUrl))
            {
                compositorTask = content.LoadAsync<GraphicsCompositor>(InitialGraphicsCompositorUrl);
            }

            if (SplashScreenUrl != null && content.Exists(SplashScreenUrl))
            {
                splashScreenTexture = content.Load<Texture>(SplashScreenUrl);
            }

            // Create the drawing context
            renderContext = RenderContext.GetShared(Services);
            renderDrawContext = new RenderDrawContext(Services, renderContext, graphicsContext);
        }

        protected override void Destroy()
        {
            if (SceneInstance != null)
            {
                ((IReferencable)SceneInstance).Release();
                SceneInstance = null;
            }

            base.Destroy();
        }

        public override void Update(GameTime gameTime)
        {
            // Execute Update step of SceneInstance
            // This will run entity processors
            SceneInstance?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Reset the context
            renderContext.Reset();

            var renderTarget = renderDrawContext.CommandList.RenderTarget;

            // If the width or height changed, we have to recycle all temporary allocated resources.
            // NOTE: We assume that they are mostly resolution dependent.
            if (previousWidth != renderTarget.ViewWidth || previousHeight != renderTarget.ViewHeight)
            {
                // Force a recycle of all allocated temporary textures
                renderContext.Allocator.Recycle(link => true);
            }

            previousWidth = renderTarget.ViewWidth;
            previousHeight = renderTarget.ViewHeight;

            // Update the entities at draw time.
            renderContext.Time = gameTime;

            // Execute Draw step of SceneInstance
            // This will run entity processors
            SceneInstance?.Draw(renderContext);

            // Render phase
            // TODO GRAPHICS REFACTOR
            //context.GraphicsDevice.Parameters.Set(GlobalKeys.Time, (float)gameTime.Total.TotalSeconds);
            //context.GraphicsDevice.Parameters.Set(GlobalKeys.TimeStep, (float)gameTime.Elapsed.TotalSeconds);

            // Push context (pop after using)
            using (renderDrawContext.RenderContext.PushTagAndRestore(SceneInstance.Current, SceneInstance))
            {
                GraphicsCompositor?.Draw(renderDrawContext);
            }

            //do this here, make sure GC and Scene are updated/rendered the next frame!
            if (sceneTask != null && compositorTask != null)
            {
                if (gameTime.Total.TotalSeconds > MinSplashScreenTime || splashScreenTexture == null) //load asap if no splash screen is here
                {
                    if (sceneTask.IsCompleted && compositorTask.IsCompleted)
                    {
                        SceneInstance = new SceneInstance(Services, sceneTask.Result);
                        GraphicsCompositor = compositorTask.Result;
                        sceneTask = null;
                        compositorTask = null;

                        if (splashScreenTexture != null)
                        {
                            var content = Services.GetSafeServiceAs<ContentManager>();
                            content.Unload(splashScreenTexture);
                            splashScreenTexture = null;
                        }
                    }
                }

                if (splashScreenTexture != null)
                {
                    int width;
                    int height;
                    if (Game.GraphicsContext.CommandList.RenderTarget.Height > Game.GraphicsContext.CommandList.RenderTarget.Width) //portrait
                    {
                        width = height = Game.GraphicsContext.CommandList.RenderTarget.Width;
                    }
                    else //landscape
                    {
                        width = height = Game.GraphicsContext.CommandList.RenderTarget.Height;
                    }

                    var viewport = Game.GraphicsContext.CommandList.Viewport;

                    var x = -width / 2;
                    var y = -height / 2;
                    x += Game.GraphicsContext.CommandList.RenderTarget.Width / 2;
                    y += Game.GraphicsContext.CommandList.RenderTarget.Height / 2;
                    Game.GraphicsContext.CommandList.SetViewport(new Viewport(x, y, width, height));

                    Game.GraphicsContext.DrawTexture(splashScreenTexture);

                    Game.GraphicsContext.CommandList.SetViewport(viewport);
                }
            }
        }
    }
}
