// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.


using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// The scene system handles the scenes of a game.
    /// </summary>
    public class SceneSystem : GameSystemBase
    {

        private RenderContext renderContext;

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
            AutoLoadDefaultScene = true;
        }

        /// <summary>
        /// Should we use a default scene upon loading.
        /// </summary>
        public bool AutoLoadDefaultScene { get; set; }

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

        public override void Initialize()
        {
            base.Initialize();

            // Load several default settings
            if (AutoLoadDefaultScene && Asset.Exists(GameSettings.AssetUrl))
            {
                var settings = Asset.Load<GameSettings>(GameSettings.AssetUrl);
                var deviceManager = (GraphicsDeviceManager)Services.GetSafeServiceAs<IGraphicsDeviceManager>();
                if (settings.DefaultGraphicsProfileUsed > 0) deviceManager.PreferredGraphicsProfile = new[] { settings.DefaultGraphicsProfileUsed };
                if (settings.DefaultBackBufferWidth > 0) deviceManager.PreferredBackBufferWidth = settings.DefaultBackBufferWidth;
                if (settings.DefaultBackBufferHeight > 0) deviceManager.PreferredBackBufferHeight = settings.DefaultBackBufferHeight;
                InitialSceneUrl = settings.DefaultSceneUrl;
            }
        }

        protected override void LoadContent()
        {
            var assetManager = Services.GetSafeServiceAs<AssetManager>();

            // Preload the scene if it exists
            if (AutoLoadDefaultScene && InitialSceneUrl != null && assetManager.Exists(InitialSceneUrl))
            {
                SceneInstance = new SceneInstance(Services, assetManager.Load<Scene>(InitialSceneUrl));
            }

            if (MainRenderFrame == null)
            {
                MainRenderFrame = RenderFrame.FromTexture(GraphicsDevice.BackBuffer, GraphicsDevice.DepthStencilBuffer);
                previousWidth = MainRenderFrame.Width;
                previousHeight = MainRenderFrame.Height;
            }

            // Create the drawing context
            renderContext = RenderContext.GetShared(Services);
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
            if (SceneInstance == null)
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

            // Renders the scene
            SceneInstance.Draw(renderContext, MainRenderFrame);
        }
    }
}