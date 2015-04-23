// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Images;

namespace SiliconStudio.Paradox.Rendering
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

        protected override void DrawCore(RenderContext context, RenderFrame output)
        {
            var input = Input.GetSafeRenderFrame(context);

            // If RenderFrame input or output are null, we can't do anything
            if (input == null)
            {
                return;
            }

            // If an effect is set, we are using it
            if (Effect != null)
            {
                Effect.SetInput(0, input);
                if (input.DepthStencil != null)
                {
                    Effect.SetInput(1, input.DepthStencil);
                }
                Effect.SetOutput(output);
                Effect.Draw(context);
            }
            else if (input != output)
            {
                // Else only use a scaler if input and output don't match
                // TODO: Is this something we want by default or we just don't output anything?
                var effect = context.GetSharedEffect<ImageScaler>();
                effect.SetInput(0, input);
                effect.SetOutput(output);
                effect.Draw(context);
            }
        }
    }
}