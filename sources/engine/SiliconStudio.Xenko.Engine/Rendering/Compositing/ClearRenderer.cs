// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// A renderer to clear a render frame.
    /// </summary>
    [Display("Clear")]
    public class ClearRenderer : SceneRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClearRenderer"/> class.
        /// </summary>
        public ClearRenderer()
        {
            Name = "Clear";
            ClearFlags = ClearRendererFlags.ColorAndDepth;
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
        [DefaultValue(ClearRendererFlags.ColorAndDepth)]
        [Display("Clear Flags")]
        public ClearRendererFlags ClearFlags { get; set; }

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

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            var commandList = drawContext.CommandList;

            var depthStencil = commandList.DepthStencilBuffer;

            // clear the targets
            if (depthStencil != null && (ClearFlags == ClearRendererFlags.ColorAndDepth || ClearFlags == ClearRendererFlags.DepthOnly))
            {
                var clearOptions = DepthStencilClearOptions.DepthBuffer | DepthStencilClearOptions.Stencil;

                commandList.Clear(depthStencil, clearOptions, Depth, Stencil);
            }

            if (ClearFlags == ClearRendererFlags.ColorAndDepth || ClearFlags == ClearRendererFlags.ColorOnly)
            {
                for (var index = 0; index < commandList.RenderTargetCount; index++)
                {
                    var renderTarget = commandList.RenderTargets[index];
                    var color = Color.ToColorSpace(drawContext.GraphicsDevice.ColorSpace);
                    commandList.Clear(renderTarget, color);
                }
            }
        }
    }
}
