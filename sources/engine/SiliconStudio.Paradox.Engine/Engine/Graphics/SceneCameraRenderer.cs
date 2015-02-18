// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A camera renderer.
    /// </summary>
    [DataContract("SceneCameraRenderer")]
    [Display("Render Camera")]
    public sealed class SceneCameraRenderer : SceneRendererBase
    {
        /// <summary>
        /// Property key to access the current <see cref="SceneCameraRenderer"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<SceneCameraRenderer> Current = new PropertyKey<SceneCameraRenderer>("SceneCameraRenderer.Current", typeof(SceneCameraRenderer));

        // TODO: Add option for Occlusion culling
        // TODO: Add support for fixed aspect ratio and auto-centered-viewport

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCameraRenderer"/> class.
        /// </summary>
        public SceneCameraRenderer()
        {
            Mode = new CameraRendererModeForward();
            CullingMask = EntityGroup.All;
            Viewport = new RectangleF(0, 0, 1.0f, 1.0f);
        }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        [DataMember(10)]
        [NotNull]
        public CameraRendererMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        /// <value>The camera.</value>
        [DataMember(20)]
        public CameraComponent Camera { get; set; }

        /// <summary>
        /// Gets or sets the culling mask.
        /// </summary>
        /// <value>The culling mask.</value>
        [DataMember(30)]
        [DefaultValue(EntityGroup.All)]
        public EntityGroup CullingMask { get; set; }

        /// <summary>
        /// Gets or sets the viewport.
        /// </summary>
        /// <value>The viewport.</value>
        [DataMember(40)]
        public RectangleF Viewport { get; set; }

        protected override void DrawCore(RenderContext context)
        {
            var output = Output.GetSafeRenderFrame(context);

            // If RenderFrame output is null, we early exit
            if (Mode == null || output == null)
            {
                return;
            }

            // Setup the render target
            context.GraphicsDevice.SetDepthAndRenderTarget(output.DepthStencil, output.RenderTarget);

            // Setup the viewport
            var width = output.RenderTarget.Width;
            var height = output.RenderTarget.Height;

            var viewport = new Viewport((int)(Viewport.X * width), (int)(Viewport.Y * height), (int)(Viewport.Width * width), (int)(Viewport.Height * height));
            context.GraphicsDevice.SetViewport(viewport);

            // Draw this camera.
            using (var t1 = context.PushTagAndRestore(Current, this))
                Mode.Draw(context);
        }
    }
}