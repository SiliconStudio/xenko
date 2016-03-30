// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.


using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Background;
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
        private RenderContext renderContext;
        private RenderDrawContext renderDrawContext;

        /// <summary>
        /// The main render frame of the scene system
        /// </summary>
        public RenderFrame MainRenderFrame { get; set; }

        private int previousWidth;
        private int previousHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSystemBase" /> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <remarks>The GameSystem is expecting the following services to be registered: <see cref="IGame" /> and <see cref="IAssetManager" />.</remarks>
        public SceneSystem(IServiceRegistry registry)
            : base(registry)
        {
            registry.AddService(typeof(SceneSystem), this);
            Enabled = true;
            Visible = true;
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

        protected override void LoadContent()
        {
            var assetManager = Services.GetSafeServiceAs<ContentManager>();
            var graphicsContext = Services.GetSafeServiceAs<GraphicsContext>();

            // Preload the scene if it exists
            if (InitialSceneUrl != null && assetManager.Exists(InitialSceneUrl))
            {
                SceneInstance = new SceneInstance(Services, assetManager.Load<Scene>(InitialSceneUrl));
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
            if (SceneInstance != null)
            {
                SceneInstance.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (SceneInstance == null || MainRenderFrame == null)
            {
                return;
            }

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
            SceneInstance.Draw(renderDrawContext, MainRenderFrame);
        }
    }
}