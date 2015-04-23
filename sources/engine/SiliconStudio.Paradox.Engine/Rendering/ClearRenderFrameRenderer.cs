// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// A renderer to clear a render frame.
    /// </summary>
    [DataContract("ClearRenderFrameRenderer")]
    [Display("Clear RenderFrame")]
    public sealed class ClearRenderFrameRenderer : SceneRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClearRenderFrameRenderer"/> class.
        /// </summary>
        public ClearRenderFrameRenderer()
        {
            Name = "Clear RenderFrame";
            ClearFlags = ClearRenderFrameFlags.Color;
            Color = Core.Mathematics.Color.CornflowerBlue;
            Depth = 1.0f;
            Stencil = 0;
        }

        /// <summary>
        /// Gets or sets the clear flags.
        /// </summary>
        /// <value>The clear flags.</value>
        [DataMember(10)]
        [DefaultValue(ClearRenderFrameFlags.Color)]
        [Display("Clear Flags")]
        public ClearRenderFrameFlags ClearFlags { get; set; }

        /// <summary>
        /// Gets or sets the clear color.
        /// </summary>
        /// <value>The clear color.</value>
        [DataMember(20)]
        [Display("Color")]
        public Color4 Color { get; set; }

        /// <summary>
        /// Gets or sets the depth value used to clear the depth stencil buffer.
        /// </summary>
        /// <value>
        /// The depth value used to clear the depth stencil buffer.
        /// </value>
        [DataMember(30)]
        [DefaultValue(1.0f)]
        [Display("Depth Value")]
        public float Depth { get; set; }

        /// <summary>
        /// Gets or sets the stencil value used to clear the depth stencil buffer.
        /// </summary>
        /// <value>
        /// The stencil value used to clear the depth stencil buffer.
        /// </value>
        [DataMember(40)]
        [DefaultValue(0)]
        [Display("Stencil Value")]
        public byte Stencil { get; set; }

        protected override void DrawCore(RenderContext context, RenderFrame output)
        {
            var graphicsDevice = context.GraphicsDevice;

            // clear the targets
            if (output.DepthStencil != null)
            {
                const DepthStencilClearOptions ClearOptions = DepthStencilClearOptions.DepthBuffer | DepthStencilClearOptions.Stencil;
                graphicsDevice.Clear(output.DepthStencil, ClearOptions, Depth, Stencil);
            }

            if (ClearFlags == ClearRenderFrameFlags.Color)
            {
                foreach (var renderTarget in output.RenderTargets)
                {
                    if (renderTarget != null)
                    {
                        graphicsDevice.Clear(renderTarget, Color);
                    }
                }
            }
        }
    }
}