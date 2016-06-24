using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Assets;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Fonts;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets
{
    /// <summary>
    /// Helper class to render the scene.
    /// </summary>
    struct SceneRenderer : IDisposable
    {
        /// <summary>
        /// The service registry.
        /// </summary>
        public readonly ServiceRegistry Services;

        /// <summary>
        /// The list of game systems.
        /// </summary>
        public readonly GameSystemCollection GameSystems;

        /// <summary>
        /// The current effect system.
        /// </summary>
        public readonly EffectSystem EffectSystem;

        /// <summary>
        ///  The content manager to load content.
        /// </summary>
        public readonly ContentManager ContentManager;

        /// <summary>
        /// The current scene system.
        /// </summary>
        public readonly SceneSystem SceneSystem;

        /// <summary>
        /// The graphics device.
        /// </summary>
        public readonly GraphicsDevice GraphicsDevice;

        /// <summary>
        /// The graphics context used during draw.
        /// </summary>
        public readonly GraphicsContext GraphicsContext;

        public SceneRenderer(GameSettingsAsset gameSettings)
        {
            if (gameSettings == null) throw new ArgumentNullException(nameof(gameSettings));

            // Initialize services
            Services = new ServiceRegistry();
            ContentManager = new ContentManager(Services);

            var renderingSettings = gameSettings.Get<RenderingSettings>();
            GraphicsDevice = GraphicsDevice.New(DeviceCreationFlags.None, new[] { renderingSettings.DefaultGraphicsProfile });

            var graphicsDeviceService = new GraphicsDeviceServiceLocal(Services, GraphicsDevice);
            EffectSystem = new EffectSystem(Services);
            GraphicsContext = new GraphicsContext(new CommandList(GraphicsDevice), new ResourceGroupAllocator(GraphicsDevice));
            Services.AddService(typeof(GraphicsContext), GraphicsContext);

            SceneSystem = new SceneSystem(Services);

            // Create game systems
            GameSystems = new GameSystemCollection(Services);
            GameSystems.Add(new GameFontSystem(Services));
            GameSystems.Add(new UISystem(Services));
            GameSystems.Add(EffectSystem);
            GameSystems.Add(SceneSystem);
            GameSystems.Initialize();

            // Fake presenter
            // TODO GRAPHICS REFACTOR: This is needed be for render stage setup
            GraphicsDevice.Presenter = new RenderTargetGraphicsPresenter(GraphicsDevice,
                Texture.New2D(GraphicsDevice, renderingSettings.DefaultBackBufferWidth, renderingSettings.DefaultBackBufferHeight,
                    renderingSettings.ColorSpace == ColorSpace.Linear ? PixelFormat.R8G8B8A8_UNorm_SRgb : PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget),
                PixelFormat.D24_UNorm_S8_UInt);

            SceneSystem.MainRenderFrame = RenderFrame.FromTexture(GraphicsDevice.Presenter.BackBuffer, GraphicsDevice.Presenter.DepthStencilBuffer);
        }

        public void Dispose()
        {
            GraphicsDevice.Presenter.Dispose();
            GameSystems.Dispose();
        }
    }
}