// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.


using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Background;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Rendering.Skyboxes;
using SiliconStudio.Xenko.Rendering.Sprites;
using ShaderMixins = SiliconStudio.Xenko.Rendering.ShaderMixins;

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
        /// The main render frame of the scene system
        /// </summary>
        public RenderFrame MainRenderFrame { get; set; }

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
            graphicsCompositor = new GraphicsCompositor();
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

        public string InitialGraphicsCompositorUrl { get; set; }

        [Obsolete]
        public ISceneGraphicsCompositor GraphicsCompositor
        {
            get { return graphicsCompositor.Instance; }
            set { graphicsCompositor.Instance = value; }
        }

        public GraphicsCompositor NewGraphicsCompositor
        {
            get { return graphicsCompositor; }
            set { graphicsCompositor = value; }
        }

        private GraphicsCompositor graphicsCompositor;

        protected override void LoadContent()
        {
            var assetManager = Services.GetSafeServiceAs<ContentManager>();
            var graphicsContext = Services.GetSafeServiceAs<GraphicsContext>();

            // Preload the scene if it exists
            if (InitialSceneUrl != null && assetManager.Exists(InitialSceneUrl))
            {
                SceneInstance = new SceneInstance(Services, assetManager.Load<Scene>(InitialSceneUrl));
            }

            if (InitialGraphicsCompositorUrl != null && assetManager.Exists(InitialGraphicsCompositorUrl))
            {
                graphicsCompositor = assetManager.Load<GraphicsCompositor>(InitialGraphicsCompositorUrl);
            }

            if (MainRenderFrame == null)
            {
                // TODO GRAPHICS REFACTOR Check if this is a good idea to use Presenter targets
                MainRenderFrame = RenderFrame.FromTexture(GraphicsDevice.Presenter?.BackBuffer, GraphicsDevice.Presenter?.DepthStencilBuffer);
                if (MainRenderFrame != null)
                {
                    previousWidth = MainRenderFrame.Width;
                    previousHeight = MainRenderFrame.Height;
                }
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
            SceneInstance?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (SceneInstance == null || MainRenderFrame == null)
            {
                return;
            }

            // Reset the context
            renderContext.Reset();

            // If the width or height changed, we have to recycle all temporary allocated resources.
            // NOTE: We assume that they are mostly resolution dependent.
            if (previousWidth != MainRenderFrame.Width || previousHeight != MainRenderFrame.Height)
            {
                // Force a recycle of all allocated temporary textures
                renderContext.Allocator.Recycle(link => true);
            }

            previousWidth = MainRenderFrame.Width;
            previousHeight = MainRenderFrame.Height;

            // Update the entities at draw time.
            renderContext.Time = gameTime;
            SceneInstance.Draw(renderContext);

            // Render phase
            // TODO GRAPHICS REFACTOR
            //context.GraphicsDevice.Parameters.Set(GlobalKeys.Time, (float)gameTime.Total.TotalSeconds);
            //context.GraphicsDevice.Parameters.Set(GlobalKeys.TimeStep, (float)gameTime.Elapsed.TotalSeconds);

            try
            {
                // Always clear the state of the GraphicsDevice to make sure a scene doesn't start with a wrong setup 
                renderDrawContext.CommandList.ClearState();

                // Push context (pop after using)
                using (renderDrawContext.RenderContext.PushTagAndRestore(RenderFrame.Current, MainRenderFrame))
                using (renderDrawContext.RenderContext.PushTagAndRestore(SceneInstance.Current, SceneInstance))
                {
                    NewGraphicsCompositor?.Draw(renderDrawContext);
                }
            }
            catch (Exception ex)
            {
                Log.Error("An exception occurred while rendering", ex);
            }
        }
    }
}
