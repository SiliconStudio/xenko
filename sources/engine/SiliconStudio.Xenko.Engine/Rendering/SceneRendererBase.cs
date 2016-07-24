// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Base implementation for a <see cref="ISceneRenderer"/>.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class SceneRendererBase : RendererBase, ISceneRenderer
    {
        protected SceneRendererBase()
        {
            Output = new CurrentRenderFrameProvider();
            ResetGraphicsStates = true;
        }

        /// <summary>
        /// Gets or sets the output of the scene renderer
        /// </summary>
        /// <userdoc>Specify the render frame to use as output of the scene renderer</userdoc>
        [DataMember(100)]
        [NotNull]
        public ISceneRendererOutput Output { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to reset the graphics states after this scene renderer is executed.
        /// </summary>
        /// <value><c>true</c> to reset the graphics states after this scene renderer is executed.; otherwise, <c>false</c>.</value>
        /// <userdoc>If this option is selected, the graphics states (blend, depth...etc.) are reseted after this scene renderer is executed.</userdoc>
        [Display("Reset Graphics States?")]
        [DataMember(110)]
        [DefaultValue(true)]
        public bool ResetGraphicsStates { get; set; }

        /// <param name="context"></param>
        /// <inheritdoc/>
        public virtual void Collect(RenderContext context)
        {
            EnsureContext(context);
        }

        /// <summary>
        /// Gets the current output <see cref="RenderFrame"/> output.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>RenderFrame.</returns>
        public RenderFrame GetOutput(RenderContext context)
        {
            return Output.GetSafeRenderFrame(context);
        }

        /// <summary>
        /// Activates the output to the current <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="disableDepth">if set to <c>true</c> [disable depth].</param>
        public void ActivateOutput(RenderDrawContext context, bool disableDepth = false)
        {
            var output = GetOutput(context.RenderContext);
            if (output != null)
            {
                ActivateOutputCore(context, output, disableDepth);
            }
        }

        /// <summary>
        /// Activates the output to the current <see cref="GraphicsDevice" />.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="output">The output.</param>
        /// <param name="disableDepth">if set to <c>true</c> [disable depth].</param>
        protected virtual void ActivateOutputCore(RenderDrawContext context, RenderFrame output, bool disableDepth)
        {
            // Set default render target states
            foreach (var renderTarget in output.RenderTargets)
            {
                context.CommandList.ResourceBarrierTransition(renderTarget, GraphicsResourceState.RenderTarget);
            }
            context.CommandList.ResourceBarrierTransition(output.DepthStencil, GraphicsResourceState.DepthWrite);

            // Setup the render target
            context.CommandList.SetRenderTargetsAndViewport(disableDepth ? null : output.DepthStencil, output.RenderTargets);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var output = GetOutput(context.RenderContext);
            if (output != null)
            {
                try
                {
                    // TODO GRAPHICS REFACTOR
                    //context.PushParameters(Parameters);

                    ActivateOutput(context);

                    DrawCore(context, output);
                }
                finally
                {
                    // TODO GRAPHICS REFACTOR
                    //context.PopParameters();

                    if (ResetGraphicsStates)
                    {
                        // Make sure that states are clean after this rendering
                        // TODO GRAPHICS REFACTOR
                        //context.GraphicsDevice.ResetStates();
                    }
                }
            }
        }

        protected abstract void DrawCore(RenderDrawContext context, RenderFrame output);

        protected override void Destroy()
        {
            if (Output != null)
            {
                Output.Dispose();
                Output = null;
            }

            base.Destroy();
        }
    }
}