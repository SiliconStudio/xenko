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
            Viewport = new RectangleF(0, 0, 100f, 100f);
            IsViewportInPercentage = true;
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
        /// Gets or sets the viewport in percentage or pixel.
        /// </summary>
        /// <value>The viewport in percentage or pixel.</value>
        [DataMember(40)]
        public RectangleF Viewport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the viewport is in fixed pixels instead of percentage.
        /// </summary>
        /// <value><c>true</c> if the viewport is in pixels instead of percentage; otherwise, <c>false</c>.</value>
        /// <userdoc>When this value is true, the Viewport size is a percentage (0-100) calculated relatively to the size of the Output, else it is a fixed size in pixels.</userdoc>
        [DataMember(50)]
        [DefaultValue(true)]
        [Display("Viewport in percentage?")]
        public bool IsViewportInPercentage { get; set; }

        protected override void DrawCore(RenderContext context, RenderFrame output)
        {
            // Early exit if some properties are null
            if (Mode == null || Camera == null)
            {
                return;
            }

            // Setup the render target
            context.GraphicsDevice.SetDepthAndRenderTarget(output.DepthStencil, output.RenderTarget);

            Viewport viewport;
            var rect = Viewport;
            // Setup the viewport
            if (IsViewportInPercentage)
            {
                var width = output.RenderTarget.Width;
                var height = output.RenderTarget.Height;
                viewport = new Viewport((int)(rect.X * width / 100.0f), (int)(rect.Y * height / 100.0f), (int)(rect.Width * width / 100.0f), (int)(rect.Height * height / 100.0f));
            }
            else
            {
                viewport = new Viewport((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            }

            context.GraphicsDevice.SetViewport(viewport);

            // Draw this camera.
            using (var t1 = context.PushTagAndRestore(Current, this))
                Mode.Draw(context);
        }
    }
}