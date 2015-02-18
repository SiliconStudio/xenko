// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Images;

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
        }

        /// <summary>
        /// Gets or sets the input of this effect.
        /// </summary>
        /// <value>The input.</value>
        [DataMember(10)]
        [NotNull]
        public IImageEffectRendererInput Input { get; set; }

        [DataMember(20)]
        [Display("Effect", AlwaysExpand = true)]
        public IImageEffectRenderer Effect { get; set; }

        protected override void Destroy()
        {
            if (Input != null)
            {
                Input.Dispose();
                Input = null;
            }

            base.Destroy();
        }

        protected override void DrawCore(RenderContext context)
        {
            var input = Input.GetSafeRenderFrame(context);
            var output = Output.GetSafeRenderFrame(context);

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