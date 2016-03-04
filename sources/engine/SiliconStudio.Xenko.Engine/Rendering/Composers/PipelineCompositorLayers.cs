// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    [DataContract]
    public abstract class PipelineCompositorLayers : CompositorLayersBase
    {
        /// <summary>
        /// Gets the render system used with this pipeline.
        /// </summary>
        [DataMemberIgnore]
        public NextGenRenderSystem RenderSystem { get; } = new NextGenRenderSystem();

        /// <summary>
        /// Gets or sets the effect to use to render the models in the scene.
        /// </summary>
        /// <value>The main model effect.</value>
        /// <userdoc>The name of the effect to use to render models (a '.xksl' or '.xkfx' filename without the extension).</userdoc>
        [DataMember(10)]
        public string ModelEffect
        {
            // TODO: This is not a good extensibility point. Check how to improve this
            get { return RenderSystem.PipelinePlugins.GetPlugin<MeshPipelinePlugin>().ModelEffect; }
            set { RenderSystem.PipelinePlugins.GetPlugin<MeshPipelinePlugin>().ModelEffect = value; }
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem.Initialize(Context);
        }

        protected override void Destroy()
        {
            RenderSystem.Dispose();

            base.Destroy();
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            // Get or create VisibilityGroup for this RenderSystem
            var sceneInstance = SceneInstance.GetCurrent(context.RenderContext);
            var visibilityGroup = sceneInstance.GetOrCreateVisibilityGroup(RenderSystem);

            using (context.RenderContext.PushTagAndRestore(SceneInstance.CurrentVisibilityGroup, visibilityGroup))
            using (context.RenderContext.PushTagAndRestore(SceneInstance.CurrentRenderSystem, RenderSystem))
            {
                // Draw the layers
                Layers.BeforeExtract(context.RenderContext);

                // Draw the master track
                Master.BeforeExtract(context.RenderContext);

                // Update current camera to render view
                // TODO GRAPHICS REFACTOR: Collecte and update views every frome
                foreach (var mainRenderView in RenderSystem.Views)
                {
                    RenderSystem.UpdateCameraToRenderView(context, mainRenderView);
                }

                // TODO GRAPHICS REFACTOR: Should happen somewhere like RenderFeature.BeforeExtract
                RenderSystem.forwardLightingRenderFeature?.BeforeExtract();

                // Collect
                // TODO GRAPHICS REFACTOR choose which views to collect
                foreach (var view in RenderSystem.Views)
                    visibilityGroup.Views.Add(view);
                visibilityGroup.Collect();
                visibilityGroup.Views.Clear();

                // Extract
                RenderSystem.Extract(context);

                // Prepare
                RenderSystem.Prepare(context);

                // Draw the layers
                Layers.Draw(context);

                // Draw the master track
                Master.Draw(context);

                // Reset render context data
                RenderSystem.Reset();
            }
        }
    }
}