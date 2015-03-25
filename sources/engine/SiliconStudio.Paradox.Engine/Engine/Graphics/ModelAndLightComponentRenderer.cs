// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.Effects.Shadows;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// The main renderer for <see cref="ModelComponent"/> and <see cref="LightComponent"/>.
    /// </summary>
    public class ModelAndLightComponentRenderer : EntityComponentRendererBase
    {
        private LightModelRendererForward lightModelRenderer;
        private ModelComponentRenderer modelRenderer;
        private bool isPickingRendering;

        private SceneCameraRenderer SceneCameraRenderer { get {  return (SceneCameraRenderer)SceneEntityRenderer; } }

        public override bool SupportPicking { get { return true; } }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            var forwardMode = SceneCameraRenderer.Mode as CameraRendererModeForward;
            var effectName =  (forwardMode != null? forwardMode.ModelEffect: null) ?? "";
            isPickingRendering = Context.IsPicking();
            if (isPickingRendering)
            {
                effectName += ".Picking";
                Context.Parameters.Set(ParadoxEffectBaseKeys.ExtensionPostVertexStageShader, "ModelComponentPickingEffect");
            }

            // TODO: Add support for mixin overrides
            modelRenderer = ToLoadAndUnload(new ModelComponentRenderer(effectName));
            if (!isPickingRendering)
            {
                lightModelRenderer = new LightModelRendererForward(modelRenderer);
            }
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            if (!isPickingRendering)
            {
                lightModelRenderer.PrepareLights(context);
            }
            modelRenderer.Prepare(context, opaqueList, transparentList);
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            modelRenderer.Draw(context, renderItems, fromIndex, toIndex);
        }
    }
}