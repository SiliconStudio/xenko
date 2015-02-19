// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine.Graphics.Composers;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// Base implementation for a <see cref="ISceneRenderer"/>.
    /// </summary>
    public abstract class SceneRendererBase : RendererBase, ISceneRenderer
    {
        protected SceneRendererBase()
        {
            Output = new CurrentRenderFrameProvider();
        }

        [DataMember(100)]
        public ISceneRendererOutput Output { get; set; }

        protected override void DrawCore(RenderContext context)
        {
            var output = Output.GetSafeRenderFrame(context);
            if (output != null)
            {
                DrawCore(context, output);
            }
        }

        protected abstract void DrawCore(RenderContext context, RenderFrame output);

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