// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Lights;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// The main renderer for <see cref="ModelComponent"/> and <see cref="LightComponent"/>.
    /// </summary>
    public class ModelAndLightComponentRenderer : EntityComponentRendererBase
    {
        private LightModelRendererForward lightModelRenderer;
        private ModelComponentRenderer modelRenderer;

        public override void Load(RenderContext context)
        {
            base.Load(context);

            // TODO: Add support for mixin overrides
            modelRenderer = ToLoadAndUnload(new ModelComponentRenderer(SceneCameraRenderer.Mode.GetMainModelEffect()));
            lightModelRenderer = new LightModelRendererForward(modelRenderer);
        }

        protected override void DrawCore(RenderContext context)
        {
            // TODO: Add support for shadows

            // TODO: We call it directly here but it might be plugged into 
            lightModelRenderer.PrepareLights(context);

            // TODO: Add support for transparent materials
            // Draw models
            modelRenderer.CullingMask = SceneCameraRenderer.CullingMask;
            modelRenderer.Draw(context);
        }
    }
}