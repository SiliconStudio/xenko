// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Font;
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
            Services.AddService(typeof(IContentManager), ContentManager);
            Services.AddService(typeof(ContentManager), ContentManager);

            var renderingSettings = gameSettings.GetOrCreate<RenderingSettings>();
            GraphicsDevice = GraphicsDevice.New(DeviceCreationFlags.Debug, new[] { renderingSettings.DefaultGraphicsProfile });

            var graphicsDeviceService = new GraphicsDeviceServiceLocal(Services, GraphicsDevice);
            Services.AddService(typeof(IGraphicsDeviceService), graphicsDeviceService);
            EffectSystem = new EffectSystem(Services);
            Services.AddService(typeof(EffectSystem), EffectSystem);

            GraphicsContext = new GraphicsContext(GraphicsDevice);
            Services.AddService(typeof(GraphicsContext), GraphicsContext);

            SceneSystem = new SceneSystem(Services);
            Services.AddService(typeof(SceneSystem), SceneSystem);

            // Create game systems
            GameSystems = new GameSystemCollection(Services);
            Services.AddService(typeof(IGameSystemCollection), GameSystems);

            var gameFontSystem = new GameFontSystem(Services);
            Services.AddService(typeof(FontSystem), gameFontSystem.FontSystem);
            Services.AddService(typeof(IFontFactory), gameFontSystem.FontSystem);
            GameSystems.Add(gameFontSystem);

            var uiSystem = new UISystem(Services);
            Services.AddService(typeof(UISystem), uiSystem);
            GameSystems.Add(uiSystem);

            GameSystems.Add(EffectSystem);
            GameSystems.Add(SceneSystem);
            GameSystems.Initialize();

            // Fake presenter
            // TODO GRAPHICS REFACTOR: This is needed be for render stage setup
            GraphicsDevice.Presenter = new RenderTargetGraphicsPresenter(GraphicsDevice,
                Texture.New2D(GraphicsDevice, renderingSettings.DefaultBackBufferWidth, renderingSettings.DefaultBackBufferHeight,
                    renderingSettings.ColorSpace == ColorSpace.Linear ? PixelFormat.R8G8B8A8_UNorm_SRgb : PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget),
                PixelFormat.D24_UNorm_S8_UInt);

            GraphicsContext.CommandList.SetRenderTarget(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
        }

        public void Dispose()
        {
            GraphicsDevice.Presenter.Dispose();
            GameSystems.Dispose();
            GraphicsDevice.Dispose();
        }
    }
}
