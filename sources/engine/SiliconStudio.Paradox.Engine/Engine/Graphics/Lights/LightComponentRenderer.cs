// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Engine.Graphics;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// The main renderer for <see cref="LightComponent"/>.
    /// </summary>
    public class LightComponentRenderer : EntityComponentRendererBase
    {
        private LightModelRendererForward lightModelRendererForward;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            // TODO: restrict to forward mode only for now
            var forwardMode = SceneCameraRenderer.Mode as CameraRendererModeForward;
            if (forwardMode != null)
            {
                lightModelRendererForward = ToLoadAndUnload(new LightModelRendererForward());
            }
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            if (lightModelRendererForward != null)
            {
                lightModelRendererForward.Draw(context);
            }
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
        }
    }
}