// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Engine.Graphics.Composers;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// An effect renderer for a scene.
    /// </summary>
    [DataContract("SceneEffectRenderer")]
    [Display("Render Effect")]
    public class SceneEffectRenderer : SceneRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneEffectRenderer"/> class.
        /// </summary>
        public SceneEffectRenderer()
        {
            Input = new LayerInputFrameProvider();
            Output = new CurrentRenderFrameProvider();
        }

        /// <summary>
        /// Gets or sets the input of this effect.
        /// </summary>
        /// <value>The input.</value>
        [DataMember(10)]
        [NotNull]
        public IImageEffectRendererInput Input { get; set; }

        /// <summary>
        /// Gets or sets the output of this effect
        /// </summary>
        /// <value>The output.</value>
        [DataMember(10)]
        [NotNull]
        public IRenderFrameOutput Output { get; set; }

        [DataMember(40)]
        [Display("Effect", AlwaysExpand = true)]
        public IImageEffectRenderer Effect { get; set; }

        protected override void DrawCore(RenderContext context)
        {
            // If Input or Output are null, early exit
            if (Input == null || Output == null)
            {
                return;
            }

            var input = Input.GetRenderFrame(context);
            var output = Output.GetRenderFrame(context);

            // If RenderFrame input or output are null, we can't do anything
            if (input == null || output == null)
            {
                return;
            }

            // If no effect found, just copy passthrough from input to output.
            var effect = (IImageEffect)Effect ?? context.GetSharedEffect<ImageScaler>();

            effect.SetInput(0, input);
            if (input.DepthStencil != null)
            {
                effect.SetInput(1, input.DepthStencil);
            }
            effect.SetOutput(output);
            effect.Draw(context);
        }
    }
}