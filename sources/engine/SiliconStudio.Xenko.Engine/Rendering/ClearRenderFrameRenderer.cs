// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
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
            ClearFlags = ClearRenderFrameFlags.ColorAndDepth;
            Color = Core.Mathematics.Color.CornflowerBlue;
            Depth = 1.0f;
            Stencil = 0;
        }

        /// <summary>
        /// Gets or sets the clear flags.
        /// </summary>
        /// <value>The clear flags.</value>
        /// <userdoc>Flag indicating which buffers to clear.</userdoc>
        [DataMember(10)]
        [DefaultValue(ClearRenderFrameFlags.ColorAndDepth)]
        [Display("Clear Flags")]
        public ClearRenderFrameFlags ClearFlags { get; set; }

        /// <summary>
        /// Gets or sets the clear color.
        /// </summary>
        /// <value>The clear color.</value>
        /// <userdoc>The color value to use when clearing the render targets</userdoc>
        [DataMember(20)]
        [Display("Color")]
        public Color4 Color { get; set; }

        /// <summary>
        /// Gets or sets the depth value used to clear the depth stencil buffer.
        /// </summary>
        /// <value>
        /// The depth value used to clear the depth stencil buffer.
        /// </value>
        /// <userdoc>The depth value to use when clearing the depth buffer</userdoc>
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
        /// <userdoc>The stencil value to use when clearing the stencil buffer</userdoc>
        [DataMember(40)]
        [DefaultValue(0)]
        [Display("Stencil Value")]
        public byte Stencil { get; set; }

        protected override void DrawCore(RenderDrawContext context, RenderFrame output)
        {
            var commandList = context.CommandList;

            // clear the targets
            if (output.DepthStencil != null && (ClearFlags == ClearRenderFrameFlags.ColorAndDepth || ClearFlags == ClearRenderFrameFlags.DepthOnly))
            {
                var clearOptions = DepthStencilClearOptions.DepthBuffer;
                if (output.DepthStencil.HasStencil)
                    clearOptions |= DepthStencilClearOptions.Stencil;

                commandList.Clear(output.DepthStencil, clearOptions, Depth, Stencil);
            }

            if (ClearFlags == ClearRenderFrameFlags.ColorAndDepth || ClearFlags == ClearRenderFrameFlags.ColorOnly)
            {
                foreach (var renderTarget in output.RenderTargets)
                {
                    if (renderTarget != null)
                    {
                        var color = Color.ToColorSpace(context.GraphicsDevice.ColorSpace);
                        commandList.Clear(renderTarget, color);
                    }
                }
            }
        }
    }
}