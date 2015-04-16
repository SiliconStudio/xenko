// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// The main renderer for <see cref="ModelComponent"/>.
    /// </summary>
    public class ModelComponentAndPickingRenderer : EntityComponentRendererBase
    {
        private ModelComponentRenderer modelRenderer;
        private bool isPickingRendering;

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

            modelRenderer = ToLoadAndUnload(new ModelComponentRenderer(effectName));

            // Setup the ModelComponentRenderer as the main renderer for the scene Camera Renderer
            // This is used by the LightComponentForwardRenderer
            // TODO: Check if we could discover declared renderers in a better way than just hacking the tags of a component
            ModelComponentRenderer.Attach(SceneCameraRenderer, modelRenderer);
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            modelRenderer.Prepare(context, opaqueList, transparentList);
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            modelRenderer.Draw(context, renderItems, fromIndex, toIndex);
        }
    }
}