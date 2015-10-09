// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// The main renderer for <see cref="LightComponent"/>.
    /// </summary>
    public class LightComponentRenderer : EntityComponentRendererBase
    {
        private LightComponentForwardRenderer lightComponentForwardRenderer;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            // TODO: restrict to forward mode only for now
            if (SceneCameraRenderer == null)
            {
                return;
            }

            var forwardMode = SceneCameraRenderer.Mode as CameraRendererModeForward;
            if (forwardMode != null)
            {
                lightComponentForwardRenderer = ToLoadAndUnload(new LightComponentForwardRenderer());
            }
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            if (lightComponentForwardRenderer != null)
            {
                lightComponentForwardRenderer.Draw(context);
            }
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
        }
    }
}