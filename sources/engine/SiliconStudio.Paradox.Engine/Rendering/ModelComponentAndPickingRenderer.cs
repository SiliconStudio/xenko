// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// The main renderer for <see cref="ModelComponent"/>.
    /// </summary>
    public class ModelComponentAndPickingRenderer : EntityComponentRendererBase
    {
        private ModelComponentRenderer modelRenderer;
        private bool isPickingRendering;

        private static readonly ShaderMixinGeneratorSource PickingEffect = new ShaderMixinGeneratorSource("ModelComponentPickingEffect");

        public override bool SupportPicking { get { return true; } }

        public ModelComponentAndPickingRenderer()
        {
            modelRenderer = new ModelComponentRenderer();
        }

        public ModelComponentRenderer ModelRenderer
        {
            get
            {
                return modelRenderer;
            }
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            if (SceneCameraRenderer == null)
            {
                return;
            }

            var forwardMode = SceneCameraRenderer.Mode;
            var effectName =  forwardMode.ModelEffect;
            if (effectName == null)
            {
                throw new InvalidOperationException("ModelEffect cannot be null");
            }

            isPickingRendering = Context.IsPicking();
            if (isPickingRendering)
            {
                effectName += ".Picking";
                Context.Parameters.Set(ParadoxEffectBaseKeys.ExtensionPostVertexStageShader, PickingEffect);
            }

            modelRenderer.EffectName = effectName;
            modelRenderer = ToLoadAndUnload(modelRenderer);

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