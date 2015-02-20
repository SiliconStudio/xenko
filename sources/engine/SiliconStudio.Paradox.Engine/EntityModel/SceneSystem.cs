// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
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
        private const string DefaultSceneName = "__DefaultScene__"; // TODO: How to determine the default scene?

        private RenderContext renderContext;

        private RenderFrame mainRenderFrame;

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
            previousWidth = mainRenderFrame.RenderTarget.Width;
            previousHeight = mainRenderFrame.RenderTarget.Height;

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
            if (previousWidth != mainRenderFrame.RenderTarget.Width || previousHeight != mainRenderFrame.RenderTarget.Height)
            {
                // Force a recycle of all allocated temporary textures
                renderContext.Allocator.Recycle(link => true);
            }

            previousWidth = mainRenderFrame.RenderTarget.Width;
            previousHeight = mainRenderFrame.RenderTarget.Height;

            // TODO: Clear camera states. This is not highly customizable
            renderContext.ClearCameraStates();

            // Update the entities at draw time.
            SceneInstance.Draw(renderContext);

            // Renders the scene
            renderContext.Time = gameTime;
            SceneInstance.Draw(renderContext, mainRenderFrame);
        }
    }
}