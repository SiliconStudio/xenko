// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// A renderer which can provide <see cref="RendererBase.Draw"/> implementation with a delegate.
    /// </summary>
    public partial class DelegateSceneRenderer : SceneRendererBase
    {
        private readonly Action<RenderDrawContext> drawAction;

        public DelegateSceneRenderer(Action<RenderDrawContext> drawAction)
        {
            this.drawAction = drawAction;
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            drawAction(drawContext);
        }
    }
}
